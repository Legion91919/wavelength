import { useCallback, useEffect, useRef, useState } from 'react';
import { api, newPlayId } from '../api/client';
import type { PlayReason, PlayReport, Track } from '../api/types';
import { finish, onTimeUpdate, pauseTracker, startTracker, type TrackerState } from './playTracker';

export type CountedState = 'idle' | 'pending' | 'counted' | 'failed';
export type AudioStatus = 'empty' | 'loading' | 'playing' | 'paused' | 'error';

/**
 * Drives the HTML audio element for the current track. Demo clips are short, so the element loops them
 * until the track's catalogue duration has been listened to; then the play ends and the queue advances.
 */
export function usePlayer(current: Track | null, playSeq: number, onFinished: () => void) {
  const audioRef = useRef<HTMLAudioElement>(null);
  const tracker = useRef<TrackerState | null>(null);
  const [listened, setListened] = useState(0);
  const [status, setStatus] = useState<AudioStatus>('empty');
  const [counted, setCounted] = useState<CountedState>('idle');

  const send = useCallback((report: PlayReport) => {
    if (report.reason === 'threshold') setCounted('pending');
    api.postPlay(report).then(
      (r) => setCounted((c) => (r.counted ? 'counted' : c === 'pending' ? 'idle' : c)),
      () => setCounted((c) => (report.reason === 'threshold' || c === 'pending' ? 'failed' : c)),
    );
  }, []);

  const end = useCallback(
    (reason: PlayReason) => {
      if (!tracker.current) return;
      const step = finish(tracker.current, reason);
      tracker.current = step.state;
      if (step.report) send(step.report);
    },
    [send],
  );

  // A new listen: report the end of the previous one, start a fresh tracker with a new play ID.
  useEffect(() => {
    const audio = audioRef.current;
    if (!current || !audio) {
      setStatus('empty');
      return;
    }
    tracker.current = startTracker(newPlayId(), current.id, current.durationSeconds);
    setListened(0);
    setCounted('idle');
    setStatus('loading');
    audio.src = current.audioUrl;
    audio.loop = true;
    audio.play().catch(() => setStatus('paused'));
    return () => end('skipped');
  }, [current, playSeq, end]);

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;
    const onTime = () => {
      if (!tracker.current) return;
      const step = onTimeUpdate(tracker.current, audio.currentTime, audio.duration);
      tracker.current = step.state;
      setListened(step.state.listened);
      if (step.report) send(step.report);
      if (step.finished) {
        audio.pause();
        onFinished();
      }
    };
    const onPlaying = () => setStatus('playing');
    const onPause = () => {
      if (tracker.current) tracker.current = pauseTracker(tracker.current);
      setStatus((s) => (s === 'error' ? s : 'paused'));
    };
    const onError = () => setStatus('error');
    audio.addEventListener('timeupdate', onTime);
    audio.addEventListener('playing', onPlaying);
    audio.addEventListener('pause', onPause);
    audio.addEventListener('error', onError);
    return () => {
      audio.removeEventListener('timeupdate', onTime);
      audio.removeEventListener('playing', onPlaying);
      audio.removeEventListener('pause', onPause);
      audio.removeEventListener('error', onError);
    };
  }, [send, onFinished]);

  const toggle = useCallback(() => {
    const audio = audioRef.current;
    if (!audio || !current) return;
    if (audio.paused) audio.play().catch(() => setStatus('error'));
    else audio.pause();
  }, [current]);

  return { audioRef, listened, status, counted, toggle };
}
