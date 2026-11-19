import { useCallback, useReducer, useState } from 'react';
import { api } from './api/client';
import { Logo } from './components/Logo';
import { NowPlayingBar } from './components/NowPlayingBar';
import { QueuePanel } from './components/QueuePanel';
import { SearchBox } from './components/SearchBox';
import { SearchResults } from './components/SearchResults';
import { emptyQueue, queueReducer } from './player/queue';
import { usePlayer } from './player/usePlayer';
import { useSearch } from './useSearch';

export function App() {
  const [query, setQuery] = useState(() => new URLSearchParams(window.location.search).get('q') ?? '');
  const [queueOpen, setQueueOpen] = useState(false);
  const { state, retry } = useSearch(query);
  const [queue, dispatch] = useReducer(queueReducer, emptyQueue);
  const advance = useCallback(() => dispatch({ type: 'advance' }), []);
  const player = usePlayer(queue.current, queue.playSeq, advance);

  return (
    <div className="app">
      <header className="header">
        <Logo />
        <SearchBox value={query} onChange={setQuery} />
      </header>
      <main className="results" aria-live="polite">
        <SearchResults
          state={state}
          nowPlayingId={queue.current?.id}
          onPlay={(track) => dispatch({ type: 'play', track })}
          onPlayNext={(track) => dispatch({ type: 'playNext', track })}
          onPlayArtist={(artist) => void api.artistTracks(artist.id).then((tracks) => dispatch({ type: 'playAll', tracks }))}
          onQueueAlbum={(album) => void api.albumTracks(album.id).then((tracks) => dispatch({ type: 'enqueue', tracks }))}
          onRetry={retry}
        />
      </main>
      <QueuePanel items={queue.upNext} open={queueOpen} onRemove={(index) => dispatch({ type: 'remove', index })} />
      <NowPlayingBar
        audioRef={player.audioRef}
        current={queue.current}
        status={player.status}
        listened={player.listened}
        counted={player.counted}
        queueLength={queue.upNext.length}
        queueOpen={queueOpen}
        onToggle={player.toggle}
        onNext={advance}
        onRestart={() => queue.current && dispatch({ type: 'play', track: queue.current })}
        onToggleQueue={() => setQueueOpen((o) => !o)}
      />
    </div>
  );
}
