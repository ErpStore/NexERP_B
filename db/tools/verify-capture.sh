#!/usr/bin/env bash
# db/tools/verify-capture.sh
#
# Mechanically reconciles a delivered stored-procedure capture in
# db/stored-procedures/ against db/stored-procedures/manifest.csv (M0-01-01's
# worklist) and the conventions in db/stored-procedures/README.md.
#
# Requires NO database access and takes NO arguments. Must be run from the
# repository root:
#   bash db/tools/verify-capture.sh
#
# Exit code: 0 if every check passes, non-zero if any hard failure is found.
# Warnings (implausibly-short files, documented exceptions) do not fail the
# run but are printed and counted -- read them.
#
# This script does not know what a correct procedure BODY looks like; it
# cannot verify semantic faithfulness to the deployed object (there is no
# database connection here to compare against). It verifies everything that
# can be checked mechanically from the files alone: presence against the
# worklist, naming, the CREATE OR ALTER convention, encoding, line endings,
# and the absence of secret-shaped or tenant-identifying text.
#
# Tooling: bash, awk, sed, grep, dd, od, tr, wc only -- runs unmodified in
# Git Bash on Windows and in a Linux CI container, matching
# db/tools/sp-inventory.sh's portability goal.

set -uo pipefail
# Deliberately not `set -e`: we want every check to run and be counted, not
# to stop at the first failure.

MANIFEST="db/stored-procedures/manifest.csv"
README="db/stored-procedures/README.md"
SPDIR="db/stored-procedures"
STATUS_FILE="$SPDIR/CAPTURE-STATUS.md"

if [ ! -f "$MANIFEST" ] || [ ! -f "$README" ]; then
    echo "ERROR: $MANIFEST or $README not found." >&2
    echo "Run this script from the repository root (the directory containing db/)." >&2
    exit 2
fi

hard_failures=0
warnings=0

fail() { echo "    FAIL: $*"; hard_failures=$((hard_failures + 1)); }
warn() { echo "    WARN: $*"; warnings=$((warnings + 1)); }
ok()   { echo "    ok:   $*"; }

echo "=================================================================="
echo " verify-capture.sh -- db/stored-procedures/ vs $MANIFEST"
echo "=================================================================="

# ---------------------------------------------------------------------
# Strip block comments (/* ... */, possibly spanning lines) and line
# comments (-- ...) from stdin, then print the first N non-blank lines
# that remain. Used to find each file's first real SQL statement without
# needing a real T-SQL parser. Documented heuristic, not a parser --
# same spirit as db/tools/sp-inventory.sh's commented-line detection.
# ---------------------------------------------------------------------
first_statement_lines() {
    awk -v maxlines=5 '
        BEGIN { in_block = 0; n = 0 }
        {
            line = $0
            if (in_block) {
                idx = index(line, "*/")
                if (idx > 0) { line = substr(line, idx + 2); in_block = 0 }
                else next
            }
            out = ""
            while (1) {
                bidx = index(line, "/*")
                lcidx = index(line, "--")
                if (bidx > 0 && (lcidx == 0 || bidx < lcidx)) {
                    out = out substr(line, 1, bidx - 1)
                    rest = substr(line, bidx + 2)
                    eidx = index(rest, "*/")
                    if (eidx > 0) { line = substr(rest, eidx + 2); continue }
                    else { in_block = 1; line = ""; break }
                } else if (lcidx > 0) {
                    out = out substr(line, 1, lcidx - 1)
                    line = ""
                    break
                } else {
                    out = out line
                    line = ""
                    break
                }
            }
            gsub(/^[ \t]+|[ \t]+$/, "", out)
            if (out != "") {
                print out
                n++
                if (n >= maxlines) exit
            }
        }
    ' "$1"
}

# =======================================================================
# 1. Manifest reconciliation -- every `missing` row needs a file (or a
#    documented, owned exception in CAPTURE-STATUS.md).
# =======================================================================
echo
echo "-- 1. Manifest reconciliation --"

