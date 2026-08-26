---
doc_id: ADR-002
title: REST API layer, contract conventions, and tenant resolution for the SPA
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-26
dependencies: [KB-014, KB-040, KB-041]
---

# ADR-002 — REST API layer and contract conventions

**Status:** Accepted · **Date:** 2026-08-12

## Context

The SPA needs an HTTP surface. Today there are 6 endpoints across 2 controllers, with
inconsistencies already visible in that small sample: two different 400 body shapes from
one endpoint, untyped `Dictionary<string, object>` filters, no route/body id validation, no
versioning. Replicating those across 60–80 controllers would bake them in permanently.

Separately, tenant resolution currently depends on the request host — which breaks when the
SPA is served from its own origin.

## Decision

### 1. REST over GraphQL/gRPC

REST with resource-oriented routes. The domain is document-CRUD with explicit workflow
commands; the existing service methods map to it almost one-to-one. GraphQL would require
a resolver layer with no corresponding benefit; gRPC does not fit browser-first delivery
or the file/PDF payloads this app needs.

### 2. Contract conventions (mandatory for every controller)

```
GET    /api/v1/{resource}?pageNumber&pageSize&sort&<typed filters>
       → { items, totalCount, pageNumber, pageSize }
GET    /api/v1/{resource}/{id}
POST   /api/v1/{resource}                 → 201 + Location
PUT    /api/v1/{resource}/{id}            → 200
DELETE /api/v1/{resource}/{id}            → 204 | 409 (business rule) | 404
POST   /api/v1/{resource}/{id}/{command}  → workflow commands
GET    /api/v1/{resource}/{id}/print      → application/pdf
```

- Resources are **plural kebab-case**: `/api/v1/sales-orders`, `/api/v1/purchase-orders`.
- Payloads are the **existing `…VM` ViewModels, unchanged** — no parallel DTO hierarchy.
- Filters are **typed query DTOs**, not `Dictionary<string, object>`.
- Controllers are thin: bind → authorize → one service call → map. No business logic.

#### 2a. Addendum — the paged list contract (M2-B02, 2026-08-20)

This addendum refines §2; it does not replace anything above. It was written from the reference
implementation on `GET api/currencies` and everything in it is **Confirmed** against code unless
marked otherwise. The types live in `V.SMART/V.SMART.Api/Contracts/`.

**Response.** One generic `PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber,
int PageSize)` for every list endpoint. Verified in `/swagger/v1/swagger.json` as a single
`CurrencyVMPagedResult` schema serialising `{ items, totalCount, pageNumber, pageSize }`. There is
deliberately **no `totalPages`**: §2 names four properties, the generated client freezes at
M2-B03, and it is derivable client-side. (An unrelated, unreferenced
`V.SMART.Shared.ViewModels.PagedResult<T>` exists at `RejectionMasterVM.cs:33-40` and does carry
`TotalPages`; it is not this type — alias if a file ever needs both.)

**Request.** A per-resource `record …Query : PagedQuery`. `PagedQuery` carries
`pageNumber` (default **1**), `pageSize` (default **20**, maximum **100**) and `sort`.

- The **maximum page size is 100**. An unbounded `pageSize` is a denial-of-service vector: the
  tenant `ApplicationDbContext` allows a 60-second command timeout
  (`TenantDbContextFactory.cs:22`) and every row is materialised and AutoMapper-projected. 100
  covers every page size the live Blazor list offers — 10/20/50, `CurrencyList.razor:85-87`.
- Date filters are `DateTime?`, never `string?`. Model binding rejects an unparseable value as a
  400; the old `string?` form was re-parsed inside the filter builder, where a parse failure fell
  through `_ => query` and was **silently discarded** (`CurrencyService.cs:201,203,206`).
- **Behaviour change, deliberate:** `GET api/currencies` defaulted to `pageSize = 10`
  (pre-M2-B02 `CurrencyController.cs:30`). It now takes the contract-wide default of 20. Callers
  that send `pageSize` explicitly are unaffected.

