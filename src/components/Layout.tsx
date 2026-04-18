import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, FlaskConical, Table, Users, TrendingUp, LogOut } from 'lucide-react';
import { api } from '../services/api';

interface LayoutProps {
  children: React.ReactNode;
  title?: string;
}

export default function Layout({ children, title }: LayoutProps) {
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await api.admin.logout();
      navigate('/');
    } catch (err) {
      navigate('/');
    }
  };

  const navItems = [
    { icon: LayoutDashboard, label: 'Overview', path: '/dashboard' },
    { icon: FlaskConical, label: 'Research', path: '/research' },
    { icon: Table, label: 'Allocation', path: '/allocation' },
    { icon: Users, label: 'Personnel', path: '/users' },
    { icon: TrendingUp, label: 'Analytics', path: '/analytics' },
  ];

  return (
    <div className="flex h-screen overflow-hidden bg-bg text-text-main font-sans selection:bg-white selection:text-black">
      {/* Sidebar */}
      <aside className="w-[240px] h-full border-r border-border p-10 px-6 flex flex-col shrink-0 bg-surface/50">
        <div className="brand font-mono text-[11px] font-bold tracking-[0.3em] mb-[60px] flex items-center gap-[10px]">
          <span className="w-2 h-2 bg-red-dot rounded-full animate-pulse"></span>
          BLIND_MATCH // PAS
        </div>
        
        <nav className="flex-1">
          <ul className="list-none space-y-8">
            {navItems.map((item) => (
              <li key={item.path}>
                <NavLink
                  to={item.path}
                  className={({ isActive }) =>
                    `text-[11px] uppercase tracking-[0.2em] transition-all duration-300 flex items-center gap-3 ${
                      isActive
                        ? 'text-white font-bold translate-x-1'
                        : 'text-text-dim hover:text-white hover:translate-x-1'
                    }`
                  }
                >
                  <item.icon size={14} />
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="mt-auto pt-6 border-t border-border/50">
          <button
            onClick={handleLogout}
            className="text-[11px] uppercase tracking-[0.2em] text-text-dim hover:text-red-500 transition-all duration-300 flex items-center gap-3"
          >
            <LogOut size={14} />
            Termination
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 bg-bg">
        {/* Top Header */}
        <header className="flex justify-between items-baseline px-[60px] pt-12 pb-8 shrink-0">
          <h1 className="text-[32px] font-extralight tracking-[-0.04em] uppercase">{title || 'System'}</h1>
          <div className="current-time font-mono text-[10px] text-text-dim tracking-widest flex items-center gap-4">
            <span className="flex items-center gap-2">
              <span className="w-1 h-1 bg-[#00FF00] rounded-full"></span>
              CORE_STABLE
            </span>
            <span>SYS_{new Date().toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}</span>
          </div>
        </header>

        {/* Scrollable Content */}
        <main className="flex-1 overflow-y-auto px-[60px] pb-10 custom-scrollbar">
          {children}
          
          {/* Footer Summary */}
          <div className="footer-summary mt-[60px] pt-8 border-t border-border/30 flex justify-between items-center opacity-50">
            <div className="text-[10px] text-text-dim uppercase tracking-[0.3em]">Data Pipeline Statistics</div>
            <div className="research-progress flex gap-1.5">
              {[...Array(12)].map((_, i) => (
                <div 
                  key={i} 
                  className={`w-4 h-0.5 ${i < 7 ? 'bg-white' : 'bg-white/10'}`}
                ></div>
              ))}
            </div>
            <div className="text-[10px] text-text-dim font-mono tracking-[0.3em] uppercase">VER_9.0.4 // LOCAL</div>
          </div>
        </main>
      </div>
    </div>
  );
}
