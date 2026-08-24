---
doc_id: KB-112
title: OpenAPI Contract and Generated TypeScript Client
module: api
source_files:
  - api/openapi.json
  - tools/generate-openapi.sh
  - tools/generate-api-client.sh
  - frontend/nexgen-web/openapi-gen.json
  - frontend/nexgen-web/src/app/core/api/generated/
  - .github/workflows/ci.yml
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Api/V.SMART.Api.csproj
status: active
confidence: confirmed
last_verified: 2026-08-24
dependencies: [ADR-002, ADR-007, KB-041, KB-080, KB-083, KB-087, KB-114]
---

# KB-112 — OpenAPI contract and the generated TypeScript client

Created by [M2-B10](../execution/tasks/M2-B10.md), 2026-08-24. Everything below was **run**, on
this Windows workstation, on that date; nothing here is a plan.

[ADR-002 §6](../decisions/ADR-002-rest-api-layer.md): *"OpenAPI is the contract; the TypeScript
client is generated in CI, never hand-written. A contract change that breaks the client fails
the build."*

---

## 1. The one command

```bash
bash tools/generate-api-client.sh
```

Run it after **any** change a caller can see — a route, a status code, a ViewModel property, an
operation id, a new controller — then commit what it changed. It rewrites both committed
artefacts:

| Artefact | What it is |
|---|---|
| `api/openapi.json` | the contract. 18 operations across 6 controllers as of 2026-08-24 |
| `frontend/nexgen-web/src/app/core/api/generated/**` | the Angular client: 5 injectable services, 17 models |

CI runs the identical script with `--check` and **fails on any difference**, printing the
command above. Developers and CI run one command, because two commands drift.

`tools/generate-api-client.sh` calls `tools/generate-openapi.sh` (the contract), then
`ng-openapi-gen`, then stamps the generated-file banner. The spec step can be run alone:

```bash
bash tools/generate-openapi.sh              # build, then write api/openapi.json
bash tools/generate-openapi.sh --no-build   # reuse existing build output
```

---

## 2. How the contract comes out of the build — and the four variables you must not mistake for secrets

**Route taken: `dotnet swagger tofile` (Swashbuckle.AspNetCore.Cli 7.3.1), pinned in
`.config/dotnet-tools.json`.** Verified working 2026-08-24. The fallback the task allowed —
start the API in `Development` inside CI and fetch `/swagger/v1/swagger.json` — was **not**
needed and is **not** used: Swagger UI stays gated to `Development` (`Program.cs`), and the
contract must not depend on a running server.

**The trap, recorded so nobody re-derives it.** `dotnet swagger tofile` builds the host, which
runs `StartupConfigurationValidator` (M0-03-03). The first attempt failed with a misleading
`"A type named 'StartupDevelopment' or 'Startup' could not be found"` — that is Swashbuckle's
*fallback* error after the host factory threw, **not** a missing `Startup` class. The real
cause was the validator refusing to boot on empty configuration. It needs **four** variables,
not two:

```
ConnectionStrings__MasterDb   Jwt__Secret (>= 32 UTF-8 bytes)   Jwt__Issuer   Jwt__Audience
```

`tools/generate-openapi.sh` supplies obviously-fake defaults for all four. **They are not
credentials and must never be replaced by real ones** — this repository is public, and the
script has to behave identically on a laptop and on a runner.

**Confirmed negative result, and it is the useful half:** nothing between the validator and
`app.Run()` opens a database connection or verifies a token. The generation succeeds with a
connection string pointing at a database that does not exist. This is why the job needs no
secret and no service container.

**Determinism, measured:** two consecutive runs produced byte-identical output (`md5sum`
equal), as did a run after a full rebuild. Swashbuckle emits LF and no trailing newline; the
script appends exactly one newline so the committed file is well-formed text.

**`.gitattributes` is load-bearing here.** `* text=auto` plus `core.autocrlf=true` (this dev
box *and* the `windows-latest` runner) would hand a CRLF working copy to every checkout, and
the drift check would then compare LF generator output against a CRLF tree and fail on every
run. `api/*.json`, the generated client and `tools/*.sh` are pinned to `eol=lf`.

