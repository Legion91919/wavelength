import type { PlayReason, PlayReport } from '../api/types';

export const QUALIFYING_SECONDS = 30;
/** A jump in currentTime bigger than this between two timeupdate events is a seek, not listening. */
const MAX_TICK_SECONDS = 1.5;

/**
 * Pure listening-time tracker for one listen (one play ID). Counts time actually heard: seeks are ignored
 * and looping demo clips keep counting. Reports at most once at the threshold and once at the end.
 */
export interface TrackerState {
  playId: string;
  trackId: string;
  durationSeconds: number;
  listened: number;
  lastTime: number | null;
  thresholdSent: boolean;
  endSent: boolean;
}

export interface TrackerStep {
  state: TrackerState;
  report?: PlayReport;
  finished: boolean;
}

export function startTracker(playId: string, trackId: string, durationSeconds: number): TrackerState {
  return { playId, trackId, durationSeconds, listened: 0, lastTime: null, thresholdSent: false, endSent: false };
}

/** Call when playback pauses so the next tick does not count the paused gap. */
export function pauseTracker(state: TrackerState): TrackerState {
  return { ...state, lastTime: null };
}

/** `clipSeconds` (the audio element's duration) lets a loop wrap count the tail end of the clip too. */
export function onTimeUpdate(state: TrackerState, currentTime: number, clipSeconds?: number): TrackerStep {
  if (state.endSent) return { state, finished: true };
  let delta = 0;
  if (state.lastTime !== null) {
    delta = currentTime - state.lastTime;
    if (delta < 0) {
      // The clip looped (small currentTime again) or the listener seeked back.
      const tail = clipSeconds && Number.isFinite(clipSeconds) ? Math.max(0, clipSeconds - state.lastTime) : 0;
      delta = currentTime <= MAX_TICK_SECONDS ? tail + currentTime : 0;
    }
    if (delta > MAX_TICK_SECONDS) delta = 0; // seek forward
  }
  const listened = Math.min(state.listened + delta, state.durationSeconds);
  let next: TrackerState = { ...state, listened, lastTime: currentTime };

  if (listened >= state.durationSeconds) {
    return finish(next, 'ended');
  }
  if (!next.thresholdSent && listened >= QUALIFYING_SECONDS) {
    next = { ...next, thresholdSent: true };
    return { state: next, report: report(next, 'threshold'), finished: false };
  }
  return { state: next, finished: false };
}

export function finish(state: TrackerState, reason: PlayReason): TrackerStep {
  if (state.endSent) return { state, finished: true };
  const next = { ...state, endSent: true, thresholdSent: state.thresholdSent || state.listened >= QUALIFYING_SECONDS };
  // Nothing was heard (e.g. skipped instantly): there is nothing worth reporting.
  if (Math.floor(next.listened) === 0) return { state: next, finished: true };
  return { state: next, report: report(next, reason), finished: true };
}

function report(state: TrackerState, reason: PlayReason): PlayReport {
  return { playId: state.playId, trackId: state.trackId, listenedSeconds: Math.floor(state.listened), reason };
}
