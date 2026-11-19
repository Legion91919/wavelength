import type { PlayEventResponse, PlayReport, SearchResponse, Track } from './types';

export class ApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

async function json<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let title = response.statusText;
    try {
      const problem = (await response.json()) as { title?: string };
      title = problem.title ?? title;
    } catch {
      /* not a problem+json body */
    }
    throw new ApiError(response.status, title);
  }
  return (await response.json()) as T;
}

export const api = {
  search: (q: string, signal?: AbortSignal) =>
    fetch(`/api/search?q=${encodeURIComponent(q)}&limit=10`, { signal }).then((r) => json<SearchResponse>(r)),
  artistTracks: (id: string) =>
    fetch(`/api/artists/${encodeURIComponent(id)}`).then((r) => json<{ tracks: Track[] }>(r)).then((d) => d.tracks),
  albumTracks: (id: string) =>
    fetch(`/api/albums/${encodeURIComponent(id)}`).then((r) => json<{ tracks: Track[] }>(r)).then((d) => d.tracks),
  /** Retries once on network/5xx failure with the SAME playId: the server counts a play ID only once. */
  postPlay: async (report: PlayReport): Promise<PlayEventResponse> => {
    const send = () =>
      fetch('/api/plays', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(report), keepalive: true })
        .then((r) => json<PlayEventResponse>(r));
    try {
      return await send();
    } catch (e) {
      if (e instanceof ApiError && e.status < 500) throw e;
      return send();
    }
  },
};

export function newPlayId(): string {
  return crypto.randomUUID();
}
