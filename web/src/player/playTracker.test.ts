import { describe, expect, it } from 'vitest';
import { finish, onTimeUpdate, pauseTracker, startTracker, type TrackerState } from './playTracker';

function playFor(state: TrackerState, seconds: number, step = 0.25, clipLength = 8) {
  const reports = [];
  let s = state;
  let pos = s.lastTime ?? 0;
  s = onTimeUpdate(s, pos, clipLength).state; // first timeupdate after play starts
  for (let i = 0; i < seconds / step; i++) {
    pos = (pos + step) % clipLength; // the demo clip loops
    const r = onTimeUpdate(s, pos, clipLength);
    s = r.state;
    if (r.report) reports.push(r.report);
    if (r.finished) break;
  }
  return { state: s, reports };
}

describe('play tracker', () => {
  it('reports once at 30 listened seconds, across clip loops', () => {
    const { state, reports } = playFor(startTracker('p1', 'trk', 200), 45);
    expect(reports).toHaveLength(1);
    expect(reports[0]).toMatchObject({ playId: 'p1', trackId: 'trk', reason: 'threshold', listenedSeconds: 30 });
    expect(state.listened).toBe(45);
  });

  it('does not report a 10 second listen until it ends, and then with the real time', () => {
    const { state, reports } = playFor(startTracker('p2', 'trk', 200), 10);
    expect(reports).toHaveLength(0);
    const end = finish(state, 'skipped');
    expect(end.report).toMatchObject({ reason: 'skipped', listenedSeconds: 10 });
    expect(finish(end.state, 'skipped').report).toBeUndefined();
  });

  it('ignores seeks and paused gaps', () => {
    let s = startTracker('p3', 'trk', 200);
    s = onTimeUpdate(s, 0).state;
    s = onTimeUpdate(s, 1).state;
    s = onTimeUpdate(s, 6).state; // seek forward 5 s
    s = pauseTracker(s);
    s = onTimeUpdate(s, 6.5).state; // first tick after resume
    s = onTimeUpdate(s, 7).state;
    expect(s.listened).toBeCloseTo(1.5, 5);
  });

  it('ends at the catalogue duration with one end report and never exceeds it', () => {
    const { state, reports } = playFor(startTracker('p4', 'trk', 40), 60);
    expect(reports.map((r) => r.reason)).toEqual(['threshold', 'ended']);
    expect(reports[1].listenedSeconds).toBe(40);
    expect(state.endSent).toBe(true);
  });
});
