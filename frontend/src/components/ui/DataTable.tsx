import { type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Icons } from './Icon';
import { SkeletonTable } from './Skeleton';
import { EmptyState } from './EmptyState';
import { Pagination } from './Pagination';
import { Checkbox } from './Checkbox';

export interface Column<T> {
  key: string;
  header: ReactNode;
  render: (row: T) => ReactNode;
  sortable?: boolean;
  className?: string;
  headerClassName?: string;
  hideOnMobile?: boolean;
}

export type SortDirection = 'asc' | 'desc';
export interface SortState {
  key: string;
  direction: SortDirection;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  rowKey: (row: T) => string;
  loading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: ReactNode;
  onRowClick?: (row: T) => void;
  sort?: SortState;
  onSortChange?: (sort: SortState) => void;
  pagination?: {
    page: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
    onPageChange: (page: number) => void;
  };
  selection?: {
    selectedIds: string[];
    onChange: (ids: string[]) => void;
    renderActions?: (selectedIds: string[]) => ReactNode;
  };
}

export function DataTable<T>({
  columns,
  data,
  rowKey,
  loading,
  emptyTitle = 'No records found',
  emptyDescription,
  emptyAction,
  onRowClick,
  sort,
  onSortChange,
  pagination,
  selection,
}: DataTableProps<T>) {
  const toggleSort = (key: string) => {
    if (!onSortChange) return;
    const direction: SortDirection = sort?.key === key && sort.direction === 'asc' ? 'desc' : 'asc';
    onSortChange({ key, direction });
  };

  const allIds = data.map(rowKey);
  const allSelected = selection && allIds.length > 0 && allIds.every((id) => selection.selectedIds.includes(id));
  const someSelected = selection && selection.selectedIds.length > 0 && !allSelected;

  const toggleAll = () => {
    if (!selection) return;
    selection.onChange(allSelected ? [] : allIds);
  };

  const toggleOne = (id: string) => {
    if (!selection) return;
    selection.onChange(
      selection.selectedIds.includes(id)
        ? selection.selectedIds.filter((x) => x !== id)
        : [...selection.selectedIds, id],
    );
  };

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-card">
      {selection && selection.selectedIds.length > 0 && (
        <div className="flex items-center justify-between gap-3 border-b border-brand-100 bg-brand-50 px-4 py-2.5 text-sm">
          <span className="font-medium text-brand-700">{selection.selectedIds.length} selected</span>
          <div className="flex items-center gap-2">{selection.renderActions?.(selection.selectedIds)}</div>
        </div>
      )}
      <div className="overflow-x-auto">
        <table className="w-full min-w-[560px] border-collapse text-left text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80">
              {selection && (
                <th className="w-10 px-4 py-3">
                  <Checkbox
                    checked={!!allSelected}
                    ref={(el) => {
                      if (el) el.indeterminate = !!someSelected;
                    }}
                    onChange={toggleAll}
                    aria-label="Select all"
                  />
                </th>
              )}
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={cn(
                    'px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500',
                    col.hideOnMobile && 'hidden md:table-cell',
                    col.headerClassName,
                  )}
                >
                  {col.sortable && onSortChange ? (
                    <button
                      onClick={() => toggleSort(col.key)}
                      className="inline-flex items-center gap-1 hover:text-slate-700"
                    >
                      {col.header}
                      <span className="text-slate-400">
                        {sort?.key === col.key ? (
                          sort.direction === 'asc' ? (
                            <Icons.arrowUp size={13} />
                          ) : (
                            <Icons.arrowDown size={13} />
                          )
                        ) : (
                          <Icons.chevronDown size={13} className="opacity-40" />
                        )}
                      </span>
                    </button>
                  ) : (
                    col.header
                  )}
                </th>
              ))}
            </tr>
          </thead>
          {!loading && (
            <tbody className="divide-y divide-slate-100">
              {data.map((row) => {
                const id = rowKey(row);
                return (
                  <tr
                    key={id}
                    onClick={() => onRowClick?.(row)}
                    className={cn(
                      'transition-colors',
                      onRowClick && 'cursor-pointer hover:bg-slate-50',
                      selection?.selectedIds.includes(id) && 'bg-brand-50/40',
                    )}
                  >
                    {selection && (
                      <td className="w-10 px-4 py-3" onClick={(e) => e.stopPropagation()}>
                        <Checkbox
                          checked={selection.selectedIds.includes(id)}
                          onChange={() => toggleOne(id)}
                          aria-label="Select row"
                        />
                      </td>
                    )}
                    {columns.map((col) => (
                      <td
                        key={col.key}
                        className={cn(
                          'px-4 py-3.5 text-slate-700',
                          col.hideOnMobile && 'hidden md:table-cell',
                          col.className,
                        )}
                      >
                        {col.render(row)}
                      </td>
                    ))}
                  </tr>
                );
              })}
            </tbody>
          )}
        </table>
      </div>

      {loading && <SkeletonTable rows={5} cols={columns.length + (selection ? 1 : 0)} />}

      {!loading && data.length === 0 && (
        <EmptyState
          icon={<Icons.list size={26} />}
          title={emptyTitle}
          description={emptyDescription}
          action={emptyAction}
        />
      )}

      {pagination && !loading && data.length > 0 && <Pagination {...pagination} />}
    </div>
  );
}
