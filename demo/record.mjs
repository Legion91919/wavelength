#!/usr/bin/env node
// Records a captioned walkthrough of a running app: MP4 + GIF + desktop and mobile screenshots.
//
//   node record.mjs <repo-root>
//
// The repo provides:
//   demo/demo.config.json   how to start the app and where it lives
//   demo/scenario.mjs       export default async ({ page, step, shot, caption, pause }) => { ... }
//                           optional: export async function mobile({ page, shot }) { ... }
//
// Output goes to <repo>/docs/media (demo.mp4, demo.gif, *.png). Needs ffmpeg on PATH.
import { chromium } from "playwright";
import { spawn, execFileSync } from "node:child_process";
import { mkdirSync, readFileSync, renameSync, rmSync, readdirSync, statSync, existsSync } from "node:fs";
import { join, resolve } from "node:path";
import { pathToFileURL } from "node:url";

const repo = resolve(process.argv[2] ?? ".");
const cfg = JSON.parse(readFileSync(join(repo, "demo/demo.config.json"), "utf8"));
const out = join(repo, cfg.out ?? "docs/media");
const viewport = cfg.viewport ?? { width: 1280, height: 800 };
const scenarioModule = await import(pathToFileURL(join(repo, "demo/scenario.mjs")).href);
mkdirSync(out, { recursive: true });

const procs = [];
function start({ name, cmd, cwd = ".", env = {} }) {
  const p = spawn(cmd, { cwd: join(repo, cwd), env: { ...process.env, ...env }, shell: true, detached: true, stdio: ["ignore", "pipe", "pipe"] });
  let log = "";
  p.stdout.on("data", d => (log += d)); p.stderr.on("data", d => (log += d));
  procs.push({ name, p, log: () => log });
  return p;
}
async function waitFor(url, seconds = 180) {
  const until = Date.now() + seconds * 1000;
  while (Date.now() < until) {
    try { const r = await fetch(url); if (r.status < 500) return; } catch { /* not up yet */ }
    await new Promise(r => setTimeout(r, 1000));
  }
  throw new Error(`Timed out waiting for ${url}`);
}
function stopAll() { for (const { p } of procs) { try { process.kill(-p.pid, "SIGTERM"); } catch { /* already gone */ } } }
process.on("exit", stopAll);
process.on("SIGINT", () => { stopAll(); process.exit(130); });

// Caption bar drawn inside the page so it is part of the recording.
async function caption(page, text) {
  await page.evaluate(t => {
    let el = document.getElementById("__demo_caption");
    if (!el) {
      el = document.createElement("div");
      el.id = "__demo_caption";
      el.setAttribute("aria-hidden", "true");
      Object.assign(el.style, {
        position: "fixed", left: "50%", bottom: "22px", transform: "translateX(-50%)", zIndex: 2147483647,
        background: "rgba(16,24,22,.88)", color: "#fff", font: "600 17px/1.35 system-ui, sans-serif",
        padding: "10px 18px", borderRadius: "10px", maxWidth: "80%", textAlign: "center", pointerEvents: "none",
        boxShadow: "0 6px 24px rgba(0,0,0,.25)",
      });
      document.body.appendChild(el);
    }
    el.textContent = t; el.style.display = t ? "block" : "none";
  }, text);
}

let failed = null;
try {
  for (const s of cfg.start ?? []) { start(s); }
  for (const s of cfg.start ?? []) { if (s.ready) { await waitFor(s.ready, s.timeoutSeconds ?? 180); console.log(`ready: ${s.name}`); } }

  const browser = await chromium.launch();
  const tmp = join(out, ".video");
  rmSync(tmp, { recursive: true, force: true });
  const context = await browser.newContext({ viewport, recordVideo: { dir: tmp, size: viewport }, deviceScaleFactor: 1 });
  const page = await context.newPage();
  // Re-draw the caption after every navigation.
  let current = "";
  page.on("load", () => { if (current) caption(page, current).catch(() => {}); });
  const helpers = {
    page,
    pause: (ms = 1200) => page.waitForTimeout(ms),
    caption: async t => { current = t; await caption(page, t); },
    shot: async name => {
      await caption(page, "");
      await page.screenshot({ path: join(out, `${name}.png`) });
      if (current) await caption(page, current);
    },
    step: async (title, fn) => {
      current = title; await caption(page, title); await page.waitForTimeout(900);
      if (fn) await fn();
      await page.waitForTimeout(cfg.stepPauseMs ?? 1400);
    },
  };
  await page.goto(cfg.url, { waitUntil: "networkidle" });
  await scenarioModule.default(helpers);
  await caption(page, ""); await page.waitForTimeout(600);
  const video = page.video();
  await context.close();
  const webm = await video.path();

  // Mobile screenshots
  const m = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, isMobile: true, hasTouch: true });
  const mp = await m.newPage();
  await mp.goto(cfg.mobileUrl ?? cfg.url, { waitUntil: "networkidle" });
  const mshot = async name => mp.screenshot({ path: join(out, `${name}.png`) });
  if (scenarioModule.mobile) await scenarioModule.mobile({ page: mp, shot: mshot, pause: (ms = 800) => mp.waitForTimeout(ms) });
  else { await mp.waitForTimeout(1200); await mshot("mobile"); }
  await m.close();
  await browser.close();

  // Encode: MP4 for quality, GIF so GitHub READMEs autoplay it.
  const mp4 = join(out, "demo.mp4"), gif = join(out, "demo.gif");
  execFileSync("ffmpeg", ["-y", "-loglevel", "error", "-i", webm, "-c:v", "libx264", "-preset", "veryfast", "-crf", "28",
    "-pix_fmt", "yuv420p", "-movflags", "+faststart", "-an", mp4]);
  const width = cfg.gifWidth ?? 960, fps = cfg.gifFps ?? 8;
  execFileSync("ffmpeg", ["-y", "-loglevel", "error", "-i", webm, "-vf",
    `fps=${fps},scale=${width}:-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=4`, gif]);
  rmSync(tmp, { recursive: true, force: true });
  for (const f of readdirSync(out)) console.log(`${f}\t${Math.round(statSync(join(out, f)).size / 1024)} KB`);
  if (statSync(gif).size > 9 * 1024 * 1024) console.warn("GIF is over 9 MB: shorten the scenario or lower gifWidth/gifFps.");
} catch (e) {
  failed = e;
  console.error(String(e));
  for (const { name, log } of procs) console.error(`--- ${name} log (tail) ---\n${log().slice(-3000)}`);
} finally {
  stopAll();
}
process.exit(failed ? 1 : 0);
