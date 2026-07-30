import { cn } from '@/utils/cn';

/** Wordmark + monogram. `tone="light"` is for dark surfaces (app rail, auth hero). */
export function Logo({
  collapsed,
  className,
  tone = 'dark',
}: {
  collapsed?: boolean;
  className?: string;
  tone?: 'dark' | 'light';
}) {
  const light = tone === 'light';
  return (
    <div className={cn('flex items-center gap-3', className)}>
      <div
        className={cn(
          'relative flex h-9 w-9 shrink-0 items-center justify-center rounded-xl shadow-sm ring-1',
          light ? 'bg-white/10 ring-white/15' : 'bg-brand-gradient ring-ink-900/10',
        )}
      >
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 4 3 8l9 4 7-3.11V14h2V8L12 4z" fill="currentColor" className="text-white" />
          <path
            d="M6 11.5V15c0 1.4 2.7 2.8 6 2.8s6-1.4 6-2.8v-3.5l-6 2.67-6-2.67z"
            className={light ? 'text-white/60' : 'text-gold-300'}
            fill="currentColor"
          />
        </svg>
      </div>
      {!collapsed && (
        <div className="leading-none">
          <p
            className={cn(
              'font-display text-[17px] font-semibold tracking-[-0.02em]',
              light ? 'text-white' : 'text-ink-900',
            )}
          >
            LMS
          </p>
          <p
            className={cn(
              'mt-1 text-[10.5px] font-semibold uppercase tracking-[0.16em]',
              light ? 'text-white/55' : 'text-ink-400',
            )}
          >
            Learning Studio
          </p>
        </div>
      )}
    </div>
  );
}
