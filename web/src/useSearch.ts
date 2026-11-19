import { useEffect, useState } from 'react';
import { api } from './api/client';
import type { SearchState } from './components/SearchResults';

/** Debounced search with cancellation, so only the latest query's results are shown. */
export function useSearch(query: string, debounceMs = 200) {
  const [state, setState] = useState<SearchState>({ kind: 'idle' });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const q = query.trim();
    if (!q) {
      setState({ kind: 'idle' });
      return;
    }
    const controller = new AbortController();
    setState({ kind: 'loading' });
    const timer = setTimeout(() => {
      api.search(q, controller.signal).then(
        (result) => setState({ kind: 'success', result }),
        (e: unknown) => {
          if (controller.signal.aborted) return;
          setState({ kind: 'error', message: e instanceof Error ? e.message : 'network error' });
        },
      );
    }, debounceMs);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [query, debounceMs, attempt]);

  return { state, retry: () => setAttempt((a) => a + 1) };
}
