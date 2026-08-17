#!/usr/bin/env bash
# tools/compare-warnings.sh
#
# Analyzer warning gate for task M0-07 -- POSIX sibling of
# tools/compare-warnings.ps1. The CI workflow (.github/workflows/ci.yml) runs
# on windows-latest and calls the .ps1; this script exists so the same gate can
# be run locally on Linux/macOS/git-bash without PowerShell, and so a future
# move to a Linux runner needs no new logic.
#
# Usage:
#   tools/compare-warnings.sh <build-log> <baseline-json> <target-key>
#   tools/compare-warnings.sh <build-log> <baseline-json> <target-key> --update
#
# THE COUNTING RULE (identical to the .ps1; see
# docs/kb/execution/ci-pipeline.md "Counting rule"):
#
#   An MSBuild console log at -v normal prints every diagnostic TWICE -- once
#   inline while the project builds (node-prefixed, e.g. "2>") and once in the
#   warning summary block after the "Build succeeded." / "Build FAILED."
#   marker. Confirmed 2026-08-17 on this repository: 13,386 raw
#   " warning <CODE>" lines vs MSBuild's own "6693 Warning(s)" -- exactly 2x.
#
#   This script counts ONLY the summary block after that marker, and asserts
#   that its size equals MSBuild's own "<n> Warning(s)" figure. If the two
#   disagree it exits 2 rather than gate on a number it does not understand.
#
#   NOT used: the raw line count (2x inflated); "divide by 2" (a magic number
#   that breaks when verbosity or node count changes); de-duplication by
#   diagnostic text (measured WRONG here -- 13,310 distinct lines vs 13,386
#   total, i.e. some diagnostics legitimately repeat with identical text).
#
# Ratchet semantics:
#   total > baseline total                     -> FAIL (exit 1)
#   a code absent from the baseline appears    -> FAIL (exit 1)
#   total < baseline total                     -> PASS + "lower the baseline"
#   total = baseline total                     -> PASS
# Exit 2 = the measurement itself could not be trusted.
#
# It never invokes -warnaserror and never edits a .csproj/.cs/.razor file.
#
# Baseline parsing: this script reads ci/warning-baseline.json with awk rather
# than jq, so it has no dependency beyond a POSIX shell. That requires the
# baseline to keep its canonical formatting -- one "CODE": N pair per line
# inside each target's "codes" object. ci/warning-baseline.json documents this;
# the .ps1 sibling uses a real JSON parser and is unaffected.

set -uo pipefail

log="${1:-}"
baseline="${2:-}"
target="${3:-}"
mode="${4:-compare}"

if [ -z "$log" ] || [ -z "$baseline" ] || [ -z "$target" ]; then
    echo "usage: tools/compare-warnings.sh <build-log> <baseline-json> <target-key> [--update]" >&2
    exit 2
fi

if [ ! -f "$log" ]; then
    echo "compare-warnings: build log not found: $log" >&2
    exit 2
fi

# --- extract MSBuild's own warning summary block ----------------------------
summary="$(awk 'f {print} /^[[:space:]]*Build (succeeded|FAILED)\.[[:space:]]*$/ && !f {f=1}' "$log")"
if [ -z "$summary" ]; then
    echo "compare-warnings: no 'Build succeeded.' / 'Build FAILED.' marker in $log." >&2
    echo "The log was probably captured below verbosity 'normal', or truncated." >&2
    echo "The counting rule depends on that block; refusing to guess." >&2
    exit 2
fi

measured_pairs="$(printf '%s\n' "$summary" \
    | grep -oE ': warning [A-Za-z]+[0-9]+:' \
    | sed -E 's/^: warning //; s/:$//' \
    | sort | uniq -c \
    | sort -k1,1nr -k2,2 \
    | awk '{print $2" "$1}')"

measured_total=0
if [ -n "$measured_pairs" ]; then
    measured_total="$(printf '%s\n' "$measured_pairs" | awk '{s+=$2} END {print s+0}')"
fi

reported="$(printf '%s\n' "$summary" | grep -oE '^[[:space:]]*[0-9]+ Warning\(s\)$' | tail -1 | grep -oE '[0-9]+' || true)"
if [ -z "$reported" ]; then
    echo "compare-warnings: could not find MSBuild's '<n> Warning(s)' line; refusing to gate on an unverified count." >&2
    exit 2
fi
if [ "$reported" != "$measured_total" ]; then
    echo "compare-warnings: COUNTING RULE BROKEN." >&2
    echo "  parsed from the summary block : $measured_total" >&2
    echo "  MSBuild's own Warning(s) line : $reported" >&2
    echo "These must be equal. Something about the log format changed. Fix the counting" >&2
    echo "rule in tools/compare-warnings.sh and .ps1, and re-document it in" >&2
    echo "ci/warning-baseline.json, before trusting this gate again." >&2
    exit 2
