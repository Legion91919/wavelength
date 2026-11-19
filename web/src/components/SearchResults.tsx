import type { Album, Artist, SearchResponse, Track } from '../api/types';
import { formatDuration } from '../format';
import { Highlight } from './Highlight';

export type SearchState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'error'; message: string }
  | { kind: 'success'; result: SearchResponse };

interface Props {
  state: SearchState;
  nowPlayingId?: string;
  onPlay: (track: Track) => void;
  onPlayNext: (track: Track) => void;
  onPlayArtist: (artist: Artist) => void;
  onQueueAlbum: (album: Album) => void;
  onRetry: () => void;
}

export function SearchResults({ state, nowPlayingId, onPlay, onPlayNext, onPlayArtist, onQueueAlbum, onRetry }: Props) {
  if (state.kind === 'idle') {
    return <p className="hint">Search 300 independent labels. Accents are optional.</p>;
  }
  if (state.kind === 'loading') {
    return (
      <div aria-busy="true">
        <p className="hint">Searching&hellip;</p>
        {[0, 1, 2, 3].map((i) => <div key={i} className="skel" />)}
      </div>
    );
  }
  if (state.kind === 'error') {
    return (
      <div className="panel error" role="alert">
        <h2>Search is unavailable</h2>
        <p>We couldn't reach the catalogue ({state.message}). What's playing keeps playing.</p>
        <button className="btn primary" onClick={onRetry}>Try again</button>
      </div>
    );
  }

  const { result } = state;
  const total = result.tracks.length + result.artists.length + result.albums.length;
  if (total === 0) {
    return (
      <div className="panel">
        <h2>No matches for "{result.query}"</h2>
        <p>Try fewer letters or another spelling. Accents are optional: "meridien" finds "Méridien".</p>
      </div>
    );
  }

  const q = result.query;
  return (
    <div>
      <p className="hint">{total} results in {Math.max(1, Math.round(result.tookMs))} ms. Accents optional.</p>
      {result.tracks.length > 0 && (
        <section className="group" aria-labelledby="h-tracks">
          <h2 id="h-tracks">Tracks</h2>
          {result.tracks.map((t) => {
            const playing = t.id === nowPlayingId;
            return (
              <div key={t.id} className={playing ? 'row playing' : 'row'}>
                <div className="thumb" aria-hidden="true" />
                <div className="t">
                  <strong><Highlight text={t.title} query={q} /></strong>
                  <small><Highlight text={t.artistName} query={q} /> &middot; {t.albumTitle}</small>
                </div>
                <span className="dur">{formatDuration(t.durationSeconds)}</span>
                <button className={playing ? 'btn' : 'btn primary'} aria-label={playing ? `Playing ${t.title}` : `Play ${t.title}`} disabled={playing} onClick={() => onPlay(t)}>
                  {playing ? '♫' : '▶'}
                </button>
                <button className="btn" aria-label={`Play ${t.title} next`} onClick={() => onPlayNext(t)}>
                  +<span className="add-label"> Next</span>
                </button>
              </div>
            );
          })}
        </section>
      )}
      {result.artists.length > 0 && (
        <section className="group" aria-labelledby="h-artists">
          <h2 id="h-artists">Artists</h2>
          {result.artists.map((a) => (
            <div key={a.id} className="row">
              <div className="thumb round" aria-hidden="true" />
              <div className="t">
                <strong><Highlight text={a.name} query={q} /></strong>
                <small>{a.labelName} &middot; {a.trackCount} tracks</small>
              </div>
              <button className="btn" onClick={() => onPlayArtist(a)}>Play all</button>
            </div>
          ))}
        </section>
      )}
      {result.albums.length > 0 && (
        <section className="group" aria-labelledby="h-albums">
          <h2 id="h-albums">Albums</h2>
          {result.albums.map((a) => (
            <div key={a.id} className="row">
              <div className="thumb" aria-hidden="true" />
              <div className="t">
                <strong><Highlight text={a.title} query={q} /></strong>
                <small>{a.artistName} &middot; {a.year} &middot; {a.labelName}</small>
              </div>
              <button className="btn" onClick={() => onQueueAlbum(a)}>Queue album</button>
            </div>
          ))}
        </section>
      )}
    </div>
  );
}
