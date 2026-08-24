#!/usr/bin/env bash
#
# tools/generate-openapi.sh -- regenerate api/openapi.json from the compiled API.
#
# Task M2-B10. Documented in docs/kb/api/generated-client.md (KB-112).
#
# WHY THIS ROUTE. Swagger UI is gated to Development in V.SMART.Api/Program.cs, and it stays
# that way, so the contract must come out of the BUILD rather than out of a running server.
# `dotnet swagger tofile` (Swashbuckle.AspNetCore.Cli, pinned to the same 7.3.1 the API
# references, in .config/dotnet-tools.json) loads the compiled assembly, builds the host in
# memory and asks the same ISwaggerProvider the UI would. Verified working on 2026-08-24; the
# emitted document is byte-identical across repeated runs.
#
# THE FOUR ENVIRONMENT VARIABLES BELOW ARE NOT SECRETS AND ARE NOT CREDENTIALS.
# Building the host runs StartupConfigurationValidator (Program.cs), which refuses to start
# when ConnectionStrings:MasterDb, Jwt:Secret, Jwt:Issuer or Jwt:Audience is empty (M0-03-03).
# Nothing in startup opens a database connection or verifies a token -- confirmed by this
# script working with the obviously-fake values below -- so these placeholders exist only to
# get past the validator. Do NOT replace them with real values: this script must behave
# identically on a laptop and on a CI runner, and a real connection string here would be a
# secret in a public repository.
#
# Usage:
#   tools/generate-openapi.sh              # build, then write api/openapi.json
#   tools/generate-openapi.sh --no-build   # reuse the existing build output
#   tools/generate-openapi.sh --output PATH
#
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

output="api/openapi.json"
do_build=1
while [ $# -gt 0 ]; do
  case "$1" in
    --no-build) do_build=0; shift ;;
    --output) output="$2"; shift 2 ;;
    *) echo "generate-openapi.sh: unknown argument '$1'" >&2; exit 2 ;;
  esac
done

export ConnectionStrings__MasterDb="${ConnectionStrings__MasterDb:-Server=(local);Database=openapi-spec-generation-only;Trusted_Connection=True;TrustServerCertificate=True}"
export Jwt__Secret="${Jwt__Secret:-openapi-spec-generation-only-not-a-real-secret-0123456789}"
export Jwt__Issuer="${Jwt__Issuer:-openapi-spec-generation-only}"
export Jwt__Audience="${Jwt__Audience:-openapi-spec-generation-only}"

if [ "$do_build" -eq 1 ]; then
  echo "==> dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj"
  dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj
fi

echo "==> dotnet tool restore"
dotnet tool restore

assembly="V.SMART/V.SMART.Api/bin/Debug/net9.0/V.SMART.Api.dll"
if [ ! -f "$assembly" ]; then
  echo "generate-openapi.sh: $assembly not found -- run without --no-build." >&2
  exit 1
fi

tmp="$(mktemp)"
echo "==> dotnet swagger tofile --output $output $assembly v1"
dotnet swagger tofile --output "$tmp" "$assembly" v1 >/dev/null

# Swashbuckle writes LF and no trailing newline. The trailing newline is added here, once, so
# that the committed file is a well-formed text file and so that the drift check compares two
# artefacts produced by this same script.
mkdir -p "$(dirname "$output")"
printf '%s\n' "$(cat "$tmp")" > "$output"
rm -f "$tmp"

echo "==> wrote $output"
