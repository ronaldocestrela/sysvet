#!/usr/bin/env bash
set -euo pipefail

COVERAGE_FILE="${1:-coveragereport/Cobertura.xml}"
MIN_LINE_COVERAGE="${MIN_LINE_COVERAGE:-70}"

if [[ ! -f "$COVERAGE_FILE" ]]; then
  echo "Coverage file not found: $COVERAGE_FILE" >&2
  exit 1
fi

line_rate="$(python3 - "$COVERAGE_FILE" <<'PY'
import sys
import xml.etree.ElementTree as ET

root = ET.parse(sys.argv[1]).getroot()
rate = float(root.attrib.get("line-rate", "0"))
print(f"{rate * 100:.2f}")
PY
)"

echo "Domain + Application line coverage: ${line_rate}% (minimum ${MIN_LINE_COVERAGE}%)"

awk -v rate="$line_rate" -v min="$MIN_LINE_COVERAGE" 'BEGIN {
  if (rate + 0 < min + 0) {
    printf("Coverage gate failed: %.2f%% < %.2f%%\n", rate, min) > "/dev/stderr"
    exit 1
  }
  printf("Coverage gate passed: %.2f%% >= %.2f%%\n", rate, min)
}'
