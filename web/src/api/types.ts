export interface Track {
  id: string;
  title: string;
  artistId: string;
  artistName: string;
  albumId: string;
  albumTitle: string;
  labelId: string;
  labelName: string;
  durationSeconds: number;
  audioUrl: string;
}

export interface Artist { id: string; name: string; labelId: string; labelName: string; trackCount: number }
export interface Album { id: string; title: string; artistId: string; artistName: string; labelName: string; year: number; trackCount: number }

export interface SearchResponse { query: string; tracks: Track[]; artists: Artist[]; albums: Album[]; tookMs: number }

export type PlayReason = 'threshold' | 'ended' | 'skipped';
export interface PlayReport { playId: string; trackId: string; listenedSeconds: number; reason: PlayReason }
export interface PlayEventResponse { playId: string; trackId: string; listenedSeconds: number; counted: boolean; countedNow: boolean }
