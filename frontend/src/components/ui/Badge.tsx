import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

export type BadgeTone = 'neutral' | 'brand' | 'success' | 'warning' | 'danger' | 'info' | 'ai' | 'gold';

/** Soft, ringed chips — the tone map is the single source of truth for status colors. */
export const badgeTones: Record<BadgeTone, string> = {
  neutral: 'bg-ink-100 text-ink-600 ring-ink-200/70',
  brand: 'bg-brand-50 text-brand-800 ring-brand-200/70',
  success: 'bg-green-50 text-green-700 ring-green-200/70',
  warning: 'bg-amber-50 text-amber-700 ring-amber-200/70',
  danger: 'bg-rose-50 text-rose-700 ring-rose-200/70',
  info: 'bg-sky-50 text-sky-700 ring-sky-200/70',
  ai: 'bg-violet-50 text-violet-700 ring-violet-200/70',
  gold: 'bg-gold-50 text-gold-700 ring-gold-200/70',
};

export function Badge({
  children,
  className,
  tone,
  dot,
}: {
  children: ReactNode;
  className?: string;
  tone?: BadgeTone;
  dot?: boolean;
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 whitespace-nowrap rounded-full px-2.5 py-1 text-[11.5px] font-semibold leading-none ring-1 ring-inset',
        tone ? badgeTones[tone] : (className ?? badgeTones.neutral),
        tone && className,
      )}
    >
      {dot && <span className="h-1.5 w-1.5 rounded-full bg-current opacity-80" />}
      {children}
    </span>
  );
}
