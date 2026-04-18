import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Eye, EyeOff } from 'lucide-react';
import { api } from '../services/api';

export default function Login() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    try {
      await api.admin.login({ email, password });
      navigate('/dashboard');
    } catch (err: any) {
      setError(err.message || 'Authentication Failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-bg flex items-center justify-center p-6 selection:bg-white selection:text-black font-sans">
      <main className="w-full max-w-md">
        {/* Branding Header */}
        <div className="mb-12 text-center">
          <div className="brand font-mono text-sm font-bold tracking-[0.2em] mb-4 flex items-center justify-center gap-[10px]">
            <span className="w-2 h-2 bg-red-dot rounded-full"></span>
            BLIND_MATCH // PAS
          </div>
          <h1 className="text-[32px] font-light tracking-[-0.02em] text-text-main">
            Authentication
          </h1>
        </div>

        {/* Login Card */}
        <div className="theme-card bg-surface p-10">
          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="border border-red-500/50 bg-red-500/10 p-3 text-red-500 text-[11px] uppercase tracking-widest text-center">
                {error}
              </div>
            )}
            
            {/* Email Field */}
            <div className="space-y-2">
              <label className="theme-label">Email Address</label>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-bg border border-border px-4 py-3 text-[13px] focus:outline-none focus:border-accent transition-colors placeholder:text-text-dim/30"
                placeholder="identity@system.core"
              />
            </div>

            {/* Password Field */}
            <div className="space-y-2">
              <label className="theme-label">Access Key</label>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full bg-bg border border-border px-4 py-3 text-[13px] focus:outline-none focus:border-accent transition-colors placeholder:text-text-dim/30"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-text-dim hover:text-text-main transition-colors"
                >
                  {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            {/* Login Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-accent text-bg py-3 text-[13px] font-bold uppercase tracking-[0.2em] transition-all hover:bg-white active:scale-[0.98] flex items-center justify-center gap-2 disabled:opacity-50"
            >
              {isLoading ? 'Processing...' : 'LOGIN'}
              <ArrowRight size={16} />
            </button>
          </form>
        </div>

        {/* System Status Bar */}
        <div className="mt-8 flex justify-center items-center gap-6">
          <div className="flex items-center gap-2">
            <div className="w-1.5 h-1.5 bg-[#00FF00]"></div>
            <span className="text-[10px] text-text-dim uppercase tracking-widest">System Online</span>
          </div>
          <div className="text-[10px] text-text-dim uppercase tracking-widest font-mono">
            SEC_ENCRYPTED_99%
          </div>
        </div>
      </main>
    </div>
  );
}