fi

if [ "$mode" = "--update" ]; then
    echo "\"total\": $measured_total,"
    echo "\"codes\": {"
    printf '%s\n' "$measured_pairs" | awk 'NR>1{printf ",\n"} {printf "  \"%s\": %s", $1, $2} END{printf "\n"}'
    echo "}"
    exit 0
fi

if [ ! -f "$baseline" ]; then
    echo "compare-warnings: baseline not found: $baseline" >&2
    exit 2
fi

# --- read the baseline target ----------------------------------------------
base_total="$(awk -v t="\"$target\"" '
    $0 ~ t"[[:space:]]*:[[:space:]]*\\{" {inside=1}
    inside && /"total"[[:space:]]*:/ {gsub(/[^0-9]/,"",$0); print; exit}
' "$baseline")"

base_codes="$(awk -v t="\"$target\"" '
    $0 ~ t"[[:space:]]*:[[:space:]]*\\{" {inside=1; next}
    inside && /"codes"[[:space:]]*:[[:space:]]*\{/ {incodes=1; next}
    incodes && /^[[:space:]]*\}/ {exit}
    incodes {
        line=$0
        gsub(/[",]/, "", line)
        gsub(/:/, " ", line)
        n=split(line, a, /[[:space:]]+/)
        code=""; cnt=""
        for (i=1;i<=n;i++) if (a[i] != "") { if (code=="") code=a[i]; else if (cnt=="") cnt=a[i] }
        if (code != "" && cnt != "") print code, cnt
    }
' "$baseline")"

if [ -z "$base_total" ]; then
    echo "compare-warnings: baseline $baseline has no target '$target'." >&2
    exit 2
fi

echo ""
echo "=== Analyzer warning gate -- $target ==="
echo "log        : $log"
echo "baseline   : $baseline (total $base_total)"
echo "measured   : $measured_total"

failed=0

new_codes="$(
    join -v1 \
        <(printf '%s\n' "$measured_pairs" | sort -k1,1) \
        <(printf '%s\n' "$base_codes"    | sort -k1,1) 2>/dev/null
)"

if [ -n "$new_codes" ]; then
    failed=1
    echo ""
    echo "=== FAIL -- warning code(s) not present in the baseline ==="
    printf '%s\n' "$new_codes" | awk '{printf "  %s  x%s   (baseline: absent)\n", $1, $2}'
    echo ""
    echo "A new warning CODE is a new class of defect, not more of an old one."
    echo "Fix it, or -- if it is genuinely acceptable -- add it to $baseline in its"
    echo "own reviewed commit that says why."
fi

if [ "$measured_total" -gt "$base_total" ]; then
    failed=1
    echo ""
    echo "=== FAIL -- warning total rose above the baseline ==="
    echo "  baseline $base_total, measured $measured_total, delta +$((measured_total - base_total))"
    echo ""
    echo "Codes that increased:"
    increased="$(join -a1 -e 0 -o 0,1.2,2.2 \
        <(printf '%s\n' "$measured_pairs" | sort -k1,1) \
        <(printf '%s\n' "$base_codes"    | sort -k1,1) \
        | awk '$2 > $3 {printf "  %s  %s -> %s  (+%s)\n", $1, $3, $2, $2-$3}')"
    if [ -n "$increased" ]; then
        printf '%s\n' "$increased"
    else
        echo "  (none -- every per-code count matches the baseline, so the baseline's"
        echo "   'total' field disagrees with the sum of its own 'codes'. Regenerate it"
        echo "   with --update rather than hand-editing the total.)"
    fi
fi

if [ "$failed" -ne 0 ]; then
    echo ""
    echo "=== Gate: FAILED ==="
    echo "This gate never uses -warnaserror. It compares against a committed baseline."
    echo "See docs/kb/execution/ci-pipeline.md -> 'How to read a failure'."
    exit 1
fi

if [ "$measured_total" -lt "$base_total" ]; then
    echo ""
    echo "=== PASS -- and the baseline is now stale (ratchet) ==="
    echo "  baseline $base_total, measured $measured_total, delta -$((base_total - measured_total))"
    echo ""
    echo "ACTION REQUIRED: lower the committed baseline so the improvement cannot be undone."
    echo "  1. Download the build log artefact from this run."
    echo "  2. tools/compare-warnings.sh <log> $baseline $target --update"
    echo "  3. Paste the printed 'total' and 'codes' into $baseline under targets.$target,"
    echo "     update generated_on/commit/date, and commit."
    echo "Warnings are allowed to fall. They are never allowed to rise."
    echo ""
    echo "=== Gate: PASSED (below baseline) ==="
    exit 0
fi

echo ""
echo "=== Gate: PASSED (equal to baseline) ==="
exit 0
