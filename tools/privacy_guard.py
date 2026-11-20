#!/usr/bin/env python3
"""Privacy guard: stop private names, secrets and personal data reaching a public repo.

Usage:
  python tools/privacy_guard.py --staged          scan files staged for commit (pre-commit hook)
  python tools/privacy_guard.py --all             scan every tracked file (before making a repo public)
  python tools/privacy_guard.py --install         install as .git/hooks/pre-commit
  python tools/privacy_guard.py --paths a.md b.cs scan specific files
  python tools/privacy_guard.py --hash "Some Name"  print a hashed denylist entry (no plain text stored)

Private denylist (never committed), first one found wins:
  $AGENTIC_DENYLIST, ~/.agentic/denylist.txt, a _private/ folder next to the repo or one level up
One term per line, '#' for comments. Matching is case-insensitive and on word boundaries.
Prefer hashed entries ("sha256:<hex>" from --hash) so the denylist never contains the real names.
Hashed entries match 1-4 consecutive words with case, spaces and punctuation ignored, so
"Acme Widgets", "AcmeWidgets" and "acme-widgets" all hit the same entry.

Allow-list (committed, optional): <repo>/.privacy-allow, one term or email per line.
Only the standard library is used, so the hook works on any machine with Python 3.9+.
"""
from __future__ import annotations

import argparse
import hashlib
import os
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path

MAX_BYTES = 1_000_000
BINARY_EXT = {".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico", ".pdf", ".zip", ".dll", ".exe", ".so",
              ".dylib", ".bin", ".fbx", ".obj", ".wav", ".mp3", ".mp4", ".ttf", ".otf", ".woff", ".woff2",
              ".unity", ".asset", ".prefab", ".mat", ".anim", ".controller", ".meta", ".apk", ".aab"}

SECRET_PATTERNS: list[tuple[str, re.Pattern[str]]] = [
    ("private key", re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH |DSA |PGP )?PRIVATE KEY")),
    ("AWS access key", re.compile(r"\b(?:AKIA|ASIA)[0-9A-Z]{16}\b")),
    ("GitHub token", re.compile(r"\b(?:ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{36,}\b|\bgithub_pat_[A-Za-z0-9_]{40,}\b")),
    ("Anthropic API key", re.compile(r"\bsk-ant-[A-Za-z0-9_\-]{20,}")),
    ("OpenAI-style API key", re.compile(r"\bsk-(?:proj-)?[A-Za-z0-9_\-]{32,}")),
    ("Google API key", re.compile(r"\bAIza[0-9A-Za-z_\-]{35}\b")),
    ("Slack token", re.compile(r"\bxox[abprs]-[A-Za-z0-9-]{10,}")),
    ("Azure storage key", re.compile(r"AccountKey=[A-Za-z0-9+/=]{40,}")),
    ("connection string password", re.compile(r"(?i)\b(?:password|pwd)\s*=\s*(?!\s*[;\"'<{$]|\*+|changeme|example|your)[^;\"'\s]{4,}")),
]
EMAIL = re.compile(r"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")
SA_ID = re.compile(r"\b\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])\d{4}[01]\d{2}\b")
DEFAULT_ALLOWED_EMAIL_DOMAINS = ("example.com", "example.org", "example.net", "anthropic.com", "users.noreply.github.com", "contoso.com", "fabrikam.com")


@dataclass(frozen=True)
class Finding:
    path: str
    line: int
    reason: str
    excerpt: str

    def __str__(self) -> str:
        return f"{self.path}:{self.line}: {self.reason}: {self.excerpt}"


def luhn_ok(digits: str) -> bool:
    total = 0
    for i, ch in enumerate(reversed(digits)):
        d = int(ch)
        if i % 2 == 1:
            d *= 2
            if d > 9:
                d -= 9
        total += d
    return total % 10 == 0


def mask(s: str) -> str:
    return s if len(s) <= 6 else s[:3] + "*" * (len(s) - 5) + s[-2:]


