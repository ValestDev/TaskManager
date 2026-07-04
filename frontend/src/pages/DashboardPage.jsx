import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import apiClient from '../api/client';
import { getPresenceConnection, stopPresenceConnection } from '../api/signalr';

export default function DashboardPage() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const [onlineUsers, setOnlineUsers] = useState([]);

  useEffect(() => {
    let isMounted = true;

    async function loadOnlineUsers() {
      try {
        const response = await apiClient.get('/auth/online-users');
        if (isMounted) {
          setOnlineUsers(response.data);
        }
      } catch (err) {
        console.error('Error al cargar usuarios online:', err);
      }
    }

    loadOnlineUsers();

    const connection = getPresenceConnection();

    connection.on('UserOnline', (data) => {
      setOnlineUsers((prev) => {
        const withoutDuplicate = prev.filter((u) => u.userId !== data.userId);
        return [...withoutDuplicate, data];
      });
    });

    connection.on('UserOffline', (data) => {
      setOnlineUsers((prev) => prev.filter((u) => u.userId !== data.userId));
    });

    if (connection.state === 'Disconnected') {
      connection.start().catch((err) => console.error('Error conectando a SignalR:', err));
    }

    return () => {
      isMounted = false;
      connection.off('UserOnline');
      connection.off('UserOffline');
    };
  }, []);

  const handleLogout = async () => {
    stopPresenceConnection();
    await logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm p-4 flex justify-between items-center">
        <h1 className="text-xl font-bold text-gray-800">Task Manager</h1>
        <div className="flex items-center gap-4">
          <span className="text-gray-600">{user?.name}</span>
          <button
            onClick={() => navigate('/tasks')}
            className="text-blue-600 hover:underline"
          >
            Tareas
          </button>
          {isAdmin && (
            <button
              onClick={() => navigate('/users')}
              className="text-blue-600 hover:underline"
            >
              Usuarios
            </button>
          )}
          <button
            onClick={handleLogout}
            className="bg-red-600 text-white px-3 py-1.5 rounded-md hover:bg-red-700 text-sm"
          >
            Cerrar sesión
          </button>
        </div>
      </nav>

      <main className="p-8">
        <h2 className="text-lg font-semibold text-gray-800 mb-4">
          Usuarios conectados ({onlineUsers.length})
        </h2>
        <div className="bg-white rounded-lg shadow-sm divide-y">
          {onlineUsers.length === 0 && (
            <p className="p-4 text-gray-500">No hay usuarios conectados.</p>
          )}
          {onlineUsers.map((u) => (
            <div key={u.userId} className="p-4 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 bg-green-500 rounded-full"></span>
                <span className="font-medium">{u.userName}</span>
              </div>
              <span className="text-sm text-gray-500">
                Conectado: {new Date(u.loginAt).toLocaleTimeString()}
              </span>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}