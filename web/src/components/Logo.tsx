export function Logo() {
  return (
    <div className="logo">
      <svg width="30" height="30" viewBox="0 0 28 28" aria-hidden="true">
        <circle cx="14" cy="14" r="13" fill="none" stroke="var(--color-accent)" strokeWidth="2" />
        <path d="M4 14c2.5-7 5-7 7.5 0s5 7 7.5 0 4-5 5-3" fill="none" stroke="var(--color-signal)" strokeWidth="2.2" strokeLinecap="round" />
      </svg>
      <span>wavelength</span>
    </div>
  );
}
