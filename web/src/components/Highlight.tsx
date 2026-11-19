import { highlight } from '../format';

export function Highlight({ text, query }: { text: string; query: string }) {
  return (
    <>
      {highlight(text, query).map((p, i) => (p.match ? <mark key={i}>{p.text}</mark> : <span key={i}>{p.text}</span>))}
    </>
  );
}
