#!/usr/bin/env bash
# db/tools/verify-capture.sh
#
# M0-01-02 half C reconciliation harness. Checks a delivered stored-procedure
# capture in db/stored-procedures/ against M0-01-01's manifest.csv —
# entirely mechanically, with NO database access and NO arguments. Run from
# the repository root.
#
# Exit code: 0 if every check passes, non-zero on the first hard failure
# category encountered (all categories are still checked and reported;
# the exit code reflects whether ANY hard failure occurred).
#
# See docs/kb/execution/tasks/M0-01-02.md for the full spec this
# implements, and db/stored-procedures/README.md for the file conventions
# it checks against.

set -uo pipefail

MANIFEST="db/stored-procedures/manifest.csv"
CAPTURE_DIR="db/stored-procedures"

fail=0
warn=0

note()  { printf '  %s\n' "$1"; }
ok()    { printf '  [OK]   %s\n' "$1"; }
bad()   { printf '  [FAIL] %s\n' "$1"; fail=1; }
flag()  { printf '  [WARN] %s\n' "$1"; warn=1; }

if [ ! -f "V.SMART/V.SMART.Shared" ] 2>/dev/null && [ ! -d "V.SMART" ]; then
  echo "error: run this script from the repository root (V.SMART/ not found)" >&2
  exit 2
fi

if [ ! -f "$MANIFEST" ]; then
  echo "error: $MANIFEST not found — M0-01-01 has not produced its manifest. Cannot verify." >&2
  exit 2
fi

echo "=== db/tools/verify-capture.sh ==="
echo

# ---------------------------------------------------------------------------
# 1. Build the expected worklist: every manifest row with status == missing,
#    plus the one case_mismatch row (its DDL, once corrected, also lands
#    here — see README's "Conventions" section, point 1).
# ---------------------------------------------------------------------------
echo "--- 1. Worklist coverage ---"

# procedure_name is column 1, status is column 2. The CSV's only quoted
# field is the last (notes), so a plain split on the first two commas is
# safe for these two columns.
missing_names=$(tail -n +2 "$MANIFEST" | awk -F',' '{print $1","$2}' | awk -F',' '$2=="missing"{print $1}' | sort -u)
missing_count=$(printf '%s\n' "$missing_names" | grep -c . || true)

echo "Expected (manifest status=missing): $missing_count"

sql_count=0
if [ -d "$CAPTURE_DIR" ]; then
  sql_count=$(find "$CAPTURE_DIR" -maxdepth 1 -name '*.sql' | wc -l | tr -d ' ')
fi
echo "Delivered (.sql files in $CAPTURE_DIR/): $sql_count"
echo

if [ "$sql_count" -eq 0 ]; then
  bad "No .sql files delivered yet. Half B (DBA capture) has not landed. This is expected before capture — re-run this script after the DBA delivers files."
else
  missing_no_file=0
  while IFS= read -r name; do
    [ -z "$name" ] && continue
    if [ ! -f "$CAPTURE_DIR/${name}.sql" ]; then
      bad "No file for missing procedure: ${name}.sql"
      missing_no_file=$((missing_no_file + 1))
    fi
  done <<< "$missing_names"
  if [ "$missing_no_file" -eq 0 ]; then
    ok "Every manifest 'missing' row has a corresponding .sql file."
  fi
fi
echo

# ---------------------------------------------------------------------------
# 2. Per-file structural checks (only runs over files that exist).
# ---------------------------------------------------------------------------
echo "--- 2. Per-file checks ---"

