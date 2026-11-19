#!/usr/bin/env bash
# Generates the invented demo clips served from /cdn/audio (short tones and chords, no samples).
# Needs ffmpeg. Output is small mono MP3 (~50-70 KB each). Re-run only if you change the recipes.
set -euo pipefail
OUT="$(dirname "$0")/../src/Wavelength.Api/wwwroot/cdn/audio"
mkdir -p "$OUT"
command -v ffmpeg >/dev/null || { echo "ffmpeg not found; clips are committed, nothing to do"; exit 0; }

# name | seconds | expression (t = time). A soft attack/decay envelope is applied to every clip.
clip() {
  local name=$1 secs=$2 expr=$3
  local env="min(1\,t/0.4)*min(1\,($secs-t)/1.2)"
  ffmpeg -hide_banner -loglevel error -y -f lavfi \
    -i "aevalsrc=0.22*($expr)*$env:s=44100:d=$secs" \
    -ac 1 -codec:a libmp3lame -b:a 64k "$OUT/$name.mp3"
}

clip wl-01-dusk-chord     8 "sin(2*PI*220*t)+sin(2*PI*277.18*t)+sin(2*PI*329.63*t)"
clip wl-02-harbour-fifths 7 "sin(2*PI*196*t)+sin(2*PI*293.66*t)*(0.6+0.4*sin(2*PI*0.5*t))"
clip wl-03-lantern-pulse  9 "(sin(2*PI*261.63*t)+sin(2*PI*392*t))*(0.55+0.45*sin(2*PI*2*t))"
clip wl-04-salt-road-drone 8 "sin(2*PI*110*t)+0.5*sin(2*PI*165*t)+0.3*sin(2*PI*220.5*t)"
clip wl-05-kelp-arpeggio  6 "sin(2*PI*(if(lt(mod(t\,1)\,0.33)\,293.66\,if(lt(mod(t\,1)\,0.66)\,369.99\,440)))*t)"
clip wl-06-signal-bells   7 "sin(2*PI*523.25*t)*exp(-2*mod(t\,1.75))+sin(2*PI*659.25*t)*exp(-2*mod(t+0.875\,1.75))"
ls -l "$OUT"
