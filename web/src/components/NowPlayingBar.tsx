import type { RefObject } from 'react';
import type { Track } from '../api/types';
import { formatDuration } from '../format';
import type { AudioStatus, CountedState } from '../player/usePlayer';

interface Props {
  audioRef: RefObject<HTMLAudioElement>;
  current: Track | null;
  status: AudioStatus;
  listened: number;
  counted: CountedState;
  queueLength: number;
  queueOpen: boolean;
  onToggle: () => void;
  onNext: () => void;
  onRestart: () => void;
  onToggleQueue: () => void;
}

const countedText: Record<CountedState, (label: string) => string> = {
  idle: () => 'Counts for the label at 0:30',
  pending: () => 'Counting…',
  counted: (label) => `✓ Counted for ${label}`,
  failed: () => '! Could not report this play',
};

export function NowPlayingBar(p: Props) {
  const { current } = p;
  const pct = current ? Math.min(100, (p.listened / current.durationSeconds) * 100) : 0;
  return (
    <div className="bar" role="region" aria-label="Now playing">
      <audio ref={p.audioRef} preload="auto" />
      <div className="now">
        {current ? (
          <>
            <strong>{current.title}</strong>
            <small>{current.artistName} &middot; {current.labelName}</small>
          </>
        ) : (
          <>
            <strong>Nothing playing yet</strong>
            <small>Search and press play</small>
          </>
        )}
      </div>
      <div className="mid">
        <div className="ctrls">
          <button className="btn restart" aria-label="Restart track" disabled={!current} onClick={p.onRestart}>&#9198;</button>
          <button className="btn primary" aria-label={p.status === 'playing' ? 'Pause' : 'Play'} disabled={!current || p.status === 'error'} onClick={p.onToggle}>
            {p.status === 'playing' ? '❚❚' : '▶'}
          </button>
          <button className="btn" aria-label="Next track" disabled={!current} onClick={p.onNext}>&#9197;</button>
        </div>
        {current && (
          <div className="prog">
            <span>{formatDuration(p.listened)}</span>
            <span className="track" role="progressbar" aria-label="Listened" aria-valuemin={0} aria-valuemax={current.durationSeconds} aria-valuenow={Math.floor(p.listened)}>
              <i style={{ width: `${pct}%` }} />
            </span>
            <span>{formatDuration(current.durationSeconds)}</span>
          </div>
        )}
      </div>
      <div className="status" aria-live="polite">
        {current && p.status === 'loading' && <span className="pending">Loading audio&hellip;</span>}
        {current && p.status === 'error' && <span className="failed" role="alert">Audio failed to load. Skip to continue.</span>}
        {current && p.status !== 'error' && p.status !== 'loading' && (
          <span className={p.counted === 'counted' ? 'counted' : p.counted === 'failed' ? 'failed' : 'pending'}>{countedText[p.counted](current.labelName)}</span>
        )}
        <button className="btn qtoggle" aria-controls="queue" aria-expanded={p.queueOpen} onClick={p.onToggleQueue}>Queue {p.queueLength}</button>
      </div>
    </div>
  );
}
