#!/usr/bin/env bash
# db/tools/compare-tenant-fingerprints.sh
#
# Compares per-tenant stored-procedure fingerprints and classifies every
# procedure name as identical / cosmetic / divergent / missing_in_tenant /
# extra_in_tenant. Answers the mechanical half of Q-14 ("do the stored
# procedures differ between tenant databases?", docs/kb/open-questions.md).
#
# Requires NO database access. It reads only CSV files that a DBA produced
# with Query B ("FINGERPRINT_QUERY_VERSION 2") of
# db/tools/list-deployed-procedures.sql -- see
# db/RUNBOOK-tenant-drift-check.md.
#
# Usage, from the repository root:
#   bash db/tools/compare-tenant-fingerprints.sh            # reads db/drift/
#   bash db/tools/compare-tenant-fingerprints.sh <dir>      # reads <dir>/*.csv
#
# The optional directory argument exists so the harness itself can be tested
# against fixtures outside db/drift/ (see "Self-test" in the runbook). It
# changes nothing else.
#
# Exit codes:
#   0  ran to completion, every name classified identical or cosmetic
#   2  STRUCTURAL FAILURE -- mismatched headers, malformed row, NULL hash,
#      duplicate row, or arithmetic that does not close. The comparison is
#      NOT trustworthy; fix the collection and re-run.
#   3  not enough input to answer the question (fewer than two tenant CSVs).
#      This is "undecided", NEVER "no drift".
#   4  ran to completion and found drift needing escalation (divergent,
#      missing_in_tenant, or extra_in_tenant).
#
# What this script CANNOT do, stated so nobody over-reads its output:
#   * A hash_normalised match is INFERRED equivalence, not proof --
#     whitespace collapsing and case folding cannot distinguish a semantic
#     difference inside a string literal from formatting.
#   * It compares fingerprints, not procedure bodies. It can tell you THAT
#     two tenants differ, never HOW.
#   * With one tenant's CSV it says nothing at all about drift.
#
# Tooling: bash, awk, sed, sort, grep only -- runs unmodified in Git Bash on
# Windows and in a Linux CI container, matching db/tools/verify-capture.sh.

set -uo pipefail

DRIFT_DIR="${1:-db/drift}"
MANIFEST="db/stored-procedures/manifest.csv"
CANONICAL_HEADER="schema_name,procedure_name,create_date,modify_date,definition_length,hash_raw,hash_normalised"

if [ ! -f "$MANIFEST" ]; then
    echo "ERROR: $MANIFEST not found." >&2
    echo "Run this script from the repository root (the directory containing db/)." >&2
    exit 2
fi

if [ ! -d "$DRIFT_DIR" ]; then
    echo "ERROR: fingerprint directory '$DRIFT_DIR' does not exist." >&2
    exit 3
fi

echo "=================================================================="
echo " compare-tenant-fingerprints.sh -- $DRIFT_DIR/*.csv vs $MANIFEST"
echo "=================================================================="
echo

# ---------------------------------------------------------------------
# 0. Collect the fingerprint files (deterministic order).
# ---------------------------------------------------------------------
CSVS=()
while IFS= read -r f; do
    [ -n "$f" ] && CSVS+=("$f")
done < <(ls -1 "$DRIFT_DIR"/*.csv 2>/dev/null | sort)

if [ "${#CSVS[@]}" -eq 0 ]; then
    echo "-- 1. Input --"
    echo "  tenant fingerprint CSVs found: 0"
    echo
    echo "NOT ENOUGH INPUT: no fingerprints have been delivered into $DRIFT_DIR."
    echo "Q-14 is UNDECIDED, not answered. Absence of fingerprints is not"
    echo "evidence of absence of drift."
    echo "Needed: a DBA with VIEW DEFINITION on at least two tenant databases,"
    echo "and a tenant list. See db/RUNBOOK-tenant-drift-check.md."
    exit 3
fi

# ---------------------------------------------------------------------
# 1. Header consistency -- fail loudly before comparing anything.
#    A different header means a different query version, and a comparison
#    across query versions compares the queries, not the procedures.
# ---------------------------------------------------------------------
echo "-- 1. Input and header check --"
echo "  tenant fingerprint CSVs found: ${#CSVS[@]}"

header_failures=0
first_header=""
for f in "${CSVS[@]}"; do
    # strip UTF-8 BOM, strip CR, lowercase, remove spaces around separators
    h=$(head -n 1 "$f" | sed -e '1s/^\xEF\xBB\xBF//' -e 's/\r$//' -e 's/[[:space:]]*,[[:space:]]*/,/g' -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' | tr 'A-Z' 'a-z')
    if [ -z "$first_header" ]; then first_header="$h"; fi
    if [ "$h" != "$CANONICAL_HEADER" ]; then
        echo "  FAIL: $f header does not match FINGERPRINT_QUERY_VERSION 2."
        echo "        expected: $CANONICAL_HEADER"
        echo "        found   : $h"
        header_failures=$((header_failures + 1))
    elif [ "$h" != "$first_header" ]; then
        echo "  FAIL: $f header differs from $(basename "${CSVS[0]}")."
        header_failures=$((header_failures + 1))
    else
        echo "  ok:   $(basename "$f") header matches"
    fi
