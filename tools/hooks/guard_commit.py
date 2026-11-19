#!/usr/bin/env python3
"""Claude Code PreToolUse hook: run the privacy guard before any `git commit` an agent makes.

Exit code 2 blocks the tool call and shows the findings to the agent, so it can fix them itself.
Works even when the git pre-commit hook was never installed.
"""
import json
import subprocess
import sys
from pathlib import Path

try:
    payload = json.load(sys.stdin)
except json.JSONDecodeError:
    sys.exit(0)

command = (payload.get("tool_input") or {}).get("command", "")
if "git commit" not in command and "git push" not in command:
    sys.exit(0)

repo = Path(payload.get("cwd") or ".").resolve()
guard = Path(__file__).resolve().parents[1] / "privacy_guard.py"
mode = "--staged" if "git commit" in command else "--all"
result = subprocess.run([sys.executable, str(guard), mode, "--repo", str(repo)], capture_output=True, text=True)
if result.returncode == 1:
    print("Privacy guard blocked this:\n" + result.stdout + result.stderr, file=sys.stderr)
    sys.exit(2)
sys.exit(0)
