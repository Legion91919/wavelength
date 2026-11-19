export function SearchBox({ value, onChange }: { value: string; onChange: (q: string) => void }) {
  return (
    <label className="search">
      <span aria-hidden="true">&#9906;</span>
      <span className="sr-only">Search tracks, artists and albums</span>
      <input type="search" value={value} placeholder="Search tracks, artists, albums" autoFocus onChange={(e) => onChange(e.target.value)} />
    </label>
  );
}
