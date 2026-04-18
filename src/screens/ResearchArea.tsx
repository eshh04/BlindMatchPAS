import React from 'react';
import { api } from '../services/api';
import { ResearchArea as IResearchArea } from '../types';
import { Plus, Trash2 } from 'lucide-react';

export default function ResearchArea() {
  const [areas, setAreas] = React.useState<IResearchArea[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  const [newName, setNewName] = React.useState('');
  const [newDesc, setNewDesc] = React.useState('');

  const fetchAreas = async () => {
    try {
      const data = await api.researchAreas.getAll();
      setAreas(data);
    } catch (err) {
      console.error('Failed to fetch research areas', err);
    } finally {
      setIsLoading(false);
    }
  };

  React.useEffect(() => {
    fetchAreas();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName) return;
    try {
      await api.researchAreas.create({ name: newName, description: newDesc });
      setNewName('');
      setNewDesc('');
      fetchAreas();
    } catch (err) {
      alert('Failed to create research area');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure?')) return;
    try {
      await api.researchAreas.delete(id);
      fetchAreas();
    } catch (err) {
      alert('Failed to delete research area');
    }
  };

  return (
    <div className="space-y-10">
      {/* Create Form */}
      <section className="theme-card bg-surface p-6">
        <h2 className="theme-label mb-4">Register New Research Node</h2>
        <form onSubmit={handleCreate} className="flex flex-col sm:flex-row gap-4">
          <input
            type="text"
            placeholder="Area Name"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            className="flex-1 bg-bg border border-border px-4 py-2 text-[13px] focus:outline-none focus:border-accent"
          />
          <input
            type="text"
            placeholder="Description"
            value={newDesc}
            onChange={(e) => setNewDesc(e.target.value)}
            className="flex-[2] bg-bg border border-border px-4 py-2 text-[13px] focus:outline-none focus:border-accent"
          />
          <button
            type="submit"
            className="bg-accent text-bg px-6 py-2 text-[11px] font-bold uppercase tracking-widest hover:bg-white transition-colors"
          >
            Register
          </button>
        </form>
      </section>

      {/* Table Container */}
      <div className="table-container theme-border bg-surface overflow-hidden">
        <table className="w-full border-collapse text-left">
          <thead>
            <tr>
              <th className="p-4 px-6 theme-label border-b border-border w-16">ID</th>
              <th className="p-4 px-6 theme-label border-b border-border">Area Name</th>
              <th className="p-4 px-6 theme-label border-b border-border">Technical Description</th>
              <th className="p-4 px-6 theme-label border-b border-border text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={4} className="p-10 text-center theme-label animate-pulse">Synchronizing Data...</td></tr>
            ) : (
              areas.map((area) => (
                <tr key={area.id} className="border-b border-white/5 hover:bg-white/5 transition-colors">
                  <td className="p-4 px-6 text-[13px] font-mono text-text-dim">#{area.id.toString().padStart(4, '0')}</td>
                  <td className="p-4 px-6 text-[13px] font-bold tracking-tight">{area.name}</td>
                  <td className="p-4 px-6 text-[12px] text-text-dim uppercase tracking-wider">{area.description}</td>
                  <td className="p-4 px-6 text-right">
                    <button 
                      title="Delete Research Area"
                      onClick={() => handleDelete(area.id)}
                      className="text-text-dim hover:text-red-500 transition-colors p-2"
                    >
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
