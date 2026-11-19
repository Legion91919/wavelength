---
name: unity-game
description: Agentic workflow for Unity games - project rules for agents, editor/runtime assembly split, asset pipeline scripts, multi-skin builds, cache cleaning, UI checks on phone and tablet shapes, and play-mode tests. Use when working in a Unity project or building game tooling.
---

# Unity game projects

## Agent rules for a Unity repo (put the short version in AGENTS.md)
- Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or anything git-ignored. Never hand-edit `.meta` GUIDs.
- Scenes and prefabs are YAML but fragile: prefer changing them through editor scripts (`[MenuItem]` or `-executeMethod`) over text edits.
- State **what is live**: which scenes, managers and code paths are current, which are legacy. Agents waste most time on dead code.
- Split code into asmdefs: `Game.Runtime`, `Game.Editor` (editor-only tools), `Game.Tests` (EditMode + PlayMode).

## Automation
- **Batch mode builds**: `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildAndroid -logFile build.log`. Keep one `BuildScript` with a method per target/skin.
- **Skins / reskins**: drive themes from ScriptableObjects (colours, sprites, audio, strings) so a skin is data, not a code fork. Build skins in parallel from copies of the project if needed, and clean those copies afterwards.
- **Asset pipeline**: keep Python/C# tools in `Tools/` (slice sprite sheets, generate placeholder art, import 3D models, make previews). Each tool: one job, `--help`, idempotent, writes to a staging folder before `Assets/`.
- **Clean script**: deletes `Library/` caches, build outputs and APKs when disk is low; never touches `Assets/` or `ProjectSettings/`.

## UI quality
- uGUI: Canvas Scaler "Scale With Screen Size", match 0.5; anchor to edges; SafeArea component for notches.
- Check every screen at phone (9:19.5, 9:16) and tablet (3:4) shapes for overlapping or cut-off text, off-screen elements and buttons smaller than ~48 dp. If the `unity-ui-overlap-audit` skill is available, run it.

## Tests
- EditMode tests for pure game logic (scoring, progression, save data).
- PlayMode smoke test: load each scene, run 5 seconds, assert no errors in the log.
- Run in CI with `game-ci/unity-test-runner` when a licence is available.

## Learning from games (privately)
Extract only **generic** lessons into the kit (`lessons.md`, this skill). Never game names, art, story or code.
