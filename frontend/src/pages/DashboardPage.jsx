import { useState, useEffect } from 'react';
import { Users as UsersIcon, Circle } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import Layout from '../components/Layout';
import apiClient from '../api/client';
import { getPresenceConnection } from '../api/signalr';

export default function DashboardPage() {
  const { user } = useAuth();
  const [onlineUsers, setOnlineUsers] = useState([]);

  useEffect(() => {
    let isMounted = true;

    async function loadOnlineUsers() {
      try {
        const response = await apiClient.get('/auth/online-users');
        if (isMounted) setOnlineUsers(response.data);
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

  const getInitials = (name) =>
    name?.split(' ').map((n) => n[0]).slice(0, 2).join('').toUpperCase();

  return (
    <Layout>
      <div className="p-8 max-w-5xl mx-auto">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-800">
            Hola, {user?.name?.split(' ')[0]} 
          </h1>
          <p className="text-gray-500 mt-1">Esto es lo que está pasando ahora mismo.</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div className="bg-gradient-to-br from-emerald-500 to-teal-600 rounded-2xl p-5 text-white shadow-sm">
            <div className="flex items-center justify-between">
              <span className="text-emerald-100 text-sm font-medium">Conectados ahora</span>
              <UsersIcon size={20} className="text-emerald-100" />
            </div>
            <p className="text-3xl font-bold mt-2">{onlineUsers.length}</p>
          </div>
        </div>

        <div className="bg-white rounded-2xl shadow-sm border border-gray-100">
          <div className="p-5 border-b border-gray-100">
            <h2 className="font-semibold text-gray-800">Usuarios conectados</h2>
          </div>

          <div className="divide-y divide-gray-50">
            {onlineUsers.length === 0 && (
              <p className="p-6 text-gray-400 text-sm text-center">No hay usuarios conectados.</p>
            )}
            {onlineUsers.map((u) => (
              <div key={u.userId} className="p-4 flex items-center justify-between hover:bg-gray-50 transition">
                <div className="flex items-center gap-3">
                  <div className="relative">
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-teal-500 flex items-center justify-center text-white text-sm font-semibold">
                      {getInitials(u.userName)}
                    </div>
                    <Circle size={10} className="absolute -bottom-0.5 -right-0.5 fill-green-500 text-green-500 ring-2 ring-white rounded-full" />
                  </div>
                  <span className="font-medium text-gray-800">{u.userName}</span>
                </div>
                <span className="text-sm text-gray-400">
                  desde las {new Date(u.loginAt).toLocaleTimeString()}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </Layout>
  );
}