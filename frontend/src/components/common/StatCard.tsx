import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Icons } from '@/components/ui';
import { cn } from '@/utils/cn';

interface StatCardProps {
  label: string;
  value: ReactNode;
  icon: ReactNode;
  accent?: 'brand' | 'green' | 'amber' | 'violet' | 'sky' | 'rose';
  to?: string;
  hint?: string;
}

const accents: Record<NonNullable<StatCardProps['accent']>, string> = {
  brand: 'bg-brand-50 text-brand-700 ring-brand-200/60',
  green: 'bg-green-50 text-green-700 ring-green-200/60',
  amber: 'bg-gold-50 text-gold-700 ring-gold-200/60',
  violet: 'bg-violet-50 text-violet-700 ring-violet-200/60',
  sky: 'bg-sky-50 text-sky-700 ring-sky-200/60',
  rose: 'bg-rose-50 text-rose-700 ring-rose-200/60',
};

export function StatCard({ label, value, icon, accent = 'brand', to, hint }: StatCardProps) {
  const inner = (
    <div
      className={cn(
        'surface relative h-full overflow-hidden p-5 transition-all duration-200',
        to && 'hover:-translate-y-0.5 hover:border-ink-300/80 hover:shadow-raised',
      )}
    >
      <div className="flex items-start justify-between gap-3">
        <p className="text-[12.5px] font-semibold uppercase tracking-[0.08em] text-ink-400">{label}</p>
        <span
          className={cn(
            'flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset',
            accents[accent],
          )}
        >
          {icon}
        </span>
      </div>
      <p className="tnum mt-4 font-display text-[30px] font-semibold leading-none tracking-[-0.02em] text-ink-900">
        {value}
      </p>
      <div className="mt-2 flex min-h-[18px] items-center gap-1.5">
        {hint && <p className="text-xs text-ink-500">{hint}</p>}
        {to && !hint && (
          <span className="inline-flex items-center gap-1 text-xs font-semibold text-brand-700">
            View details <Icons.arrowRight size={13} />
          </span>
        )}
      </div>
    </div>
  );
  return to ? (
    <Link to={to} className="focus-ring block rounded-2xl">
      {inner}
    </Link>
  ) : (
    inner
  );
}