**Query-parameter names are camel case, and every property declares its own.** The wire names
are `pageNumber`, `pageSize`, `sort` and the resource's filters (`currName`, `createdBy`,
`fromDate`, `toDate`) — the same casing as the JSON response body and as the `sort` field names
above. This is **not** free: `[FromQuery]` on a record binds by *C# property name* and
Swashbuckle emits that name verbatim, so without an explicit `[FromQuery(Name = "…")]` on each
property the OpenAPI document advertises `PageNumber`/`CurrName` while this document and KB-040
say `pageNumber`/`currName`, and M2-B10 generates its TypeScript client from the document. Every
property on `PagedQuery` and on each `…Query` therefore carries `[FromQuery(Name = …)]`, sourced
from a `const` (`PagedQuery.PageNumberParameter` and siblings) so the attribute and the code that
reports errors cannot drift apart. For the same reason, `IValidatableObject` member names are the
**wire** names, not `nameof(Property)`: a member name becomes the `errors` dictionary key
verbatim, and a binding failure on the same field is keyed by its `[FromQuery]` name — using
`nameof` keys one field two ways depending on which check rejected it. Binding itself stays
case-insensitive, so a caller sending `PageSize` is still accepted. Guarded by
`tests/V.SMART.Api.Tests/PagedContractTests.cs`
(`Every_query_property_declares_its_camel_case_wire_name`); observed in
`/swagger/v1/swagger.json` on 2026-08-20 as `currName, createdBy, fromDate, toDate, pageNumber,
pageSize, sort`.

**Sort syntax.** A comma-separated list of camel-case field names, `-` prefixed for descending:
`sort=-createdDate,currName`. One parameter, survives URL encoding, and is the form generated
clients expect. Terms apply in the order written. **Absent `sort` means the service's existing
default ordering** — for Currency, `OrderByDescending(x => x.CurrId)` (`CurrencyService.cs:279`; the same expression stood at `:56` before this task)
— never "no ordering". When a sort is supplied, the primary key is appended as a final tie-break
so paging cannot repeat or drop rows on a non-unique key.

**Sortable-field allow-list.** Every resource declares an explicit list of sortable wire names
(`CurrencyQuery.Sortable`). Reflecting an arbitrary string onto an `IQueryable` is an
injection-shaped API surface even through EF, and a reflection-derived list changes silently when
a property is renamed. An unknown field is **400, and the message lists the permitted values**.
The list is derived from what the list screen shows, not from the entity's whole property set.

**Validation → 400 `application/problem+json`** (§4, via M2-A06's
`InvalidModelStateResponseFactory`, `ErrorContractExtensions.cs:21-25`): `pageNumber < 1`;
`pageSize < 1`; `pageSize > 100`; an unparseable date; `fromDate > toDate`; a `sort` field not on
the allow-list, or repeated. All are DataAnnotations/`IValidatableObject` on the query record, so
no controller writes error-mapping code.

**Adapter, not a service rewrite.** `FilterDictionaryAdapter` maps the typed query onto the
`Dictionary<string, object>` the existing services take (§Consequences authorises exactly this).
One explicit method per resource — **never reflection**, because a renamed property would then
stop filtering silently for the same `_ => query` reason. The dictionary never appears on the
wire. Dates are handed over as `yyyy-MM-dd` invariant strings: the builder stringifies and
re-parses with the server's current culture, and both predicates use `.Date`, so nothing is lost.

**How `sort` reaches a service whose ordering is hardcoded (INV-041).** `SearchWithDynamicFilterAsync`
is declared **134 times** across `V.SMART.Shared/BusinessLayer/` with a uniform
`(int, int, Dictionary<string, object>?)` signature, consumed by **67** nested `*FilterBuilder`
classes; **no** service anywhere takes a sort parameter (re-measured 2026-08-20). Three options
were evaluated against code:

| Option | Verdict |
|---|---|
| **1. Additive overload** `(int, int, Dictionary<string,object>?, string? sort)` | **Chosen.** Compiler-checked, breaks no caller — the only production call site, `CurrencyList.razor:344-348`, uses three named arguments and still binds to the three-argument member, which now delegates with `sort: null`. The 134 sites convert per module, in that module's own wave, never in one sweep. |
| 2. Reserved `"__sort"` key in the filter dictionary | **Rejected.** Every `*FilterBuilder.ApplyFilter` ends `_ => query` (`CurrencyService.cs:206`), so an unrecognised key is silently ignored and the request answers 200 while sorting nothing. This is not hypothetical: `CurrencyList.razor:760` already sets a `Status` filter key that `CurrencyFilterBuilder` has no case for and `Currency` has no column for, so that live dropdown filters nothing, silently. A contract whose violation is invisible is worse than one that fails loudly. |
| 3. Sort the materialised page in the controller | **Rejected.** `Skip`/`Take` run before it (`CurrencyService.cs:80-81`), so it sorts one page of rows — the wrong rows — and "page 2 of a sorted list" becomes meaningless. Recorded explicitly so it is not re-proposed. |

The chosen mechanism keeps the sort **allow-list in two places that must agree**: the API's
per-resource list (the 400) and the service's `*SortBuilder` switch (the SQL). Drift fails
**loudly** — the service throws `ArgumentException` naming the permitted values rather than
ignoring the term — which is the property option 2 could not offer.

#### 2b. Addendum — money is a JSON string, not a JSON number (Q-85, 2026-08-26)

