import { Fragment } from 'react';
import { Link } from 'react-router-dom';
import { Icons } from '@/components/ui';

export interface Crumb {
  label: string;
  to?: string;
}

export function Breadcrumbs({ items }: { items: Crumb[] }) {
  if (items.length === 0) return null;
  return (
    <nav aria-label="Breadcrumb" className="mb-5 flex items-center gap-1.5 text-[12.5px]">
      {items.map((crumb, i) => {
        const last = i === items.length - 1;
        return (
          <Fragment key={`${crumb.label}-${i}`}>
            {crumb.to && !last ? (
              <Link
                to={crumb.to}
                className="rounded font-semibold text-ink-500 transition-colors hover:text-brand-700"
              >
                {crumb.label}
              </Link>
            ) : (
              <span className={last ? 'truncate font-semibold text-ink-800' : 'font-semibold text-ink-500'}>
                {crumb.label}
              </span>
            )}
            {!last && <Icons.chevronRight size={13} className="shrink-0 text-ink-300" />}
          </Fragment>
        );
      })}
    </nav>
  );
}
