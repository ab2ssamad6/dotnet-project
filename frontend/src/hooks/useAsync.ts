import { useCallback, useEffect, useRef, useState } from 'react';
import { toApiError, type ApiError } from '@/api/errors';

interface AsyncState<T> {
  data: T | null;
  loading: boolean;
  error: ApiError | null;
}

/**
 * Runs an async function on mount (and whenever `deps` change), exposing
 * data/loading/error plus a `refetch`. Guards against setState after unmount.
 */
export function useAsync<T>(fn: () => Promise<T>, deps: unknown[] = []) {
  const [state, setState] = useState<AsyncState<T>>({ data: null, loading: true, error: null });
  const mounted = useRef(true);
  const fnRef = useRef(fn);
  fnRef.current = fn;

  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);

  const run = useCallback(async () => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      const data = await fnRef.current();
      if (mounted.current) setState({ data, loading: false, error: null });
      return data;
    } catch (err) {
      if (mounted.current) setState({ data: null, loading: false, error: toApiError(err) });
      return null;
    }
  }, []);

  useEffect(() => {
    void run();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  return { ...state, refetch: run, setData: (data: T) => setState((s) => ({ ...s, data })) };
}