This addendum refines §2; it does not replace anything above. It was raised by the `M2-C10`
diagnosis and decided by the repository owner. Full measurement:
[`Q-85`](../open-questions.md); the mechanism: [KB-114 §8a](../api/controller-conventions.md).

**The problem.** `System.Text.Json` writes a `decimal` losslessly as JSON number *text* — the
wire itself is exact. But a JSON *number*, once read by a browser's own `JSON.parse`, becomes
an IEEE-754 double, and that conversion silently discards precision beyond what a double can
hold. This happens before any client code — including a `decimal.js`-based guard — ever sees
the value, so no amount of frontend arithmetic care fixes a value already rounded on arrival.

**The decision.** A money-typed `decimal`/`decimal?` property is annotated
`[JsonConverter(typeof(MoneyJsonConverter))]` (`V.SMART.Api/Contracts/MoneyJsonConverter.cs`)
and crosses the wire as a JSON string — `"1234.56"`, never `1234.56`. The client parses the
exact string instead of trusting a value a browser's own parser already rounded.

**Deliberately narrow, not a blanket rule for every `decimal`.** GST rates and quantities are
`decimal` too and are **not** money — they stay plain JSON numbers. Three options were on the
table: (a) string for money specifically — **chosen**; (b) accept the double everywhere and
confine `M2-C10` to post-parse arithmetic, documenting the precision bound — rejected, because
it leaves the underlying loss uncorrected for the one kind of value (money on a tax invoice)
where exactness actually matters; (c) an even finer per-field split — folded into (a), since
"money" already is the per-field line being drawn. Which specific properties count as money is
a controller-by-controller judgement, not something the type system infers — see KB-114 §8a's
worked example.

**Not yet a breaking change.** No live endpoint exposes a money field as of this decision
(verified 2026-08-26: the only `decimal`s reachable through any of the six controllers are the
GST rate arrays, which are rates). This is adopted ahead of the first controller that needs
it — the `M2-C05`+ document series — so that controller ships with the exact contract from the
start rather than a `number` that has to be widened to a `string` later.

### 3. Workflow commands are server-side and atomic

Anything that is a business operation — cancel, short-close, approve, reject, release,
post — is a single `POST /{id}/{verb}` that runs the **entire** sequence server-side.
The client collects input (e.g. the mandatory cancellation reason of BR-SO-003) and calls
one endpoint. **The client never orchestrates a multi-step business operation.**

This directly addresses the current situation where `MfgPOUpsert.razor` sequences
validation → transaction check → quantity revert → status update from the UI.

### 4. Error contract: `application/problem+json` everywhere

| Status | Meaning | Body |
|---|---|---|
| 400 | model binding / `DataAnnotations` | `errors` dictionary keyed by field |
| 401 | unauthenticated / token expired | — |
| 403 | screen right or approval authority denied | which screen + right |
| 404 | not found | — |
| **409** | **business-rule refusal** | `title` carries the **service's existing message verbatim** |
| 500 | unhandled | `traceId` only |

The 409 rule matters: strings like *"Cannot delete this Sales Order as a Sales DC
transaction exists."* are product UX written by the domain team. They are surfaced
unchanged, never replaced with generic text.

### 5. Tenant resolution for the SPA

**Tenant is supplied in the login request** and carried in the JWT thereafter.

```
POST /api/v1/auth/login  { tenant, username, password }
  → resolve tenant by Name/Hostname from MasterDb
  → authenticate
  → JWT with the existing TenantId claim
Every subsequent request → tenant from the JWT claim (existing resolution step 1, unchanged)
```

The SPA derives `tenant` from its own subdomain where available, falling back to a picker.
`TenantProvider`, `TenantDbContextFactory`, `TenantInfo`, and the database-per-tenant model
are **unchanged**.

CORS moves from the hardcoded `http://localhost:4200` to a per-environment configured
origin list.

### 6. Versioning and generation

- All routes under `/api/v1`.
- OpenAPI is the contract; the **TypeScript client is generated in CI**, never hand-written.
  A contract change that breaks the client fails the build.

## Consequences

**Positive.** One template makes 60–80 controllers mechanical and parallelisable. The
generated client eliminates drift. Workflow commands push orchestration where it belongs
and make the `@code` extraction goal concrete. Tenant isolation stays exactly as sound as
it is today.

**Negative.** The existing `CurrencyController` must be reworked to the convention (small,
do it first as the template). `/api/auth/login` gains a `tenant` field — a breaking change
to the Angular pilot, which is being archived anyway.

**Neutral.** Some service signatures fit REST awkwardly (e.g. dynamic-filter dictionaries);
those get a typed DTO at the controller and an adapter into the existing service, without
changing the service.