mapfile -t missing_names < <(awk -F',' 'NR>1 && $2=="missing" { print $1 }' "$MANIFEST")
missing_count=${#missing_names[@]}
echo "  'missing' rows in manifest: $missing_count"

declare -A expected
for n in "${missing_names[@]}"; do expected["$n"]=1; done

mapfile -t other_names < <(awk -F',' 'NR>1 && $2!="missing" { print $1 }' "$MANIFEST")
declare -A not_expected
for n in "${other_names[@]}"; do not_expected["$n"]=1; done

for n in "${missing_names[@]}"; do
    f="$SPDIR/$n.sql"
    if [ -f "$f" ]; then
        ok "$n.sql present"
    elif [ -f "$STATUS_FILE" ] && grep -qF -- "$n" "$STATUS_FILE"; then
        warn "$n.sql absent, but '$n' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. Confirm it actually has an owner and reason, not just a passing text match."
    else
        fail "$n.sql absent and '$n' is not mentioned anywhere in CAPTURE-STATUS.md -- undocumented gap"
    fi
done

# =======================================================================
# 2. Per-file checks on everything actually present in db/stored-procedures/
# =======================================================================
echo
echo "-- 2. Per-file checks --"

shopt -s nullglob
sql_files=("$SPDIR"/*.sql)
sql_file_count=${#sql_files[@]}
echo "  .sql files present in $SPDIR: $sql_file_count"

# Known tenant-hostname tokens (V.SMART/V.SMART.Shared/wwwroot/templates/*,
# the per-tenant FastReport template folders -- ADR-005). This is a fixed
# list from the current codebase, not a general tenant-name detector: a
# tenant added later would not be caught here. Flagged as a known
# limitation rather than assumed complete.
tenant_tokens=(acucom sns srinuenggind sharadaelectrou1 bhargavisofttech)

for f in "${sql_files[@]}"; do
    base=$(basename "$f")
    name="${base%.sql}"
    echo
    echo "  [$base]"

    # 2a. Authorized to be here at all (README: only 'missing' rows).
    if [ -n "${expected[$name]:-}" ]; then
        ok "authorized: 'missing' row in manifest"
    elif [ -n "${not_expected[$name]:-}" ]; then
        fail "'$name' has a manifest status other than 'missing' -- README forbids a .sql file here for scripted/case_mismatch/unreferenced rows"
    else
        fail "'$name' does not appear in manifest.csv at all -- not part of this task's worklist"
    fi

    # 2b. UTF-8 BOM.
    bom_hex=$(dd if="$f" bs=1 count=3 2>/dev/null | od -An -tx1 | tr -d ' \n')
    if [ "$bom_hex" = "efbbbf" ]; then
        fail "UTF-8 BOM present (must be UTF-8 without BOM)"
    else
        ok "no BOM"
    fi

    # 2c. CRLF / lone-CR line endings.
    cr_count=$(LC_ALL=C tr -cd '\r' < "$f" | wc -c | tr -d ' ')
    if [ "${cr_count:-0}" -gt 0 ]; then
        fail "$cr_count carriage-return byte(s) found (must be LF-only line endings)"
    else
        ok "LF line endings"
    fi

    # 2d. Truncation guard -- implausibly short is a WARN (human review),
    #     not an automatic hard failure, per the task spec.
    size=$(wc -c < "$f" | tr -d ' ')
    if [ "${size:-0}" -lt 100 ]; then
        warn "only $size bytes -- implausibly short for a stored procedure, flag for human review"
    else
        ok "size ${size} bytes"
    fi

    # 2e. Forbidden secret / connection-string / host-identifying patterns.
    if grep -InE '(Password[[:space:]]*=|User Id[[:space:]]*=|Data Source[[:space:]]*=|Server[[:space:]]*=|Initial Catalog[[:space:]]*=)' "$f" > /dev/null 2>&1; then
        fail "connection-string-shaped text found (Password=/User Id=/Data Source=/Server=/Initial Catalog=)"
    else
        ok "no connection-string-shaped text"
    fi
    if grep -InE '[0-9]{1,3}(\.[0-9]{1,3}){3}' "$f" > /dev/null 2>&1; then
        fail "IPv4-literal-shaped text found"
    else
        ok "no IPv4-literal-shaped text"
    fi
    if grep -InE '^[[:space:]]*USE[[:space:]]' "$f" > /dev/null 2>&1; then
        fail "USE statement found"
    else
        ok "no USE statement"
    fi
    # Whole-word match only (grep -w): a short token like "sns" must not
    # fire on a substring inside an unrelated identifier such as
    # "Sp_GetHSNSummaryReport" (which contains the literal characters
    # "sns" inside "HSNSummary" with no word boundary around them).
    # Caught by testing against a synthetic sample before this shipped --
    # see the verification notes in the task report.
    tenant_hit=""
    for t in "${tenant_tokens[@]}"; do
        if grep -qiw -- "$t" "$f"; then
            tenant_hit="$tenant_hit $t"
        fi
    done
    if [ -n "$tenant_hit" ]; then
        fail "known tenant-name token(s) found:$tenant_hit"
    else
        ok "no known tenant-name token"
    fi

    # 2f. First statement is CREATE OR ALTER PROCEDURE, and its declared
    #     name matches the file name.
    first_lines=$(first_statement_lines "$f")
    first_joined=$(echo "$first_lines" | tr '\n' ' ')
    if echo "$first_joined" | grep -qiE '^CREATE[[:space:]]+OR[[:space:]]+ALTER[[:space:]]+PROCEDURE'; then
        ok "first statement is CREATE OR ALTER PROCEDURE"
    else
        preview=$(echo "$first_joined" | cut -c1-80)
        fail "first non-comment statement is not CREATE OR ALTER PROCEDURE (saw: '$preview...')"
    fi

    declared=$(echo "$first_joined" \
        | grep -ioE 'PROCEDURE[[:space:]]+(\[?[A-Za-z0-9_]+\]?\.)?\[?[A-Za-z0-9_]+\]?' \
        | head -n1 \
        | sed -E 's/^[Pp][Rr][Oo][Cc][Ee][Dd][Uu][Rr][Ee][[:space:]]+//' \
        | sed -E 's/^\[?[A-Za-z0-9_]+\]?\.//' \
        | tr -d '[]')
    if [ -z "$declared" ]; then
        fail "could not find a declared procedure name after PROCEDURE keyword"
    elif [ "$declared" = "$name" ]; then
        ok "declared name matches file name ($declared)"
    else
        fail "declared name '$declared' does not match file name '$name'"
    fi
done

# =======================================================================
# 3. Arithmetic
# =======================================================================
echo
echo "-- 3. Arithmetic --"

extra_count=0
for f in "${sql_files[@]}"; do
    n=$(basename "$f" .sql)
    if [ -z "${expected[$n]:-}" ]; then
        extra_count=$((extra_count + 1))
    fi
done

echo "  manifest 'missing' rows           : $missing_count"
echo "  .sql files present                : $sql_file_count"
echo "  of which not an authorized 'missing' name (extras/unauthorized): $extra_count"
echo "  README permits zero extras -- any nonzero value above is also counted as a FAIL in section 2."

expected_present=0
for n in "${missing_names[@]}"; do
    [ -f "$SPDIR/$n.sql" ] && expected_present=$((expected_present + 1))
done
echo "  'missing' rows with a file actually present: $expected_present / $missing_count"

# =======================================================================
# Result
# =======================================================================
echo
echo "=================================================================="
echo " RESULT"
echo "=================================================================="
echo "  hard failures : $hard_failures"
echo "  warnings      : $warnings"
if [ "$hard_failures" -gt 0 ]; then
    echo "  verify-capture.sh: FAIL"
    exit 1
else
    echo "  verify-capture.sh: PASS"
    exit 0
fi
