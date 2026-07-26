import { cn } from '@/utils/cn';

export function ProgressBar({
  value,
  className,
  barClassName,
  showLabel,
}: {
  value: number;
  className?: string;
  barClassName?: string;
  showLabel?: boolean;
}) {
  const pct = Math.max(0, Math.min(100, Math.round(value)));
  return (
    <div className="flex items-center gap-2">
      <div className={cn('h-2 w-full overflow-hidden rounded-full bg-slate-200', className)}>
        <div
          className={cn('h-full rounded-full bg-brand-600 transition-all duration-500', barClassName)}
          style={{ width: `${pct}%` }}
        />
      </div>
      {showLabel && <span className="w-10 shrink-0 text-right text-xs font-medium text-slate-600">{pct}%</span>}
    </div>
  );
}
