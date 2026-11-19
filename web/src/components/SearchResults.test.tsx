import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type { SearchResponse } from '../api/types';
import { SearchResults } from './SearchResults';

const noop = () => {};
const handlers = { onPlay: noop, onPlayNext: noop, onPlayArtist: noop, onQueueAlbum: noop, onRetry: noop };

const result: SearchResponse = {
  query: 'meridien',
  tookMs: 0.4,
  tracks: [{ id: 'trk-salt-road', title: 'Salt Road Lullaby', artistId: 'art-dune', artistName: 'Dune Méridien', albumId: 'alb-meridien', albumTitle: 'Méridien Nights', labelId: 'lbl-saltpan', labelName: 'Saltpan Sounds', durationSeconds: 178, audioUrl: '/cdn/audio/x.mp3' }],
  artists: [{ id: 'art-dune', name: 'Dune Méridien', labelId: 'lbl-saltpan', labelName: 'Saltpan Sounds', trackCount: 3 }],
  albums: [{ id: 'alb-meridien', title: 'Méridien Nights', artistId: 'art-dune', artistName: 'Dune Méridien', labelName: 'Saltpan Sounds', year: 2026, trackCount: 3 }],
};

describe('SearchResults', () => {
  it('shows loading, empty and error states', () => {
    const onRetry = vi.fn();
    const { rerender } = render(<SearchResults state={{ kind: 'loading' }} {...handlers} />);
    expect(screen.getByText(/Searching/)).toBeTruthy();

    rerender(<SearchResults state={{ kind: 'success', result: { ...result, tracks: [], artists: [], albums: [], query: 'xqzzt' } }} {...handlers} />);
    expect(screen.getByText('No matches for "xqzzt"')).toBeTruthy();

    rerender(<SearchResults state={{ kind: 'error', message: 'Service Unavailable' }} {...handlers} onRetry={onRetry} />);
    expect(screen.getByRole('alert')).toBeTruthy();
    fireEvent.click(screen.getByText('Try again'));
    expect(onRetry).toHaveBeenCalledOnce();
  });

  it('groups results, highlights accent-insensitive matches and wires play / play next', () => {
    const onPlay = vi.fn();
    const onPlayNext = vi.fn();
    render(<SearchResults state={{ kind: 'success', result }} {...handlers} onPlay={onPlay} onPlayNext={onPlayNext} />);

    expect(screen.getByRole('heading', { name: 'Tracks' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Artists' })).toBeTruthy();
    expect(screen.getByRole('heading', { name: 'Albums' })).toBeTruthy();
    expect(screen.getAllByText('Méridien').some((el) => el.tagName === 'MARK')).toBe(true);

    fireEvent.click(screen.getByRole('button', { name: 'Play Salt Road Lullaby' }));
    fireEvent.click(screen.getByRole('button', { name: 'Play Salt Road Lullaby next' }));
    expect(onPlay).toHaveBeenCalledWith(result.tracks[0]);
    expect(onPlayNext).toHaveBeenCalledWith(result.tracks[0]);
  });
});