done

if [ "$header_failures" -gt 0 ]; then
    echo
    echo "=================================================================="
    echo " ABORTED -- $header_failures file(s) have a non-conforming header."
    echo " Mixing fingerprint query versions silently invalidates the whole"
    echo " comparison. Re-collect the offending tenants with Query B of"
    echo " db/tools/list-deployed-procedures.sql and re-run."
    echo "=================================================================="
    exit 2
fi
echo

if [ "${#CSVS[@]}" -lt 2 ]; then
    echo "NOT ENOUGH INPUT: only ${#CSVS[@]} tenant fingerprint present."
    echo "Two is the minimum for the question to be answerable at all, and the"
    echo "source tenant named in db/stored-procedures/CAPTURE-STATUS.md must be"
    echo "one of them. With one tenant every name would trivially classify"
    echo "'identical', which would be a statement about the arithmetic, not"
    echo "about the tenants. Q-14 stays UNDECIDED."
    exit 3
fi

# ---------------------------------------------------------------------
# 2..6. Parse, classify, report.
# ---------------------------------------------------------------------
awk -v manifest="$MANIFEST" '
function tenant_of(path,   n, parts) {
    n = split(path, parts, "/")
    f = parts[n]
    sub(/\.csv$/, "", f)
    return f
}
# Portable index sort -- gawk-only asorti() is deliberately avoided so this
# runs under mawk in a CI container too. Insertion sort; ~100 keys.
function keysort(arr, out,   k, n, i, j, tmp) {
    n = 0
    for (k in arr) out[++n] = k
    for (i = 2; i <= n; i++) {
        tmp = out[i]; j = i - 1
        while (j > 0 && out[j] > tmp) { out[j + 1] = out[j]; j-- }
        out[j + 1] = tmp
    }
    return n
}
BEGIN {
    FS = ","
    structural = 0
    # manifest names -> the worklist scope (M0-01-01 output)
    while ((getline line < manifest) > 0) {
        if (mfirst++ == 0) continue          # header row
        split(line, mf, ",")
        gsub(/\r/, "", mf[1])
        if (mf[1] != "") { in_manifest[mf[1]] = 1; manifest_count++ }
    }
    close(manifest)
}
FNR == 1 { t = tenant_of(FILENAME); torder[++tn] = t; next }
{
    gsub(/\r/, "")
    if ($0 ~ /^[[:space:]]*$/) next
    if (NF != 7) {
        printf("  FAIL: %s line %d has %d fields, expected 7 -- malformed CSV (an unquoted comma in a value?).\n", FILENAME, FNR, NF) > "/dev/stderr"
        structural++
        next
    }
    schema = $1; name = $2; cdate = $3; mdate = $4; dlen = $5; hraw = $6; hnorm = $7
    if (name == "") next
    if (hraw == "" || hnorm == "" || toupper(hraw) == "NULL" || toupper(hnorm) == "NULL") {
        printf("  FAIL: %s: %s has an empty/NULL hash -- the collector lacked VIEW DEFINITION, the object is WITH ENCRYPTION, or HASHBYTES truncated on a pre-2016 instance. Re-collect; do not read this as \"no drift\".\n", FILENAME, name) > "/dev/stderr"
        structural++
        next
    }
    if ((t SUBSEP name) in seen) {
        printf("  FAIL: %s: duplicate row for %s.\n", FILENAME, name) > "/dev/stderr"
        structural++
        next
    }
    seen[t, name] = 1
    names[name] = 1
    present[t, name] = 1
    raw[t, name] = hraw
    norm[t, name] = hnorm
    sch[t, name] = schema
    mod[t, name] = mdate
    len[t, name] = dlen
    count[t]++
    if (schema != "dbo") {
        if (!(name in nondbo)) n_nondbo++
        nondbo[name] = nondbo[name] (nondbo[name] == "" ? "" : " ") t ":" schema
    }
}
END {
    if (structural > 0) {
        printf("\nABORTED -- %d structural failure(s) in the fingerprint CSVs (see FAIL lines above).\n", structural)
        exit 2
    }

    print "-- 2. Tenants compared (label only -- never a connection string) --"
    for (i = 1; i <= tn; i++) printf("  %-40s %d procedures\n", torder[i], count[torder[i]])
    printf("  manifest names in scope (M0-01-01 worklist): %d\n\n", manifest_count)

    n_identical = n_cosmetic = n_divergent = n_missing = n_extra = 0
    distinct = 0

    for (name in names) {
        distinct++
        missing_from = ""; present_in = ""
        for (i = 1; i <= tn; i++) {
            t = torder[i]
            if ((t SUBSEP name) in present) present_in = present_in (present_in == "" ? "" : " ") t
            else missing_from = missing_from (missing_from == "" ? "" : " ") t
        }
        if (!(name in in_manifest)) {
            n_extra++
            extra_lines[name] = sprintf("  %-55s present in: %s", name, present_in)
            continue
        }
        if (missing_from != "") {
            n_missing++
            missing_lines[name] = sprintf("  %-55s present in: %s | ABSENT from: %s", name, present_in, missing_from)
            continue
        }
        # present everywhere -- compare hashes
        raw_same = 1; norm_same = 1
        base_t = torder[1]
        for (i = 2; i <= tn; i++) {
            t = torder[i]
            if (raw[t, name]  != raw[base_t, name])  raw_same = 0
            if (norm[t, name] != norm[base_t, name]) norm_same = 0
        }
        if (raw_same) {
            n_identical++
            classified[name] = "identical"
            # a differing modify_date with identical bodies is still worth seeing
            md_same = 1
            for (i = 2; i <= tn; i++) if (mod[torder[i], name] != mod[base_t, name]) md_same = 0
            if (!md_same) {
                d = ""
                for (i = 1; i <= tn; i++) d = d (d == "" ? "" : " | ") torder[i] "=" mod[torder[i], name]
                moddate_lines[name] = sprintf("  %-55s %s", name, d)
                n_moddate++
            }
        } else if (norm_same) {
            n_cosmetic++
            classified[name] = "cosmetic"
            d = ""
            for (i = 1; i <= tn; i++) d = d (d == "" ? "" : " | ") torder[i] "=" substr(raw[torder[i], name], 1, 12)
            cosmetic_lines[name] = sprintf("  %-55s hash_raw: %s", name, d)
        } else {
            n_divergent++
            classified[name] = "divergent"
            d = ""
            for (i = 1; i <= tn; i++) d = d (d == "" ? "" : " | ") torder[i] "=" substr(norm[torder[i], name], 1, 12) "/" len[torder[i], name] "B"
            divergent_lines[name] = sprintf("  %-55s hash_normalised/len: %s", name, d)
        }
    }

    print "-- 3. Classification counts --"
    printf("  identical         : %d   (hash_raw equal across every tenant)\n", n_identical)
    printf("  cosmetic          : %d   (hash_raw differs, hash_normalised equal -- INFERRED equivalence)\n", n_cosmetic)
    printf("  divergent         : %d   (hash_normalised differs -- ESCALATE)\n", n_divergent)
    printf("  missing_in_tenant : %d   (present in some tenants, absent in others -- ESCALATE)\n", n_missing)
    printf("  extra_in_tenant   : %d   (observed but not in manifest.csv -- record, DO NOT DELETE)\n\n", n_extra)

    print "-- 4. Arithmetic closure --"
    total = n_identical + n_cosmetic + n_divergent + n_missing + n_extra
    printf("  %d + %d + %d + %d + %d = %d ; distinct procedure names observed across all tenants = %d\n",
           n_identical, n_cosmetic, n_divergent, n_missing, n_extra, total, distinct)
    if (total != distinct) {
        printf("  FAIL: class counts do not sum to the distinct names observed. The comparison is not trustworthy.\n")
        arithmetic_broken = 1
    } else {
        printf("  ok:   class counts sum to the distinct names observed.\n")
    }
    # manifest coverage
    absent_all = 0
    for (name in in_manifest) if (!(name in names)) { absent_all++; absent_lines[name] = sprintf("  %-55s not present in ANY fingerprinted tenant", name) }
    in_scope_observed = n_identical + n_cosmetic + n_divergent + n_missing
    printf("  manifest coverage: %d classified + %d absent from every tenant = %d ; manifest rows = %d\n",
           in_scope_observed, absent_all, in_scope_observed + absent_all, manifest_count)
    if (in_scope_observed + absent_all != manifest_count) {
        printf("  FAIL: manifest coverage does not close.\n")
        arithmetic_broken = 1
    } else {
        printf("  ok:   every manifest name is either classified or listed as absent from every tenant.\n")
    }
    print ""

    print "-- 5. divergent (hash_normalised differs -- POTENTIALLY BEHAVIOURAL, escalate) --"
    if (n_divergent == 0) print "  (none)"
    else { n = keysort(divergent_lines, ix); for (i = 1; i <= n; i++) print divergent_lines[ix[i]] }
    print ""

    print "-- 6. missing_in_tenant (a report that works for one customer and throws for another) --"
    if (n_missing == 0) print "  (none)"
    else { n = keysort(missing_lines, ix2); for (i = 1; i <= n; i++) print missing_lines[ix2[i]] }
    print ""

    print "-- 7. extra_in_tenant (present in a tenant, absent from manifest.csv -- likely a customisation or a dead object; DO NOT DELETE) --"
    if (n_extra == 0) print "  (none)"
    else { n = keysort(extra_lines, ix3); for (i = 1; i <= n; i++) print extra_lines[ix3[i]] }
    print ""

    print "-- 8. cosmetic (hash_raw differs, hash_normalised equal -- Inferred equivalent, not proven) --"
    if (n_cosmetic == 0) print "  (none)"
    else { n = keysort(cosmetic_lines, ix4); for (i = 1; i <= n; i++) print cosmetic_lines[ix4[i]] }
    print ""

    print "-- 9. manifest names absent from every fingerprinted tenant --"
    if (absent_all == 0) print "  (none)"
    else { n = keysort(absent_lines, ix5); for (i = 1; i <= n; i++) print absent_lines[ix5[i]] }
    print ""

    print "-- 10. secondary signals --"
    if (n_moddate > 0) {
        print "  differing modify_date with identical hash_raw (deployment history differs, body does not):"
        n = keysort(moddate_lines, ix6); for (i = 1; i <= n; i++) print moddate_lines[ix6[i]]
    } else {
        print "  modify_date: no differences among identical procedures."
    }
    if (n_nondbo > 0) {
        print "  procedures observed under a schema other than dbo (the application always calls dbo.<name> -- ReportExecutor.cs:27; this is a finding in itself):"
        n = keysort(nondbo, ix7); for (i = 1; i <= n; i++) printf("  %-55s %s\n", ix7[i], nondbo[ix7[i]])
    } else {
        print "  schema: every observed procedure is in dbo."
    }
    print ""

    print "=================================================================="
    print " RESULT"
    print "=================================================================="
    if (arithmetic_broken) {
        print "  STRUCTURAL FAILURE -- arithmetic did not close. Do not use this output."
        exit 2
    }
    if (n_divergent + n_missing + n_extra > 0) {
        printf("  DRIFT REQUIRING ESCALATION: %d divergent, %d missing_in_tenant, %d extra_in_tenant.\n", n_divergent, n_missing, n_extra)
        print "  Do NOT reconcile anything. Present Options A/B/C to the product owner"
        print "  (see docs/kb/execution/tasks/M0-02.md) and record the finding in"
        print "  docs/kb/architecture/stored-procedure-drift.md."
        exit 4
    }
    if (n_cosmetic > 0) {
        printf("  COSMETIC DRIFT ONLY across %d tenants: %d identical, %d cosmetic.\n", tn, n_identical, n_cosmetic)
        print "  The single artefact set in db/stored-procedures/ stands. Equivalence of the"
        print "  cosmetic set is INFERRED (normalisation), not Confirmed."
        exit 0
    }
    printf("  NO DRIFT across %d tenants: all %d procedure names identical by hash_raw.\n", tn, n_identical)
    exit 0
}
' "${CSVS[@]}"
awk_status=$?
exit $awk_status
