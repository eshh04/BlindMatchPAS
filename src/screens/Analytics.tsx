import React from 'react';
import { api } from '../services/api';
import { motion } from 'motion/react';
import { TrendingUp, BarChart3, Activity, PieChart } from 'lucide-react';

export default function Analytics() {
  const [data, setData] = React.useState<any>(null);
  const [isLoading, setIsLoading] = React.useState(true);

  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await api.analytics.getSummary();
        setData(res);
      } catch (err) {
        console.error('Failed to fetch analytics', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  if (isLoading) return <div className="text-center p-20 animate-pulse text-text-dim uppercase tracking-[.3em]">Processing Core Data...</div>;

  return (
    <div className="space-y-10 selection:bg-white selection:text-black">
      {/* Top Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="theme-card bg-surface p-8 relative overflow-hidden">
          <div className="theme-label text-[10px] mb-2">System Load</div>
          <div className="text-4xl font-light tracking-tighter">{Math.round(data?.totalSystemLoad || 0)}%</div>
          <motion.div 
            className="absolute bottom-0 left-0 h-1 bg-white"
            initial={{ width: 0 }}
            animate={{ width: `${data?.totalSystemLoad || 0}%` }}
            transition={{ duration: 1.5, ease: "easeOut" }}
          />
        </div>
        <div className="theme-card bg-surface p-8">
          <div className="theme-label text-[10px] mb-2">Active Nodes</div>
          <div className="text-4xl font-light tracking-tighter">{data?.areaDistribution?.length || 0}</div>
        </div>
        <div className="theme-card bg-surface p-8">
          <div className="theme-label text-[10px] mb-2">Personnel Sync</div>
          <div className="text-4xl font-light tracking-tighter">100%</div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Research Area Distribution */}
        <section className="theme-card bg-surface p-8 space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xs font-bold uppercase tracking-[.2em]">Research Vector Distribution</h2>
            <PieChart size={16} className="text-text-dim" />
          </div>
          <div className="space-y-5">
            {data?.areaDistribution.map((area: any, i: number) => (
              <div key={i} className="space-y-2">
                <div className="flex justify-between text-[11px] uppercase tracking-widest">
                  <span>{area.name}</span>
                  <span className="font-mono">{area.value} units</span>
                </div>
                <div className="w-full h-[2px] bg-white/5 relative">
                  <motion.div 
                    className="absolute inset-y-0 left-0 bg-white"
                    initial={{ width: 0 }}
                    animate={{ width: `${(area.value / 10) * 100}%` }} // Simplified percentage
                    transition={{ duration: 1, delay: i * 0.1 }}
                  />
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* Supervisor Load */}
        <section className="theme-card bg-surface p-8 space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xs font-bold uppercase tracking-[.2em]">Supervisor Allocation Log</h2>
            <BarChart3 size={16} className="text-text-dim" />
          </div>
          <div className="space-y-4">
            {data?.supervisorLoad.map((s: any, i: number) => (
              <div key={i} className="flex items-center gap-4">
                <div className="w-32 text-[10px] uppercase tracking-tighter truncate text-text-dim">{s.name}</div>
                <div className="flex-1 flex gap-1 h-3">
                  {[...Array(5)].map((_, idx) => (
                    <motion.div
                      key={idx}
                      className={`flex-1 ${idx < s.projectCount ? 'bg-white' : 'bg-white/5'}`}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      transition={{ delay: i * 0.05 + idx * 0.1 }}
                    />
                  ))}
                </div>
                <div className="w-8 text-right font-mono text-[10px]">{s.projectCount}.0</div>
              </div>
            ))}
          </div>
        </section>
      </div>

      <div className="theme-card bg-zinc-950 p-6 border-dashed border-zinc-800 border flex items-center justify-center text-zinc-600 gap-4 text-[10px] uppercase tracking-[.3em]">
        <Activity size={14} className="animate-pulse" />
        Neural Core Processing Active // Data Streams Stabilized
      </div>
    </div>
  );
}