---

## 3. The metadata the document is generated from

| Concern | Mechanism | Where |
|---|---|---|
| Operation id | route `Name` on the HTTP attribute, read by `CustomOperationIds` | e.g. `[HttpGet(Name = "getCurrencies")]` |
| Tag | `[Tags("…")]` on the controller | `CurrencyExcelController` deliberately shares `Currency` |
| Responses | `[ProducesResponseType]` per status, per [KB-114 §11](controller-conventions.md#11-openapi-annotations--m2-b10-depends-on-this-section) | all 18 actions |
| Descriptions | XML `<summary>`, via `<GenerateDocumentationFile>` + `IncludeXmlComments` | `V.SMART.Api.csproj`, `Program.cs` |
| Schema ids | **Swashbuckle's default (short type name), deliberately unchanged** | see below |

**Operation ids are API surface.** Renaming one renames a method in every SPA call site. It is a
breaking change, and it is reviewable precisely because it appears in the `api/openapi.json`
diff.

**Schema ids: no custom strategy, and that is a decision.** The default already emits
`CurrencyVM`, `ProblemDetails`, `CurrencyVMPagedResult` — readable, no namespace mangling,
despite the dot in `V.SMART`. A custom `CustomSchemaIds` would have renamed types for no gain.
The residual risk is recorded rather than pre-solved: the default is the **short** name, so two
same-named types on the surface would collide. One such pair already exists but is not on the
surface — `V.SMART.Api.Contracts.PagedResult<T>` and the unreferenced
`V.SMART.Shared.ViewModels.PagedResult<T>` (`RejectionMasterVM.cs:33`). Swashbuckle throws at
generation time on a collision, so the failure would be loud and would land in the CI job, not
in production.

**`CS1591` and `CS1573` are suppressed in `V.SMART.Api.csproj`,** explicitly, because enabling
the documentation file surfaces them. Measured with `dotnet build -t:Rebuild`: 6,699 warnings
without the suppressions, **6,694 with them**, against the repository baseline of 6,695. The
baseline does not creep.

---

## 4. The generator: ng-openapi-gen 1.0.5 — and why not the other three

Scored against **this** committed `api/openapi.json`, not a toy spec. Each candidate was
installed and actually run.

| | **ng-openapi-gen 1.0.5** *(chosen)* | @hey-api/openapi-ts 0.99.0 | openapi-typescript 7.13.0 | @openapitools/openapi-generator-cli |
|---|---|---|---|---|
| Angular fit | **HttpClient + Observable + `@Injectable`** — flows through the app's interceptors | fetch/axios only; **no `@hey-api/client-angular` exists** (npm 404, checked) | none: types only, no runtime | `typescript-angular` template exists |
| Ergonomics | `currencies.getCurrencies({ pageNumber: 1, pageSize: 20, sort: '-createdDate' })` → `Observable<CurrencyVMPagedResult>` | best of the four: typed per-status result unions | `client.GET('/api/v1/currencies', …)` — path strings, no methods | verbose; one class per tag |
| `problem+json` | `ProblemDetails` is a named interface; the body arrives as `HttpErrorResponse.error`, **typed `any`** — narrow it in the wrapper layer | per-status error union: `409: ProblemDetails` typed end to end | named type, but you wire the call yourself | named type, error typed `any` |
| Nullability | `\| null` preserved from the document | preserved | preserved, most literal of the four | preserved |
| Determinism | byte-identical on repeat runs; `endOfLineStyle: lf` pinned | deterministic | deterministic | deterministic, but output depends on the JVM being present |
| Toolchain | npm devDependency | npm devDependency | npm devDependency | **requires a JRE — `java` is not installed on this workstation (`command not found`)** |
| Maintenance | 1.0.5, published 2025-11-24; Angular-specific, narrow blast radius | very active (2026-08-19) but **pre-1.0**: breaking changes are routine | extremely widely used, stable | large, slow-moving, Java-coupled |

**Why ng-openapi-gen won.** The SPA's authentication is an Angular `HttpInterceptor` (ported by
M2-C11 from the pilot). A fetch-based client bypasses the interceptor chain entirely, so
`@hey-api` and `openapi-typescript`+`openapi-fetch` would each require a second, parallel auth
and error-handling stack — an architecture split for the sake of nicer error types.
`openapi-generator-cli` was rejected on toolchain: it would put a JRE on every developer machine
and in the CI job.

