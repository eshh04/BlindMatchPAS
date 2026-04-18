import React from 'react';
import { api } from '../services/api';
import { Supervisor, Student } from '../types';
import { Trash2, Users, GraduationCap, Plus, Mail, Building } from 'lucide-react';

type UserType = 'SUPERVISOR' | 'STUDENT';

export default function UserManagement() {
  const [activeTab, setActiveTab] = React.useState<UserType>('SUPERVISOR');
  const [supervisors, setSupervisors] = React.useState<Supervisor[]>([]);
  const [students, setStudents] = React.useState<Student[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  
  // Registration Form State
  const [name, setName] = React.useState('');
  const [email, setEmail] = React.useState('');
  const [password, setPassword] = React.useState('');
  const [department, setDepartment] = React.useState('');

  const fetchData = async () => {
    setIsLoading(true);
    try {
      if (activeTab === 'SUPERVISOR') {
        const res = await api.supervisors.getAll();
        setSupervisors(res);
      } else {
        const res = await api.students.getAll();
        setStudents(res);
      }
    } catch (err) {
      console.error('Failed to sync users', err);
    } finally {
      setIsLoading(false);
    }
  };

  React.useEffect(() => {
    fetchData();
  }, [activeTab]);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !email || !password) return;

    try {
      const data = { name, email, password, department };
      if (activeTab === 'SUPERVISOR') {
        await api.supervisors.create(data);
      } else {
        await api.students.create(data);
      }
      
      // Reset form
      setName('');
      setEmail('');
      setPassword('');
      setDepartment('');
      fetchData();
    } catch (err) {
      alert('Registration failed');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Permanently redact this user from the system?')) return;
    try {
      if (activeTab === 'SUPERVISOR') {
        await api.supervisors.delete(id);
      } else {
        await api.students.delete(id);
      }
      fetchData();
    } catch (err) {
      alert('Redaction error');
    }
  };

  return (
    <div className="space-y-10 selection:bg-white selection:text-black">
      {/* Role Switcher */}
      <div className="flex gap-4 border-b border-white/10 pb-4">
        <button
          onClick={() => setActiveTab('SUPERVISOR')}
          className={`flex items-center gap-2 px-6 py-2 text-[11px] uppercase tracking-[0.2em] transition-all ${
            activeTab === 'SUPERVISOR' ? 'text-white border-b-2 border-white' : 'text-text-dim hover:text-white'
          }`}
        >
          <Users size={14} />
          Supervisors
        </button>
        <button
          onClick={() => setActiveTab('STUDENT')}
          className={`flex items-center gap-2 px-6 py-2 text-[11px] uppercase tracking-[0.2em] transition-all ${
            activeTab === 'STUDENT' ? 'text-white border-b-2 border-white' : 'text-text-dim hover:text-white'
          }`}
        >
          <GraduationCap size={14} />
          Students
        </button>
      </div>

      {/* Registration Form */}
      <section className="theme-card bg-surface p-8 border-l-2 border-l-accent">
        <h2 className="text-[10px] uppercase tracking-[0.3em] text-text-dim mb-6">
          Register New {activeTab === 'SUPERVISOR' ? 'Personnel' : 'Scholar'} Account
        </h2>
        <form onSubmit={handleRegister} className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <input
            type="text"
            placeholder="Identity Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="bg-bg border border-border px-4 py-3 text-[12px] focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="email"
            placeholder="Official Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="bg-bg border border-border px-4 py-3 text-[12px] focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="password"
            placeholder="Access Key"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="bg-bg border border-border px-4 py-3 text-[12px] focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="text"
            placeholder="Unit / Dept"
            value={department}
            onChange={(e) => setDepartment(e.target.value)}
            className="bg-bg border border-border px-4 py-3 text-[12px] focus:outline-none focus:border-white transition-colors"
          />
          <button
            type="submit"
            className="bg-white text-black text-[10px] font-bold uppercase tracking-[0.2em] hover:bg-accent transition-colors"
          >
            Authorize
          </button>
        </form>
      </section>

      {/* Data Grid */}
      <div className="table-container theme-border bg-surface overflow-hidden">
        <table className="w-full border-collapse text-left">
          <thead>
            <tr className="bg-white/5">
              <th className="p-5 px-8 theme-label border-b border-border">Identity</th>
              <th className="p-5 px-8 theme-label border-b border-border">Communication</th>
              <th className="p-5 px-8 theme-label border-b border-border">Department</th>
              <th className="p-5 px-8 theme-label border-b border-border text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={4} className="p-20 text-center theme-label animate-pulse uppercase tracking-widest">
                  Synchronizing Neural Links...
                </td>
              </tr>
            ) : (
              (activeTab === 'SUPERVISOR' ? supervisors : students).map((user) => (
                <tr key={user.id} className="border-b border-white/5 hover:bg-white/5 transition-all group">
                  <td className="p-5 px-8">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-white/5 flex items-center justify-center text-[10px] font-mono">
                        {user.name.charAt(0)}
                      </div>
                      <span className="text-[13px] font-bold tracking-tight">{user.name}</span>
                    </div>
                  </td>
                  <td className="p-5 px-8 text-[12px] text-text-dim font-mono">{user.email}</td>
                  <td className="p-5 px-8">
                    <span className="px-3 py-1 bg-white/5 text-[10px] uppercase tracking-widest rounded-sm border border-white/10">
                      {user.department || 'GENERAL'}
                    </span>
                  </td>
                  <td className="p-5 px-8 text-right">
                    <button
                      title="Delete User"
                      onClick={() => handleDelete(user.id)}
                      className="opacity-0 group-hover:opacity-100 text-text-dim hover:text-red-500 transition-all p-2"
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
