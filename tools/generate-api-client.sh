#!/usr/bin/env bash
#
# tools/generate-api-client.sh -- THE regeneration command.
#
# Task M2-B10; documented in docs/kb/api/generated-client.md (KB-112).
#
# Run this after changing anything a caller can see: a route, a status code, a ViewModel
# property, an operation id. It rewrites BOTH committed artefacts:
#
#     api/openapi.json                                    (the contract)
#     frontend/nexgen-web/src/app/core/api/generated/**    (the Angular client)
#
# Commit whatever it changes. CI runs this exact script and fails when either artefact
# differs from what is committed -- one command for developers and for CI, because two
# commands drift.
#
#   bash tools/generate-api-client.sh
#   bash tools/generate-api-client.sh --check     # what CI does: regenerate, then fail on drift
#
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

check_mode=0
[ "${1:-}" = "--check" ] && check_mode=1

spec="api/openapi.json"
client_dir="frontend/nexgen-web/src/app/core/api/generated"

# ---------------------------------------------------------------- 1. the contract
bash tools/generate-openapi.sh

# ---------------------------------------------------------------- 2. the client
# ng-openapi-gen is a devDependency of frontend/nexgen-web and is configured by
# frontend/nexgen-web/openapi-gen.json. `removeStaleFiles` there is what makes a deleted
# endpoint delete its generated file rather than leaving a stale one behind.
echo "==> ng-openapi-gen (frontend/nexgen-web/openapi-gen.json)"
(cd frontend/nexgen-web && npx --no-install ng-openapi-gen --config openapi-gen.json)

# ---------------------------------------------------------------- 3. the banner
# ng-openapi-gen writes its own one-line marker; this adds the banner the task requires,
# naming the command that regenerates the file. Idempotent because step 2 rewrites every
# file from scratch on every run.
echo "==> stamping the generated-file banner"
while IFS= read -r f; do
  tmp="$(mktemp)"
  {
    printf '%s\n' "// AUTO-GENERATED FROM api/openapi.json - DO NOT EDIT BY HAND."
    printf '%s\n' "// Regenerate with: bash tools/generate-api-client.sh"
    cat "$f"
  } > "$tmp"
  mv "$tmp" "$f"
done < <(find "$client_dir" -name '*.ts' -type f | LC_ALL=C sort)

# ---------------------------------------------------------------- 4. drift check
if [ "$check_mode" -eq 1 ]; then
  if ! git diff --quiet -- "$spec" "$client_dir" || \
     [ -n "$(git ls-files --others --exclude-standard -- "$spec" "$client_dir")" ]; then
    echo ""
    echo "=============================================================================="
    echo "CONTRACT DRIFT: the committed API contract or generated client is out of date."
    echo ""
    echo "The API's OpenAPI document no longer matches api/openapi.json, or the generated"
    echo "Angular client no longer matches $client_dir."
    echo ""
    echo "FIX IT LIKE THIS, from the repository root:"
    echo ""
    echo "    bash tools/generate-api-client.sh"
    echo "    git add api/openapi.json $client_dir"
    echo "    git commit"
    echo ""
    echo "Review the diff before committing it: it IS the contract change your API edit"
    echo "made, and every SPA call site sees it."
    echo "=============================================================================="
    echo ""
    git --no-pager diff --stat -- "$spec" "$client_dir"
    git --no-pager diff -- "$spec" "$client_dir" | head -200
    git ls-files --others --exclude-standard -- "$spec" "$client_dir"
    exit 1
  fi
  echo "==> no drift: the committed contract and client match the API."
fi
