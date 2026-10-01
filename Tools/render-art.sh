#!/usr/bin/env bash
# Rasterize the original geometric placeholder artwork using ImageMagick.
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p Textures/Things/Building/Minisplits
for spec in Small:128 Large:256; do
  name=${spec%:*}
  width=${spec#*:}
  convert -size "${width}x128" xc:none \
    -stroke '#30393b' -strokewidth 4 -fill '#b9c0bf' \
    -draw "roundrectangle 8,38 $((width-8)),91 9,9" \
    -fill '#e9e9dd' -draw "roundrectangle 8,30 $((width-8)),81 9,9" \
    -strokewidth 2 -fill '#3c484b' -draw "rectangle 14,67 $((width-14)),78" \
    -stroke '#929c9d' -draw "line 17,72 $((width-17)),72" \
    -stroke '#ffffff' -draw "line 18,40 $((width-18)),40" \
    -stroke '#30393b' -strokewidth 1.5 -fill '#73bbce' \
    -draw "roundrectangle $((width-34)),51 $((width-18)),57 2,2" \
    "PNG32:Textures/Things/Building/Minisplits/${name}.png"
done