**What the choice costs, stated plainly (do not rediscover these):**

1. **`$Plain` twins.** Every JSON operation is emitted twice: `getCurrencies()` and
   `getCurrencies$Plain()`. The cause is in the document, and the document is truthful — MVC's
   default output formatters accept `text/plain`, `application/json` and `text/json`, so
   Swashbuckle lists all three. Removing them would mean changing serialisation configuration,
   which M2-B10 was explicitly forbidden to touch. **Use the unsuffixed method; ignore the
   twin.**
2. **Errors are `any` at the boundary.** `HttpErrorResponse.error` must be cast to the generated
   `ProblemDetails` (or `ValidationProblemDetails`). That cast belongs in the hand-written
   wrapper layer in `src/app/core/api/`, written **once**, not at each call site.
3. `camelizeModelNames: false` is set on purpose so `CurrencyVM` stays `CurrencyVM` and matches
   the C# type a backend developer is reading. With the default it becomes `CurrencyVm`.

### The 409 message survives — verified

[ADR-002 §4](../decisions/ADR-002-rest-api-layer.md) requires the service's message to reach the
user verbatim in `title`. `models/problem-details.ts` declares `title?: string | null`, and this
compiled under `strict` in a throwaway probe:

```ts
const problem = err.error as ProblemDetails;
const message: string = problem.title ?? 'Unexpected error';
```

Nothing in the generated client wraps, remaps or discards a non-2xx body: the service methods
return the raw `Observable`, so the body reaches `HttpErrorResponse.error` untouched.

### `PagedResult<T>` emits once

`grep -rl PagedResult` over the generated tree returns one model:
`models/currency-vm-paged-result.ts` → `CurrencyVMPagedResult`. There is no per-controller paged
type. OpenAPI has no generics, so each *item type* instantiates the wrapper once — that is one
type per resource, not one per controller, and it is the floor for any generator.

---

## 5. Decimals — read this before writing any money code

**Measured, not assumed.** There is no nullable decimal on the API surface today, so M2-B10
added a temporary `decimal?`/`decimal` pair to a contract record, regenerated, read the output
and reverted. Verbatim:

| C# | OpenAPI | TypeScript emitted |
|---|---|---|
| `decimal? ProbeNullableMoney` | `{"type":"number","format":"double","nullable":true}` | `probeNullableMoney?: number \| null;` |
| `decimal ProbeMoney` | `{"type":"number","format":"double"}` | `probeMoney?: number;` |

Two consequences, neither of them resolved here:

1. **`decimal` becomes an IEEE-754 double on the wire and a `number` in TypeScript.** This is
   ASP.NET Core's own JSON serialisation, not the generator's doing, and the document merely
   describes it truthfully. **This is flagged to [M2-C10](../execution/tasks/M2-C10.md) (*no
   float money arithmetic*), not silently resolved.** `decimal.js` is already a dependency of
   `frontend/nexgen-web`; M2-C10 must decide where the `number → Decimal` boundary sits, and
   whether the API should ever emit money as a string. The only decimals reachable today are
   `IReadOnlyList<decimal> Igst/CgstSgst` (`Contracts/ReferenceContracts.cs` → `number[]`), which
   are GST rate ladders, not amounts — so nothing is at risk *yet*, and that is exactly why the
   decision should be made before the first money endpoint lands.
2. **A non-nullable `decimal` is emitted as an *optional* TypeScript property** (`?`), because
   Swashbuckle marks it `nullable: false` but does not put it in `required`. Distinguishing
   "not sent" from "zero" is therefore not possible from the type alone. Same for every
   non-nullable value type on the surface.

