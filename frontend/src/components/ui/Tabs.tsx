import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

export interface TabItem {
  id: string;
  label: string;
  icon?: ReactNode;
  count?: number;
}

/** Segmented pill switcher — reads as a control rather than a page divider. */
export function Tabs({
  tabs,
  active,
  onChange,
  className,
}: {
  tabs: TabItem[];
  active: string;
  onChange: (id: string) => void;
  className?: string;
}) {
  return (
    <div
      role="tablist"
      className={cn(
        'inline-flex max-w-full gap-1 overflow-x-auto rounded-xl border border-ink-200/80 bg-white p-1 shadow-card',
        className,
      )}
    >
      {tabs.map((tab) => {
        const isActive = active === tab.id;
        return (
          <button
            key={tab.id}
            role="tab"
            aria-selected={isActive}
            onClick={() => onChange(tab.id)}
            className={cn(
              'focus-ring inline-flex items-center gap-2 whitespace-nowrap rounded-lg px-3.5 py-2 text-[13px] font-semibold transition-colors',
              isActive ? 'bg-ink-900 text-white shadow-sm' : 'text-ink-500 hover:bg-ink-100 hover:text-ink-800',
            )}
          >
            {tab.icon}
            {tab.label}
            {tab.count !== undefined && (
              <span
                className={cn(
                  'tnum rounded-full px-1.5 py-0.5 text-[11px] font-bold',
                  isActive ? 'bg-white/15 text-white' : 'bg-ink-100 text-ink-500',
                )}
              >
                {tab.count}
              </span>
            )}
          </button>
        );
      })}
    </div>
  );
}
