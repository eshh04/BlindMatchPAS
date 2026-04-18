import React from 'react';
import { api } from '../services/api';
import { Project, Supervisor, ResearchArea } from '../types';
import { Shield, User, FlaskConical, Search, AlertCircle, Eye, EyeOff } from 'lucide-react';

export default function Allocation() {
  const [data, setData] = React.useState<{ projects: Project[], supervisors: Supervisor[], researchAreas: ResearchArea[] } | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [updatingId, setUpdatingId] = React.useState<number | null>(null);
  const [searchTerm, setSearchTerm] = React.useState('');

  const fetchData = async () => {
    try {
      const res = await api.allocation.getMatrix();
      setData(res);
    } catch (err) {
      console.error('Failed to fetch allocation matrix', err);
    } finally {
      setIsLoading(false);
    }
  };

  React.useEffect(() => {
    fetchData();
  }, []);

  const handleAssign = async (projectId: number, supervisorId: number) => {
    setUpdatingId(projectId);
    try {
      await api.allocation.assign({ projectId, supervisorId });
      fetchData();
    } catch (err) {
      alert('Allocation Error');
    } finally {
      setUpdatingId(null);
    }
  };

  const handleToggleReveal = async (projectId: number) => {
    try {
      await api.allocation.toggleReveal(projectId);
      fetchData();
    } catch (err) {
      alert('Toggle Error');
    }
  };

  const filteredProjects = data?.projects.filter(p => 
    p.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.student?.name.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const getAreaName = (id: number) => data?.researchAreas.find(a => a.id === id)?.name || 'General';

  if (isLoading) return <div className="text-center p-20 animate-pulse text-text-dim uppercase tracking-[.3em]">Synapsing Projection Matrix...</div>;

  return (
    <div className="space-y-8 selection:bg-white selection:text-black">
      {/* Search Bar */}
      <div className="flex items-center gap-4 bg-surface border border-border px-6 py-4">
        <Search size={18} className="text-text-dim" />
        <input
          type="text"
          placeholder="Filter by Project Title or Student Name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="bg-transparent border-none focus:outline-none text-[13px] w-full uppercase tracking-widest"
        />
      </div>

      <div className="grid grid-cols-1 gap-6">
        {filteredProjects.map((project) => (
          <div key={project.id} className={`theme-card bg-surface p-8 flex flex-col lg:flex-row justify-between gap-8 border-l-4 transition-all ${project.isRevealed ? 'border-l-[#00FF00]' : 'border-l-zinc-700'}`}>
            <div className="flex-1 space-y-4">
              <div className="flex items-baseline gap-4">
                <span className="font-mono text-[10px] text-text-dim bg-white/5 px-2 py-0.5 border border-white/5">PRJ-{project.id.toString().padStart(4, '0')}</span>
                <h3 className="text-xl font-light tracking-tight">{project.title}</h3>
              </div>
              
              <div className="flex flex-wrap gap-6 items-center">
                <div className="group relative">
                  <div className="flex items-center gap-2 text-[11px] uppercase tracking-[0.2em]">
                    <User size={14} className={project.isRevealed ? 'text-[#00FF00]' : 'text-zinc-600'} />
                    <span className={project.isRevealed ? 'text-white' : 'text-zinc-600 font-mono italic'}>
                      {project.isRevealed ? project.student?.name : 'IDENTITY_ENCRYPTED'}
                    </span>
                  </div>
                  {!project.isRevealed && (
                    <div className="absolute -top-8 left-0 scale-0 group-hover:scale-100 transition-all bg-white text-black px-2 py-1 text-[9px] font-bold uppercase whitespace-nowrap">
                      Blind Match Protocol Active
                    </div>
                  )}
                </div>

                <div className="flex items-center gap-2 text-[11px] uppercase tracking-[0.2em] text-text-dim">
                  <FlaskConical size={14} />
                  {getAreaName(project.researchAreaId)}
                </div>
              </div>
            </div>

            <div className="flex flex-col sm:flex-row items-end lg:items-center gap-6">
              <div className="space-y-2 w-full sm:w-72">
                <label className="text-[9px] uppercase tracking-[0.3em] text-text-dim block ml-1">Assigned Supervisor</label>
                <select
                  title="Assign Supervisor"
                  disabled={updatingId === project.id}
                  value={project.supervisorId}
                  onChange={(e) => handleAssign(project.id, parseInt(e.target.value))}
                  className="w-full bg-bg border border-zinc-800 px-4 py-3 text-[12px] focus:outline-none focus:border-white disabled:opacity-50 transition-all"
                >
                  <option value="0">-- UNASSIGNED / OPEN --</option>
                  {data?.supervisors.map(s => (
                    <option key={s.id} value={s.id}>{s.name} [{s.department}]</option>
                  ))}
                </select>
              </div>

              <div className="flex items-center gap-3 h-full pt-6 sm:pt-0">
                <button
                  onClick={() => handleToggleReveal(project.id)}
                  title={project.isRevealed ? "Hide Identity" : "Reveal Identity"}
                  className={`p-3 border transition-all ${
                    project.isRevealed 
                      ? 'bg-[#00FF00]/10 border-[#00FF00] text-[#00FF00]' 
                      : 'border-zinc-800 text-zinc-600 hover:border-white hover:text-white'
                  }`}
                >
                  {project.isRevealed ? <Eye size={18} /> : <EyeOff size={18} />}
                </button>
              </div>
            </div>
          </div>
        ))}
        {filteredProjects.length === 0 && (
          <div className="p-20 text-center border-2 border-dashed border-zinc-900 text-text-dim uppercase tracking-[.4em] text-[10px]">
            No projects found // Matrix empty
          </div>
        )}
      </div>
    </div>
  );
}
