#!/bin/bash

# requires imagemagick (`apt install imagemagick` on ubuntu I think)
# splits all.png into number files with variable width and equal height

mkdir -p dist
maxheight=0

rm -f dist/number-*.png
convert -crop 10%x0 +repage "$1" "dist/number.png"
cd dist
for f in number-*.png; do
    height=`identify -format "%h" "$f"`
    if [ $height -gt $maxheight ]; then maxheight=$height; fi
    convert "$f" -trim +repage "$f"
done

for f in number-*.png; do
    convert "$f" -background transparent -gravity center -extent 0x$height "$f"
    mv "$f" "`echo -n "$f" | sed 's/number-//'`"
done
cd -