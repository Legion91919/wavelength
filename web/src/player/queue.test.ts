import { describe, expect, it } from 'vitest';
import type { Track } from '../api/types';
import { emptyQueue, queueReducer } from './queue';

const t = (id: string): Track => ({
  id, title: id, artistId: 'a', artistName: 'A', albumId: 'b', albumTitle: 'B', labelId: 'l', labelName: 'L', durationSeconds: 200, audioUrl: '/cdn/audio/x.mp3',
});

describe('queueReducer', () => {
  it('plays a track and queues the next one in front of the rest', () => {
    let s = queueReducer(emptyQueue, { type: 'play', track: t('one') });
    s = queueReducer(s, { type: 'enqueue', tracks: [t('later')] });
    s = queueReducer(s, { type: 'playNext', track: t('two') });
    expect(s.current?.id).toBe('one');
    expect(s.upNext.map((x) => x.id)).toEqual(['two', 'later']);
  });

  it('advancing starts the next track as a new listen', () => {
    let s = queueReducer(emptyQueue, { type: 'play', track: t('one') });
    s = queueReducer(s, { type: 'playNext', track: t('two') });
    const before = s.playSeq;
    s = queueReducer(s, { type: 'advance' });
    expect(s.current?.id).toBe('two');
    expect(s.playSeq).toBe(before + 1);
    s = queueReducer(s, { type: 'advance' });
    expect(s.current).toBeNull();
  });

  it('replaying the same track is a new listen (new play ID)', () => {
    let s = queueReducer(emptyQueue, { type: 'play', track: t('one') });
    const seq = s.playSeq;
    s = queueReducer(s, { type: 'play', track: t('one') });
    expect(s.playSeq).toBe(seq + 1);
  });

  it('play next with nothing playing starts it now; remove drops by position', () => {
    let s = queueReducer(emptyQueue, { type: 'playNext', track: t('one') });
    expect(s.current?.id).toBe('one');
    s = queueReducer(s, { type: 'enqueue', tracks: [t('a'), t('b'), t('c')] });
    s = queueReducer(s, { type: 'remove', index: 1 });
    expect(s.upNext.map((x) => x.id)).toEqual(['a', 'c']);
  });
});