def load_terms(path: Path | None) -> list[str]:
    if path is None or not path.is_file():
        return []
    out = []
    for raw in path.read_text(encoding="utf-8", errors="replace").splitlines():
        term = raw.split("#", 1)[0].strip()
        if len(term) >= 3:
            out.append(term)
    return out


def normalise(term: str) -> str:
    return re.sub(r"[^a-z0-9]", "", term.lower())


def hash_term(term: str) -> str:
    return "sha256:" + hashlib.sha256(normalise(term).encode("utf-8")).hexdigest()


def split_terms(terms: list[str]) -> tuple[list[str], frozenset[str]]:
    plain = [t for t in terms if not t.lower().startswith("sha256:")]
    hashed = frozenset(t.lower() for t in terms if t.lower().startswith("sha256:"))
    return plain, hashed


WORD = re.compile(r"[A-Za-z0-9][A-Za-z0-9:_\-]*")


def hashed_hits(line: str, hashes: frozenset[str]) -> list[str]:
    words = WORD.findall(line)
    hits = []
    for n in range(1, 5):
        for i in range(len(words) - n + 1):
            phrase = " ".join(words[i:i + n])
            if len(normalise(phrase)) >= 3 and hash_term(phrase) in hashes:
                hits.append(phrase)
    return hits


def find_denylist(repo: Path) -> Path | None:
    candidates = [os.environ.get("AGENTIC_DENYLIST", ""),
                  str(Path.home() / ".agentic" / "denylist.txt"),
                  str(repo.parent / "_private" / "denylist.txt"),
                  str(repo.parent.parent / "_private" / "denylist.txt")]
    for c in candidates:
        if c and Path(c).is_file():
            return Path(c)
    return None


def compile_terms(terms: list[str]) -> re.Pattern[str] | None:
    if not terms:
        return None
    alts = sorted((re.escape(t) for t in terms), key=len, reverse=True)
    return re.compile(r"(?i)(?<![A-Za-z0-9])(?:" + "|".join(alts) + r")(?![A-Za-z0-9])")


def scan_text(path: str, text: str, deny: re.Pattern[str] | None, allow: set[str],
              hashes: frozenset[str] = frozenset()) -> list[Finding]:
    findings: list[Finding] = []
    allow_lower = {a.lower() for a in allow}
    for n, line in enumerate(text.splitlines(), start=1):
        if "privacy-guard: ignore" in line:
            continue
        if deny:
            for m in deny.finditer(line):
                if m.group(0).lower() not in allow_lower:
                    findings.append(Finding(path, n, "private term from denylist", mask(m.group(0))))
        if hashes:
            for phrase in hashed_hits(line, hashes):
                findings.append(Finding(path, n, "private term from denylist", mask(phrase)))
        for reason, pat in SECRET_PATTERNS:
            for m in pat.finditer(line):
                findings.append(Finding(path, n, reason, mask(m.group(0))))
        for m in EMAIL.finditer(line):
            email = m.group(0).lower()
            domain = email.rsplit("@", 1)[1]
            if email in allow_lower or domain in allow_lower or domain.endswith(DEFAULT_ALLOWED_EMAIL_DOMAINS):
                continue
            findings.append(Finding(path, n, "email address", mask(email)))
        for m in SA_ID.finditer(line):
            if luhn_ok(m.group(0)):
                findings.append(Finding(path, n, "possible SA ID number", mask(m.group(0))))
    return findings


def git(repo: Path, *args: str) -> str:
    return subprocess.run(["git", *args], cwd=repo, capture_output=True, text=True, check=True).stdout


def read_staged(repo: Path, rel: str) -> str | None:
    try:
        data = subprocess.run(["git", "show", f":{rel}"], cwd=repo, capture_output=True, check=True).stdout
    except subprocess.CalledProcessError:
        return None
    return None if b"\x00" in data[:8000] or len(data) > MAX_BYTES else data.decode("utf-8", errors="replace")


