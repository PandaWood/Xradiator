#!/usr/bin/env bash
set -euo pipefail

PROJECT="src/Xradiator/Xradiator.csproj"
OUT="publish"
RIDS=(win-x64 osx-x64 osx-arm64 linux-x64)

rm -rf "$OUT"

for RID in "${RIDS[@]}"; do
  echo "--- Publishing $RID ---"
  dotnet publish "$PROJECT" \
    -c Release \
    -r "$RID" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o "$OUT/$RID"
done

echo ""
echo "Done. Artifacts:"
for RID in "${RIDS[@]}"; do
  echo "  $OUT/$RID/"
done
