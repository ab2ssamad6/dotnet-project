import { cn } from '@/utils/cn';

export function ProgressBar({
  value,
  className,
  barClassName,
  showLabel,
  size = 'md',
}: {
  value: number;
  className?: string;
  barClassName?: string;
  showLabel?: boolean;
  size?: 'sm' | 'md';
}) {
  const pct = Math.max(0, Math.min(100, Math.round(value)));
  const complete = pct >= 100;
  return (
    <div className="flex items-center gap-2.5">
      <div
        className={cn(
          'w-full overflow-hidden rounded-full bg-ink-200/80 shadow-[inset_0_1px_2px_rgb(29_27_24_/_0.06)]',
          size === 'sm' ? 'h-1.5' : 'h-2',
          className,
        )}
        role="progressbar"
        aria-valuenow={pct}
        aria-valuemin={0}
        aria-valuemax={100}
      >
        <div
          className={cn(
            'h-full rounded-full transition-[width] duration-700 ease-out',
            complete ? 'bg-green-500' : 'bg-brand-600',
            barClassName,
          )}
          style={{ width: `${pct}%` }}
        />
      </div>
      {showLabel && (
        <span className="tnum w-9 shrink-0 text-right text-xs font-bold text-ink-700">{pct}%</span>
      )}
    </div>
  );
}
