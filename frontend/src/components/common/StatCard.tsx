import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { cn } from '@/utils/cn';

interface StatCardProps {
  label: string;
  value: ReactNode;
  icon: ReactNode;
  accent?: 'brand' | 'emerald' | 'amber' | 'violet' | 'sky' | 'rose';
  to?: string;
  hint?: string;
}

const accents: Record<NonNullable<StatCardProps['accent']>, string> = {
  brand: 'bg-brand-50 text-brand-600',
  emerald: 'bg-emerald-50 text-emerald-600',
  amber: 'bg-amber-50 text-amber-600',
  violet: 'bg-violet-50 text-violet-600',
  sky: 'bg-sky-50 text-sky-600',
  rose: 'bg-rose-50 text-rose-600',
};

export function StatCard({ label, value, icon, accent = 'brand', to, hint }: StatCardProps) {
  const inner = (
    <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-card transition-shadow hover:shadow-md">
      <div className={cn('flex h-12 w-12 shrink-0 items-center justify-center rounded-xl', accents[accent])}>{icon}</div>
      <div className="min-w-0">
        <p className="text-sm font-medium text-slate-500">{label}</p>
        <p className="text-2xl font-bold tracking-tight text-slate-900">{value}</p>
        {hint && <p className="text-xs text-slate-400">{hint}</p>}
      </div>
    </div>
  );
  return to ? (
    <Link to={to} className="block">
      {inner}
    </Link>
  ) : (
    inner
  );
}
