export function formatDuration(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds));
  return `${Math.floor(s / 60)}:${String(s % 60).padStart(2, '0')}`;
}

/** Splits text into parts, marking those whose accent-free form starts a word of the query. */
export function fold(text: string): string {
  return text.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
}

export function highlight(text: string, query: string): { text: string; match: boolean }[] {
  const words = fold(query).split(/[^a-z0-9]+/).filter(Boolean);
  if (words.length === 0) return [{ text, match: false }];
  const parts: { text: string; match: boolean }[] = [];
  const re = /[\p{L}\p{N}]+|[^\p{L}\p{N}]+/gu;
  for (const m of text.matchAll(re)) {
    const token = m[0];
    const folded = fold(token);
    const w = words.find((q) => folded.startsWith(q));
    if (w && /[\p{L}\p{N}]/u.test(token)) {
      // fold() keeps length for precomposed Latin letters, so the prefix length maps back to the original.
      const cut = Math.min(token.length, w.length);
      parts.push({ text: token.slice(0, cut), match: true });
      if (cut < token.length) parts.push({ text: token.slice(cut), match: false });
    } else {
      parts.push({ text: token, match: false });
    }
  }
  return parts;
}
