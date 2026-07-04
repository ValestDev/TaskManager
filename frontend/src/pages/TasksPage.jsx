import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import apiClient from '../api/client';

const STATUS_LABELS = {
  Pendiente: 'Pendiente',
  EnProgreso: 'En progreso',
  Finalizada: 'Finalizada',
};

const STATUS_COLORS = {
  Pendiente: 'bg-yellow-100 text-yellow-700',
  EnProgreso: 'bg-blue-100 text-blue-700',
  Finalizada: 'bg-green-100 text-green-700',
};

export default function TasksPage() {
  const navigate = useNavigate();
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
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm p-4 flex justify-between items-center">
        <h1 className="text-xl font-bold text-gray-800">Tareas</h1>
        <button
          onClick={() => navigate('/dashboard')}
          className="text-blue-600 hover:underline"
        >
          ← Volver al dashboard
        </button>
      </nav>

      <main className="p-8">
        <div className="flex justify-between items-center mb-4 flex-wrap gap-3">
          <div className="flex items-center gap-3">
            <select
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
              className="border border-gray-300 rounded-md px-3 py-2"
            >
              <option value="">Todos los estados</option>
              <option value="Pendiente">Pendiente</option>
              <option value="EnProgreso">En progreso</option>
              <option value="Finalizada">Finalizada</option>
            </select>

            <label className="flex items-center gap-2 text-sm text-gray-700">
              <input
                type="checkbox"
                checked={onlyMine}
                onChange={(e) => { setOnlyMine(e.target.checked); setPage(1); }}
              />
              Solo mis tareas asignadas
            </label>
          </div>

          <button
            onClick={openCreateModal}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
          >
            + Nueva tarea
          </button>
        </div>

        {error && <p className="text-red-600 mb-4">{error}</p>}

        <div className="bg-white rounded-lg shadow-sm overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-100 text-sm text-gray-600">
              <tr>
                <th className="p-3">Título</th>
                <th className="p-3">Estado</th>
                <th className="p-3">Asignado a</th>
                <th className="p-3">Creado por</th>
                <th className="p-3">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {loading && (
                <tr><td colSpan={5} className="p-4 text-center text-gray-500">Cargando...</td></tr>
              )}
              {!loading && tasks.length === 0 && (
                <tr><td colSpan={5} className="p-4 text-center text-gray-500">No se encontraron tareas.</td></tr>
              )}
              {!loading && tasks.map((t) => (
                <tr key={t.id}>
                  <td className="p-3">{t.title}</td>
                  <td className="p-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs ${STATUS_COLORS[t.status]}`}>
                      {STATUS_LABELS[t.status]}
                    </span>
                  </td>
                  <td className="p-3">{t.assignedToName || '—'}</td>
                  <td className="p-3">{t.createdByName}</td>
                  <td className="p-3">
                    {canEdit(t) ? (
                      <button
                        onClick={() => openEditModal(t)}
                        className="text-blue-600 hover:underline text-sm"
                      >
                        Editar
                      </button>
                    ) : (
                      <span className="text-gray-400 text-sm">Sin permiso</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="flex justify-between items-center mt-4">
          <span className="text-sm text-gray-600">
            Página {page} de {totalPages} — {total} tareas
          </span>
          <div className="space-x-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              className="px-3 py-1 border rounded-md disabled:opacity-40"
            >
              Anterior
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              className="px-3 py-1 border rounded-md disabled:opacity-40"
            >
              Siguiente
            </button>
          </div>
        </div>
      </main>

      {showModal && (
        <TaskFormModal task={editingTask} activeUsers={activeUsers} onClose={handleModalClose} />
      )}
    </div>
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
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-md p-6">
        <h2 className="text-lg font-bold mb-4">
          {isEditing ? 'Editar tarea' : 'Nueva tarea'}
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Título</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className="w-full border border-gray-300 rounded-md px-3 py-2"
            />
          </div>

          {isEditing && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full border border-gray-300 rounded-md px-3 py-2"
              >
                <option value="Pendiente">Pendiente</option>
                <option value="EnProgreso">En progreso</option>
                <option value="Finalizada">Finalizada</option>
              </select>
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Asignar a</label>
            <select
              value={assignedToId}
              onChange={(e) => setAssignedToId(e.target.value)}
              className="w-full border border-gray-300 rounded-md px-3 py-2"
            >
              <option value="">Sin asignar</option>
              {activeUsers.map((u) => (
                <option key={u.id} value={u.id}>{u.name}</option>
              ))}
            </select>
          </div>

          {error && <p className="text-red-600 text-sm">{error}</p>}

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={() => onClose(false)}
              className="px-4 py-2 rounded-md border hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 rounded-md bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? 'Guardando...' : 'Guardar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}