import { useCallback, useEffect, useRef, useState } from 'react';
import { toApiError, type ApiError } from '@/api/errors';
import type { PagedQuery, PagedResult } from '@/types';
import type { SortState } from '@/components/ui';
import { DEFAULT_PAGE_SIZE } from '@/constants/config';

interface Options {
  pageSize?: number;
  initialSort?: SortState;
}

/**
 * Generic controller for a paginated, searchable, sortable list backed by a
 * server endpoint of shape `(query) => Promise<PagedResult<T>>`.
 * Sorting is applied client-side on the current page (the API has no sort param).
 */
export function usePagedList<T>(fetcher: (q: PagedQuery) => Promise<PagedResult<T>>, options: Options = {}) {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(options.pageSize ?? DEFAULT_PAGE_SIZE);
  const [search, setSearchState] = useState('');
  const [sort, setSort] = useState<SortState | undefined>(options.initialSort);
  const [result, setResult] = useState<PagedResult<T> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ApiError | null>(null);
  const mounted = useRef(true);
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetcherRef.current({ page, pageSize, search: search || undefined });
      if (mounted.current) setResult(data);
    } catch (err) {
      if (mounted.current) setError(toApiError(err));
    } finally {
      if (mounted.current) setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    void load();
  }, [load]);

  const setSearch = useCallback((value: string) => {
    setSearchState(value);
    setPage(1);
  }, []);

  const items = result?.items ?? [];
  const sortedItems = sort ? sortRows(items, sort) : items;

  return {
    items: sortedItems,
    raw: result,
    loading,
    error,
    page,
    pageSize,
    search,
    sort,
    totalPages: result?.totalPages ?? 0,
    totalCount: result?.totalCount ?? 0,
    setPage,
    setSearch,
    setSort,
    refetch: load,
  };
}

/** Sort an array of records by a dotless key, coercing values sensibly. */
function sortRows<T>(rows: T[], sort: SortState): T[] {
  const dir = sort.direction === 'asc' ? 1 : -1;
  return [...rows].sort((a, b) => {
    const av = (a as Record<string, unknown>)[sort.key];
    const bv = (b as Record<string, unknown>)[sort.key];
    if (av == null) return 1;
    if (bv == null) return -1;
    if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * dir;
    return String(av).localeCompare(String(bv), undefined, { numeric: true }) * dir;
  });
}