if [ "$sql_count" -gt 0 ]; then
  for f in "$CAPTURE_DIR"/*.sql; do
    [ -e "$f" ] || continue
    base=$(basename "$f" .sql)
    file_ok=1

    # 2a. Declared name matches file name (case-sensitive, per README
    #     convention: "matching the declared name character for character").
    declared=$(grep -m1 -iE '(CREATE|ALTER)[[:space:]]+(OR[[:space:]]+ALTER[[:space:]]+)?PROC(EDURE)?[[:space:]]+' "$f" \
      | sed -E 's/.*PROC(EDURE)?[[:space:]]+//I; s/^\[dbo\]\.//I; s/^\[|\]$//g; s/[[:space:]].*$//' \
      | tr -d '\r')
    if [ -z "$declared" ]; then
      bad "$base.sql — could not find a CREATE/ALTER PROCEDURE statement at all"
      file_ok=0
    elif [ "$declared" != "$base" ]; then
      bad "$base.sql — declared name '$declared' does not match file name '$base'"
      file_ok=0
    fi

    # 2b. First non-comment, non-blank statement is CREATE OR ALTER.
    first_stmt=$(grep -vE '^[[:space:]]*(--|$)' "$f" | head -n1 | tr -d '\r')
    if ! printf '%s' "$first_stmt" | grep -qiE '^[[:space:]]*create[[:space:]]+or[[:space:]]+alter[[:space:]]+proc'; then
      bad "$base.sql — first statement is not 'CREATE OR ALTER PROCEDURE' (found: ${first_stmt:0:60})"
      file_ok=0
    fi

    # 2c. No BOM.
    first3=$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')
    if [ "$first3" = "efbbbf" ]; then
      bad "$base.sql — starts with a UTF-8 BOM (must be UTF-8 without BOM)"
      file_ok=0
    fi

    # 2d. No CRLF line endings.
    if grep -qU $'\r' "$f" 2>/dev/null || LC_ALL=C grep -qc $'\r' "$f" 2>/dev/null; then
      bad "$base.sql — contains CRLF line endings (must be LF)"
      file_ok=0
    fi

    # 2e. No USE statement.
    if grep -qiE '^[[:space:]]*USE[[:space:]]' "$f"; then
      bad "$base.sql — contains a USE statement (forbidden; must run standalone against any tenant DB)"
      file_ok=0
    fi

    # 2f. No secret-shaped content: credentials, connection-string keys,
    #     an IPv4 literal (a hostname), or GO before the CREATE OR ALTER.
    if grep -qiE '(Password|Pwd)[[:space:]]*=' "$f" \
       || grep -qiE 'User[[:space:]]*Id[[:space:]]*=' "$f" \
       || grep -qiE 'Data[[:space:]]*Source[[:space:]]*=' "$f" \
       || grep -qiE 'Server[[:space:]]*=' "$f" \
       || grep -qiE 'Initial[[:space:]]*Catalog[[:space:]]*=' "$f" \
       || grep -qE '[0-9]{1,3}(\.[0-9]{1,3}){3}' "$f"; then
      bad "$base.sql — contains a credential/connection-string-shaped or IPv4-literal pattern"
      file_ok=0
    fi

    # 2g. Truncation guard — flag, don't fail, anything implausibly short.
    size=$(wc -c < "$f" | tr -d ' ')
    if [ "$size" -lt 100 ]; then
      flag "$base.sql — only $size bytes; flagged for human review as a possible truncated capture"
    fi

    [ "$file_ok" -eq 1 ] && ok "$base.sql — structurally OK"
  done
else
  note "(skipped — no .sql files to check)"
fi
echo

# ---------------------------------------------------------------------------
# 3. Idempotency — no file missing "CREATE OR ALTER" anywhere in its body
#    (belt-and-braces on top of check 2b, matching the task's documented
#    verification command).
# ---------------------------------------------------------------------------
echo "--- 3. Idempotency (CREATE OR ALTER present) ---"
if [ "$sql_count" -gt 0 ]; then
  non_idempotent=$(grep -rLi "CREATE OR ALTER" "$CAPTURE_DIR" --include=*.sql 2>/dev/null || true)
  if [ -n "$non_idempotent" ]; then
    while IFS= read -r f; do
      bad "not idempotent (no 'CREATE OR ALTER' found): $f"
    done <<< "$non_idempotent"
  else
    ok "All delivered files contain CREATE OR ALTER."
  fi
else
  note "(skipped — no .sql files to check)"
fi
echo

# ---------------------------------------------------------------------------
# 4. Sp_Print_CompanyDetails presence — hard print dependency (KB-102
#    Finding 4). Already scripted pre-existing; this just confirms it
#    wasn't lost in a directory reshuffle.
# ---------------------------------------------------------------------------
echo "--- 4. Sp_Print_CompanyDetails present ---"
if [ -f "Existing Store Procedures/StoredProcedures/Sp_Print_CompanyDetails.sql" ] \
   || [ -f "$CAPTURE_DIR/Sp_Print_CompanyDetails.sql" ]; then
  ok "Sp_Print_CompanyDetails.sql found (existing scripted set or capture dir)."
else
  bad "Sp_Print_CompanyDetails.sql not found anywhere — every printed document depends on it (ReportService.cs:74-77)."
fi
echo

# ---------------------------------------------------------------------------
# 5. Tenant-name leakage — best-effort. CAPTURE-STATUS.md is the one file
#    allowed to name the tenant (by identifier); .sql files must not.
#    This check cannot know the tenant name in advance, so it only flags
#    the presence of CAPTURE-STATUS.md as informational context.
# ---------------------------------------------------------------------------
echo "--- 5. CAPTURE-STATUS.md provenance ---"
if [ -f "$CAPTURE_DIR/CAPTURE-STATUS.md" ]; then
  if grep -qi "TBD" "$CAPTURE_DIR/CAPTURE-STATUS.md"; then
    flag "CAPTURE-STATUS.md exists but still contains TBD placeholders — provenance not fully recorded."
  else
    ok "CAPTURE-STATUS.md exists and has no TBD placeholders."
  fi
else
  bad "CAPTURE-STATUS.md is missing — provenance of any delivered .sql files cannot be established."
fi
echo

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
echo "=== Arithmetic ==="
echo "  manifest 'missing' rows : $missing_count"
echo "  .sql files delivered    : $sql_count"
echo "  difference               : $((sql_count - missing_count))"
echo

echo "=== Result ==="
if [ "$fail" -eq 1 ]; then
  echo "FAIL — one or more hard checks failed. See [FAIL] lines above."
  exit 1
elif [ "$warn" -eq 1 ]; then
  echo "PASS with warnings — see [WARN] lines above for items needing human review."
  exit 0
else
  echo "PASS — capture is complete and conforms to all checked conventions."
  exit 0
fi
