import { cn } from '@/utils/cn';

export function Logo({ collapsed, className }: { collapsed?: boolean; className?: string }) {
  return (
    <div className={cn('flex items-center gap-2.5', className)}>
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-brand-600 text-white shadow-sm">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 4 3 8l9 4 7-3.11V14h2V8L12 4z" fill="currentColor" />
          <path
            d="M6 11.5V15c0 1.4 2.7 2.8 6 2.8s6-1.4 6-2.8v-3.5l-6 2.67-6-2.67z"
            fill="currentColor"
            opacity="0.55"
          />
        </svg>
      </div>
      {!collapsed && (
        <div className="leading-tight">
          <p className="text-sm font-bold tracking-tight text-slate-900">LMS</p>
          <p className="text-[11px] font-medium text-slate-400">Learning Platform</p>
        </div>
      )}
    </div>
  );
}
