import { cn } from '@/utils/cn';
import { Icons } from './Icon';

interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  /** Drops the divider — for use as a standalone bar rather than a table footer. */
  bare?: boolean;
}

function range(page: number, total: number): (number | '…')[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | '…')[] = [1];
  const start = Math.max(2, page - 1);
  const end = Math.min(total - 1, page + 1);
  if (start > 2) pages.push('…');
  for (let i = start; i <= end; i++) pages.push(i);
  if (end < total - 1) pages.push('…');
  pages.push(total);
  return pages;
}

const arrow =
  'focus-ring inline-flex h-9 w-9 items-center justify-center rounded-lg border border-ink-200 bg-white text-ink-500 shadow-card transition-colors hover:border-ink-300 hover:text-ink-800 disabled:opacity-40 disabled:hover:border-ink-200';

export function Pagination({ page, totalPages, totalCount, pageSize, onPageChange, bare }: PaginationProps) {
  if (totalCount === 0) return null;
  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  return (
    <div
      className={cn(
        'flex flex-col items-center justify-between gap-3 px-5 py-3.5 sm:flex-row',
        !bare && 'border-t border-ink-100',
      )}
    >
      <p className="tnum text-[13px] text-ink-500">
        <span className="font-bold text-ink-800">{from}</span>–<span className="font-bold text-ink-800">{to}</span> of{' '}
        <span className="font-bold text-ink-800">{totalCount}</span>
      </p>
      <div className="flex items-center gap-1.5">
        <button onClick={() => onPageChange(page - 1)} disabled={page <= 1} className={arrow} aria-label="Previous page">
          <Icons.chevronLeft size={17} />
        </button>
        {range(page, totalPages).map((p, i) =>
          p === '…' ? (
            <span key={`ellipsis-${i}`} className="px-1 text-ink-400">
              …
            </span>
          ) : (
            <button
              key={p}
              onClick={() => onPageChange(p)}
              aria-current={p === page ? 'page' : undefined}
              className={cn(
                'focus-ring tnum inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-2.5 text-[13px] font-bold transition-colors',
                p === page
                  ? 'bg-ink-900 text-white shadow-sm'
                  : 'text-ink-500 hover:bg-ink-100 hover:text-ink-800',
              )}
            >
              {p}
            </button>
          ),
        )}
        <button
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          className={arrow}
          aria-label="Next page"
        >
          <Icons.chevronRight size={17} />
        </button>
      </div>
    </div>
  );
}
