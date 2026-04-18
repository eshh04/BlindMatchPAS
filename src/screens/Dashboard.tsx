import React from 'react';
import { api } from '../services/api';
import { DashboardMetrics } from '../types';

export default function Dashboard() {
  const [metrics, setMetrics] = React.useState<DashboardMetrics | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);

  React.useEffect(() => {
    const fetchMetrics = async () => {
      try {
        const data = await api.admin.getMetrics();
        setMetrics(data);
      } catch (err) {
        console.error('Failed to fetch metrics', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchMetrics();
  }, []);

  const displayMetrics = [
    { label: 'Total Projects', value: metrics?.totalProjects ?? '...' },
    { label: 'Supervisors', value: metrics?.totalSupervisors ?? '...' },
    { label: 'Research Areas', value: metrics?.totalResearchAreas ?? '...' },
    { label: 'Allocation Rate', value: metrics ? `${Math.round((metrics.revealedCount / (metrics.totalProjects || 1)) * 100)}%` : '...' },
  ];

  return (
    <div className="space-y-10">
      {/* Metrics Grid */}
      <section className="metrics grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {displayMetrics.map((m, i) => (
          <div key={i} className="metric-card theme-card">
            <div className="metric-label theme-label">{m.label}</div>
            <div className={`metric-value theme-value ${isLoading ? 'animate-pulse opacity-50' : ''}`}>{m.value}</div>
          </div>
        ))}
      </section>

      {/* Performance Chart Section (Adapted to theme) */}
      <section className="theme-card relative overflow-hidden">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-[14px] font-bold uppercase tracking-widest">SYSTEM PERFORMANCE</h2>
          <div className="flex gap-4">
            <span className="text-[11px] uppercase tracking-widest border border-border px-2 py-1">REAL-TIME</span>
          </div>
        </div>
        <div className="h-64 w-full relative flex items-end">
          <svg className="w-full h-full absolute inset-0 overflow-visible" preserveAspectRatio="none" viewBox="0 0 1000 300">
            <polyline
              fill="none"
              stroke="rgba(255,255,255,0.2)"
              strokeWidth="1"
              points="0,280 100,250 200,270 300,180 400,200 500,100 600,150 700,50 800,120 900,80 1000,10"
            />
          </svg>
          <div className="absolute left-0 top-0 bottom-0 flex flex-col justify-between text-[10px] text-text-dim font-mono">
            <span>100%</span>
            <span>0%</span>
          </div>
        </div>
      </section>
    </div>
  );
}
