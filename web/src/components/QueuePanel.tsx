import type { Track } from '../api/types';
import { formatDuration } from '../format';

export function QueuePanel({ items, open, onRemove }: { items: Track[]; open: boolean; onRemove: (index: number) => void }) {
  return (
    <aside id="queue" className={open ? 'queue open' : 'queue'} aria-label="Queue">
      <h2>Up next ({items.length})</h2>
      {items.length === 0 ? (
        <p className="hint">Nothing queued. Use + Next on any track.</p>
      ) : (
        <ol>
          {items.map((t, i) => (
            <li key={`${t.id}-${i}`}>
              <div className="t">{t.title}<small>{t.artistName}</small></div>
              <span className="dur">{formatDuration(t.durationSeconds)}</span>
              <button className="btn" aria-label={`Remove ${t.title} from queue`} onClick={() => onRemove(i)}>&times;</button>
            </li>
          ))}
        </ol>
      )}
    </aside>
  );
}
