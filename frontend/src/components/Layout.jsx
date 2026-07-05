import { NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Users, ListTodo, LogOut } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { stopPresenceConnection } from '../api/signalr';

export default function Layout({ children }) {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    stopPresenceConnection();
    await logout();
    navigate('/login');
  };

  const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { to: '/tasks', label: 'Tareas', icon: ListTodo },
    ...(isAdmin ? [{ to: '/users', label: 'Usuarios', icon: Users }] : []),
  ];

  const initials = user?.name
    ?.split(' ')
    .map((n) => n[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();

  return (
    <div className="min-h-screen bg-gray-50 flex">
    <aside className="w-64 shrink-0 bg-gradient-to-b from-emerald-600 to-teal-700 text-white flex flex-col">
        <div className="p-6">
          <h1 className="text-xl font-bold tracking-tight">Task Manager</h1>
          <p className="text-emerald-200 text-xs mt-1">Gestión de tareas</p>
        </div>

        <nav className="flex-1 px-3 space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
                  isActive
                    ? 'bg-white/15 text-white'
                    : 'text-emerald-100 hover:bg-white/10'
                }`
              }
            >
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-white/10">
          <div className="flex items-center gap-3 mb-3">
            <div className="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center text-sm font-semibold">
              {initials}
            </div>
            <div className="min-w-0">
              <p className="text-sm font-medium truncate">{user?.name}</p>
              <p className="text-xs text-emerald-200 truncate">{user?.role === 'Admin' ? 'Administrador' : 'Usuario'}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-emerald-100 hover:bg-white/10 transition"
          >
            <LogOut size={16} />
            Cerrar sesión
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">{children}</main>
    </div>
  );
}