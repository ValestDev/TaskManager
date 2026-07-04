import { useState, useEffect, useCallback } from 'react';
import { Plus, Pencil, Lock } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import Layout from '../components/Layout';
import apiClient from '../api/client';

const STATUS_LABELS = {
  Pendiente: 'Pendiente',
  EnProgreso: 'En progreso',
  Finalizada: 'Finalizada',
};

const STATUS_STYLES = {
  Pendiente: 'bg-amber-50 text-amber-700',
  EnProgreso: 'bg-blue-50 text-blue-700',
  Finalizada: 'bg-emerald-50 text-emerald-700',
};

export default function TasksPage() {
  const { user, isAdmin } = useAuth();

  const [tasks, setTasks] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [statusFilter, setStatusFilter] = useState('');
  const [onlyMine, setOnlyMine] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const [activeUsers, setActiveUsers] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  const totalPages = Math.ceil(total / pageSize) || 1;

  const loadTasks = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await apiClient.get('/tasks', {
        params: {
          page,
          pageSize,
          status: statusFilter || undefined,
          onlyMine: onlyMine || undefined,
        },
      });
      setTasks(response.data.items);
      setTotal(response.data.total);
    } catch (err) {
      setError('No se pudieron cargar las tareas.');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, statusFilter, onlyMine]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  useEffect(() => {
    apiClient
      .get('/users/active')
      .then((res) => setActiveUsers(res.data))
      .catch(() => setActiveUsers([]));
  }, []);

  const canEdit = (task) =>
    isAdmin || task.createdById === user?.userId || task.assignedToId === user?.userId;

  const openCreateModal = () => {
    setEditingTask(null);
    setShowModal(true);
  };

  const openEditModal = (task) => {
    setEditingTask(task);
    setShowModal(true);
  };

  const handleModalClose = (shouldReload) => {
    setShowModal(false);
    setEditingTask(null);
    if (shouldReload) loadTasks();
  };

  return (
    <Layout>
      <div className="p-8 max-w-6xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Tareas</h1>
            <p className="text-gray-500 mt-1 text-sm">{total} tareas en total</p>
          </div>
          <button
            onClick={openCreateModal}
            className="flex items-center gap-2 bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-4 py-2.5 rounded-xl font-medium shadow-sm hover:shadow-md transition"
          >
            <Plus size={18} />
            Nueva tarea
          </button>
        </div>

        <div className="flex items-center gap-4 mb-5 flex-wrap">
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="border border-gray-200 rounded-xl px-3.5 py-2.5 bg-white focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
          >
            <option value="">Todos los estados</option>
            <option value="Pendiente">Pendiente</option>
            <option value="EnProgreso">En progreso</option>
            <option value="Finalizada">Finalizada</option>
          </select>

          <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={onlyMine}
              onChange={(e) => { setOnlyMine(e.target.checked); setPage(1); }}
              className="w-4 h-4 rounded accent-emerald-600"
            />
            Solo mis tareas asignadas
          </label>
        </div>

        {error && (
          <div className="mb-4 bg-red-50 border border-red-100 text-red-600 text-sm rounded-xl p-3">
            {error}
          </div>
        )}

        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-50 text-xs uppercase tracking-wide text-gray-500">
              <tr>
                <th className="p-4">Tarea</th>
                <th className="p-4">Estado</th>
                <th className="p-4">Asignado a</th>
                <th className="p-4">Creado por</th>
                <th className="p-4 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {loading && (
                <tr><td colSpan={5} className="p-8 text-center text-gray-400">Cargando...</td></tr>
              )}
              {!loading && tasks.length === 0 && (
                <tr><td colSpan={5} className="p-8 text-center text-gray-400">No se encontraron tareas.</td></tr>
              )}
              {!loading && tasks.map((t) => (
                <tr key={t.id} className="hover:bg-gray-50/50 transition">
                  <td className="p-4">
                    <p className="font-medium text-gray-800">{t.title}</p>
                    {t.description && (
                      <p className="text-sm text-gray-500 truncate max-w-xs">{t.description}</p>
                    )}
                  </td>
                  <td className="p-4">
                    <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${STATUS_STYLES[t.status]}`}>
                      {STATUS_LABELS[t.status]}
                    </span>
                  </td>
                  <td className="p-4 text-gray-600">{t.assignedToName || '—'}</td>
                  <td className="p-4 text-gray-600">{t.createdByName}</td>
                  <td className="p-4">
                    <div className="flex justify-end">
                      {canEdit(t) ? (
                        <button
                          onClick={() => openEditModal(t)}
                          title="Editar"
                          className="p-2 text-gray-500 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition"
                        >
                          <Pencil size={16} />
                        </button>
                      ) : (
                        <span title="Sin permiso para editar" className="p-2 text-gray-300">
                          <Lock size={16} />
                        </span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="flex justify-between items-center mt-5">
          <span className="text-sm text-gray-500">
            Página {page} de {totalPages}
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              className="px-3.5 py-2 text-sm border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-gray-50 transition"
            >
              Anterior
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              className="px-3.5 py-2 text-sm border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-gray-50 transition"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>

      {showModal && (
        <TaskFormModal task={editingTask} activeUsers={activeUsers} onClose={handleModalClose} />
      )}
    </Layout>
  );
}

function TaskFormModal({ task, activeUsers, onClose }) {
  const isEditing = !!task;
  const [title, setTitle] = useState(task?.title || '');
  const [description, setDescription] = useState(task?.description || '');
  const [status, setStatus] = useState(task?.status || 'Pendiente');
  const [assignedToId, setAssignedToId] = useState(task?.assignedToId || '');
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSaving(true);

    try {
      const payload = {
        title,
        description: description || null,
        assignedToId: assignedToId || null,
      };

      if (isEditing) {
        await apiClient.put(`/tasks/${task.id}`, { ...payload, status });
      } else {
        await apiClient.post('/tasks', payload);
      }
      onClose(true);
    } catch (err) {
      setError(err.response?.data?.error || 'Error al guardar la tarea.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-gray-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <h2 className="text-lg font-bold text-gray-800 mb-5">
          {isEditing ? 'Editar tarea' : 'Nueva tarea'}
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Título</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              className="w-full border border-gray-200 rounded-xl px-3.5 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Descripción</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className="w-full border border-gray-200 rounded-xl px-3.5 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent resize-none"
            />
          </div>

          {isEditing && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Estado</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full border border-gray-200 rounded-xl px-3.5 py-2.5 bg-white focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
              >
                <option value="Pendiente">Pendiente</option>
                <option value="EnProgreso">En progreso</option>
                <option value="Finalizada">Finalizada</option>
              </select>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">Asignar a</label>
            <select
              value={assignedToId}
              onChange={(e) => setAssignedToId(e.target.value)}
              className="w-full border border-gray-200 rounded-xl px-3.5 py-2.5 bg-white focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
            >
              <option value="">Sin asignar</option>
              {activeUsers.map((u) => (
                <option key={u.id} value={u.id}>{u.name}</option>
              ))}
            </select>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-100 text-red-600 text-sm rounded-xl p-3">
              {error}
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={() => onClose(false)}
              className="px-4 py-2.5 rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2.5 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 text-white font-medium hover:shadow-md disabled:opacity-50 transition"
            >
              {saving ? 'Guardando...' : 'Guardar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}