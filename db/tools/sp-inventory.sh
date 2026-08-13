#!/usr/bin/env bash
# db/tools/sp-inventory.sh
#
# Re-runnable reference-index generator for M0-01-01 (see
# docs/kb/execution/tasks/M0-01-01.md and
# docs/kb/architecture/stored-procedure-inventory.md).
#
# Emits one line per (procedure_name, path:line) match found in .cs/.razor
# source under V.SMART/, tab-separated:
#
#   procedure_name<TAB>path:line<TAB>commented(yes|no)
#
# Takes no arguments. Must be run from the repository root. Uses only
# grep/sort/sed-class tooling available in Git Bash on Windows and in a
# Linux CI container. Contains no connection string, host name or
# credential.
#
# "commented" is a heuristic, not a parser: a match is marked "yes" if the
# text preceding it on the same line contains a "//" (C#/Razor line
# comment) or a "@*" not yet closed by "*@" (Razor block comment opener),
# OR the line, once leading whitespace is stripped, begins with "*" (a
# continuation line inside a /* */ block comment). It does NOT track
# multi-line /* ... */ or @* ... *@ spans that open on an earlier line and
# close after the match's line — grep operates one line at a time. Verify
# any "commented(no)" classification you rely on by opening the file.

set -euo pipefail

if [ ! -d "V.SMART" ]; then
  echo "error: run this script from the repository root (V.SMART/ not found)" >&2
  exit 1
fi

# 1. Find every (file, line-number, line-text) triple containing an Sp_*
#    token, restricted to application code the same way M0-01-01's count
#    was derived: .cs and .razor under V.SMART/, excluding build output.
grep -rnoE '^.*Sp_[A-Za-z0-9_]+.*$' \
  --include=*.cs --include=*.razor \
  --exclude-dir=obj --exclude-dir=bin \
  V.SMART/ 2>/dev/null |
while IFS=: read -r file line rest; do
  # rest may itself contain ':' (e.g. "EXEC dbo.Sp_Foo:"), so re-join
  # anything grep split past the second colon back into the line text.
  text="$rest"

  # 2. Pull out every distinct Sp_* token on this line — a line can
  #    reference more than one procedure name.
  printf '%s\n' "$text" | grep -oE 'Sp_[A-Za-z0-9_]+' | sort -u |
  while read -r proc; do
    # 3. Classify commented(yes|no) using the heuristic in the header.
    before="${text%%"$proc"*}"
    trimmed="$(printf '%s' "$text" | sed -E 's/^[[:space:]]*//')"

    commented=no
    if printf '%s' "$before" | grep -q '//'; then
      commented=yes
    elif printf '%s' "$before" | grep -q '@\*'; then
      commented=yes
    elif printf '%s' "$trimmed" | grep -qE '^\*([^/]|$)'; then
      commented=yes
    fi

    printf '%s\t%s:%s\t%s\n' "$proc" "$file" "$line" "$commented"
  done
done | sort -u