def read_disk(path: Path) -> str | None:
    try:
        if path.stat().st_size > MAX_BYTES:
            return None
        data = path.read_bytes()
    except OSError:
        return None
    return None if b"\x00" in data[:8000] else data.decode("utf-8", errors="replace")


def is_scannable(rel: str) -> bool:
    p = Path(rel)
    return p.suffix.lower() not in BINARY_EXT and p.name not in {"denylist.txt"}


def install(repo: Path) -> int:
    hooks = repo / ".git" / "hooks"
    if not hooks.is_dir():
        print("Not a git repository (no .git/hooks).", file=sys.stderr)
        return 2
    hook = hooks / "pre-commit"
    script = Path(__file__).resolve().relative_to(repo).as_posix()
    # Always LF: Git's sh can't run a hook whose shebang line ends in CRLF (Windows' default for write_text).
    # Run the guard exactly once (a "||" fallback would re-run it after a block). Pick the first Python that
    # actually runs: on Windows "python3" is often the Microsoft Store placeholder, so probe instead of trusting PATH.
    hook.write_bytes((f"#!/bin/sh\nfor py in python3 python py; do\n"
                      f"  if \"$py\" -c \"\" >/dev/null 2>&1; then exec \"$py\" \"{script}\" --staged; fi\n"
                      f"done\necho \"privacy-guard: no working Python found\" >&2\nexit 1\n").encode("utf-8"))
    try:
        hook.chmod(0o755)
    except OSError:
        pass
    print(f"Installed pre-commit hook -> {script} --staged")
    return 0


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    mode = ap.add_mutually_exclusive_group(required=True)
    mode.add_argument("--staged", action="store_true")
    mode.add_argument("--all", action="store_true")
    mode.add_argument("--install", action="store_true")
    mode.add_argument("--paths", nargs="+")
    mode.add_argument("--hash", nargs="+", metavar="TERM")
    ap.add_argument("--repo", default=".")
    ap.add_argument("--denylist")
    args = ap.parse_args(argv)

    repo = Path(args.repo).resolve()
    if args.install:
        return install(repo)
    if args.hash:
        for t in args.hash:
            print(hash_term(t))
        return 0

    deny_path = Path(args.denylist) if args.denylist else find_denylist(repo)
    plain, hashes = split_terms(load_terms(deny_path))
    deny = compile_terms(plain)
    allow = set(load_terms(repo / ".privacy-allow"))
    if deny is None and not hashes:
        print("privacy-guard: no private denylist found; checking secrets and personal data only.", file=sys.stderr)

    findings: list[Finding] = []
    if args.staged:
        files = [f for f in git(repo, "diff", "--cached", "--name-only", "--diff-filter=ACMR").splitlines() if is_scannable(f)]
        for rel in files:
            text = read_staged(repo, rel)
            if text is not None:
                findings += scan_text(rel, text, deny, allow, hashes)
    else:
        if args.all:
            try:
                files = git(repo, "ls-files").splitlines()
            except (subprocess.CalledProcessError, FileNotFoundError):
                skip = {".git", "node_modules", "bin", "obj", "Library", "Temp", ".vs"}
                files = [str(p.relative_to(repo)) for p in repo.rglob("*") if p.is_file() and not skip & set(p.relative_to(repo).parts)]
        else:
            files = args.paths
        for rel in files:
            if is_scannable(rel):
                text = read_disk(repo / rel)
                if text is not None:
                    findings += scan_text(rel, text, deny, allow, hashes)

    for f in findings:
        print(f)
    if findings:
        print(f"\nprivacy-guard: {len(findings)} finding(s). Remove them, add a term to .privacy-allow, "
              "or mark a deliberate line with 'privacy-guard: ignore'.", file=sys.stderr)
        return 1
    print("privacy-guard: clean.", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
