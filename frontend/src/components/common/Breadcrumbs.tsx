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
    <nav aria-label="Breadcrumb" className="mb-4 flex items-center gap-1.5 text-sm">
      {items.map((crumb, i) => {
        const last = i === items.length - 1;
        return (
          <Fragment key={`${crumb.label}-${i}`}>
            {crumb.to && !last ? (
              <Link to={crumb.to} className="text-slate-500 hover:text-brand-600">
                {crumb.label}
              </Link>
            ) : (
              <span className={last ? 'font-medium text-slate-700' : 'text-slate-500'}>{crumb.label}</span>
            )}
            {!last && <Icons.chevronRight size={14} className="text-slate-300" />}
          </Fragment>
        );
      })}
    </nav>
  );
}
