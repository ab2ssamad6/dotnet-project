import type { ReactNode } from 'react';

export function EmptyState({
  icon,
  title,
  description,
  action,
}: {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
}) {
  return (
    <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
      {icon && (
        <div className="relative mb-5">
          <span className="absolute inset-0 -m-2 rounded-2xl bg-brand-100/50 blur-md" aria-hidden />
          <span className="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-ink-200/70 bg-white text-brand-600 shadow-card">
            {icon}
          </span>
        </div>
      )}
      <h3 className="text-base font-bold tracking-[-0.01em] text-ink-900">{title}</h3>
      {description && <p className="mt-1.5 max-w-sm text-sm leading-relaxed text-ink-500">{description}</p>}
      {action && <div className="mt-6">{action}</div>}
    </div>
  );
}