---

## 6. The CI job

`.github/workflows/ci.yml`, job **`api-contract`** (`windows-latest`, `shell: bash`):

1. `npm ci` in `frontend/nexgen-web` (ng-openapi-gen is a devDependency; the script calls it
   with `npx --no-install`);
2. `dotnet tool restore` (Swashbuckle CLI, pinned to the same 7.3.1 the API references);
3. `bash tools/generate-api-client.sh --check` — builds the API, regenerates the spec **and**
   the client, and fails on any difference from what is committed, printing the fix command and
   the diff;
4. `npm run typecheck` — the SPA against the regenerated client. This is ADR-002 §6's *"a
   contract change that breaks the client fails the build"*;
5. uploads `api/openapi.json` as an artefact, always, so a reviewer can read the branch's
   contract without running anything.

**Both halves are required.** Drift detection without type-checking misses an API change the
client cannot express; type-checking without drift detection lets the committed spec rot.

**Proven to fail, locally, both ways (2026-08-24):**

| Break | Result |
|---|---|
| Renamed one operation id (`getCurrencyById` → `fetchCurrencyById`) in the controller | exit **1**; diff named `api/openapi.json`, `functions.ts`, `services/currency.service.ts` and the deleted `fn/currency/get-currency-by-id.ts` |
| Hand-edited a generated model file | exit **1**, with the same message: regeneration overwrites the edit, so the edit *is* the drift |
| Unmodified tree | exit **0**, `no drift: the committed contract and client match the API` |

The failure message names `bash tools/generate-api-client.sh`. A drift check whose message does
not say how to fix it gets bypassed.

**Not observed: the job running on GitHub Actions.** An execution session may not push, so
`api-contract` has never executed on a runner. What was verified is the script it runs (locally,
three times, three outcomes above) and that `ci.yml` parses and exposes the job with the
expected steps. The runner-only risks are the ordinary ones — a cold `npm ci`, a cold NuGet
restore, git-bash path handling.

---

## 7. Where the client lives, and what may touch it

```
frontend/nexgen-web/src/app/core/api/
  generated/**     <- ng-openapi-gen output. NEVER hand-edited. Banner in every file.
  *.ts             <- the hand-written wrapper layer: base URL, auth, error narrowing
```

- `eslint.config.js` **ignores** `src/app/core/api/generated/**`. Reason recorded there: the
  generated files carry their own `/* eslint-disable */`, which `reportUnusedDisableDirectives`
  then reports as 15 warnings, and `--max-warnings=0` turns that into a failed build. A lint rule
  could only ever be satisfied by changing the generator, not the code. **What actually stops a
  hand edit is the CI drift check.**
- `.prettierignore` excludes it too: formatting generated output would make the drift check
  depend on the prettier version.
- The existing boundary rule (`eslint.config.js`) still bans feature code from importing
  `core/api/generated/*` directly. Features import the wrapper; the wrapper imports the client.
- Every generated file starts with:

  ```
  // AUTO-GENERATED FROM api/openapi.json - DO NOT EDIT BY HAND.
  // Regenerate with: bash tools/generate-api-client.sh
  ```

---

## 8. Adding an endpoint — the whole procedure

1. Write the controller action to [KB-114](controller-conventions.md), including §11: route
   `Name` (the operation id), `[Tags]` on the controller, `[ProducesResponseType]` for every
   status it can return **and no status it cannot**, and an XML `<summary>` written for the
   client developer.
2. `bash tools/generate-api-client.sh`
3. Read the `api/openapi.json` diff. It is the contract change.
4. `cd frontend/nexgen-web && npm run typecheck`
5. Commit the controller, the spec and the client **together**. They are one change.

`tests/V.SMART.Api.Tests/OpenApiConformanceTests.cs` fails the build if a new action omits its
operation id, duplicates one, omits every `[ProducesResponseType]`, or if a new controller omits
`[Tags]`. Which *statuses* an action declares is a judgement only the action's body can settle,
so that part stays a review duty (KB-114 §11), not an assertion.
