import type { Track } from '../api/types';

/** Client-side play queue. `playSeq` changes every time a new listen starts (each listen gets a new play ID). */
export interface QueueState {
  current: Track | null;
  upNext: Track[];
  playSeq: number;
}

export type QueueAction =
  | { type: 'play'; track: Track }
  | { type: 'playAll'; tracks: Track[] }
  | { type: 'playNext'; track: Track }
  | { type: 'enqueue'; tracks: Track[] }
  | { type: 'advance' }
  | { type: 'remove'; index: number };

export const emptyQueue: QueueState = { current: null, upNext: [], playSeq: 0 };

export function queueReducer(state: QueueState, action: QueueAction): QueueState {
  switch (action.type) {
    case 'play':
      return { ...state, current: action.track, playSeq: state.playSeq + 1 };
    case 'playAll': {
      const [first, ...rest] = action.tracks;
      if (!first) return state;
      return { current: first, upNext: [...rest, ...state.upNext], playSeq: state.playSeq + 1 };
    }
    case 'playNext':
      if (!state.current) return { ...state, current: action.track, playSeq: state.playSeq + 1 };
      return { ...state, upNext: [action.track, ...state.upNext] };
    case 'enqueue':
      if (!state.current && action.tracks.length > 0) {
        const [first, ...rest] = action.tracks;
        return { current: first, upNext: [...state.upNext, ...rest], playSeq: state.playSeq + 1 };
      }
      return { ...state, upNext: [...state.upNext, ...action.tracks] };
    case 'advance': {
      const [next, ...rest] = state.upNext;
      return { current: next ?? null, upNext: rest, playSeq: next ? state.playSeq + 1 : state.playSeq };
    }
    case 'remove':
      return { ...state, upNext: state.upNext.filter((_, i) => i !== action.index) };
  }
}
