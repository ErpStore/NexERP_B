---
doc_id: KB-124
title: Reference Data and Output Caching
module: api
source_files:
  - V.SMART/V.SMART.Api/Controllers/ReferenceController.cs
  - V.SMART/V.SMART.Api/Caching/TenantScopedOutputCachePolicy.cs
  - V.SMART/V.SMART.Api/Caching/ReferenceCachePolicy.cs
  - V.SMART/V.SMART.Api/Contracts/ReferenceContracts.cs
  - V.SMART/V.SMART.Api/Contracts/GstRateAttribute.cs
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Shared/Utility_Constants/CommonConstants.cs
entities: [State, Currency, UOM, TermsAndConditions, Screens]
api_endpoints:
  - "GET /api/v1/reference/gst-rates"
  - "GET /api/v1/reference/uoms"
  - "GET /api/v1/reference/states"
  - "GET /api/v1/reference/terms"
  - "GET /api/v1/reference/screens"
  - "GET /api/v1/reference/currencies"
database_tables: [State, Currency, UOM, TermsAndConditions, Screens]
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-21
dependencies: [KB-041, KB-060, KB-105, KB-012]
---

# Reference Data and Output Caching

Delivered by **M2-B09**. Closes [KB-041](api-readiness-assessment.md) item **B6** and the
reference-data half of item **C1**.

*(`doc_id` KB-124 is the next free id after **KB-123**, which is held by the unmerged
`migration/M0-10-candelete-guard-audit` branch — checked with `git branch --no-merged master`
per KB-093's id-allocation note.)*

---

## 1. The six lists, measured rather than assumed

Every figure below was measured on 2026-08-21 against **two** databases: the tenant database
rebuilt from source control by the M0-01-03 drill, and the live development database
`NexGenErpDb` (read-only). **They agree exactly**, which is itself worth knowing — it means
these lists are migration-seeded rather than tenant-entered, and a rebuilt environment is
representative for this data class.

| Endpoint | Backing call | Rows | Tenancy | Cached |
|---|---|---:|---|---|
| `/reference/gst-rates` | `CommonConstants.IGSTRates` / `GSTRates` | 12 + 12 | **Global** — compile-time constants, identical everywhere | yes |
| `/reference/states` | `ICommonService.GetStatesAsync()` `:53` | **40** | Per-tenant table, seeded identically | yes |
| `/reference/uoms` | `GetUOMsAsync()` `:181` | **49** | Per-tenant table, seeded identically | yes |
| `/reference/currencies` | `GetCurrenciesAsync()` `:59` | **3** | Per-tenant table, tenant-editable | yes |
| `/reference/screens` | `GetAllScreenAsync()` `:175` | **150** | Per-tenant table, seeded identically | yes |
| `/reference/terms` | `GetAllActiveTermsAsync()` `:122` | **0** | Per-tenant, **tenant-entered** | yes |

**Two findings from the measurement:**

1. **`/reference/terms` returns an empty array in both databases.** `TermsAndConditions` holds
   **zero** rows — not zero *active* rows, zero rows at all. The endpoint is correct; there is
   simply no terms data in any environment reachable from here, so its shape is verified by
   unit test and its behaviour against real data is **unverified**. It is also the only list of
   the six that is genuinely tenant-entered rather than seeded, which makes it the one most
   likely to differ between tenants in production — and therefore the one the tenant-keyed
   cache matters most for.
2. **`/reference/screens` returns 150, not 152.** See **R-65**: 152 rows are seeded and later
   migrations delete two (`ScreenCode` 114 and 115, `Bill Pending List` and `Bill Paid List`).
   This endpoint reports what the database actually holds, so it will disagree with
   `V.SMART.Api/Authorization/ScreenCatalogue.cs`, which still compiles 152 names. **That
   disagreement is a defect in the catalogue, not in this endpoint.**

### Measured cost — and why it does *not* justify caching

```
300 reference queries (states + uoms + screens, ×100)  →  16 ms total
                                                       ≈  0.05 ms per query
```

Measured with `sqlcmd` against the rebuilt tenant database on SQL Server 2019 Express.

**The database cost of these lists is negligible, and this document says so plainly so that
nobody later justifies a cache on grounds that were never true.** These are unindexed-scan-free
reads of 3–150 narrow rows with no joins. If the only cost were the query, caching would not be
worth the complexity.

**What the cache is actually for**, in decreasing order of confidence:

- **Request count.** Every screen needs several of these lists before it renders. The burst is
  the problem, not any single call — and `AllowLocking` collapses a stampede of concurrent
  misses into one upstream call.
- **Per-request work above the query.** Resolving the per-tenant `ApplicationDbContext`,
  materialising entities, projecting, and serialising JSON all happen per request and are
  **not** included in the 0.05 ms above. This was not measured and is stated as *Inferred*.
- **Not payload size.** The largest response is 150 short rows. Nothing here is big.

---

## 2. The cache key is the tenant, and the policy fails closed

`TenantScopedOutputCachePolicy` — one named policy, `"ReferenceData"`, applied to this route
group and **nowhere else**.

### Why the policy is hand-written

ASP.NET Core's **default output-cache policy declines to cache authenticated responses.** Every
endpoint in this group is `[Authorize]`. Composing on the default with
`OutputCachePolicyBuilder` would therefore have produced a cache that **silently stores
nothing**: the endpoints work, the tests pass, the measurements look fine, and nobody discovers
it until someone profiles the API. Opting into authenticated caching has to be explicit, and
the price of opting in is owning the key.

### The key

Five of the six lists are read through `ApplicationDbContext`, resolved **per tenant**. A cache
keyed on the URL alone serves tenant A's data to tenant B. The key therefore includes the
**`TenantId` claim** — the same claim `TenantProvider` and `UserRightsProvider` resolve tenancy
from (BR-TEN-001/002), so the cache and the authorization filter cannot disagree about who the
caller is.

**Fail closed.** If the caller is unauthenticated, or the claim is missing, empty, unparseable
or non-positive, the policy **disables caching for that request** rather than degrading to an
unkeyed entry. A cache miss costs 0.05 ms; a cache hit on an unkeyed entry costs a cross-tenant
disclosure. The asymmetry decides it.

**Not user-keyed, deliberately.** All six lists are tenant-wide and identical for every user in
the tenant. In particular `/reference/screens` is the permission **vocabulary**, not anyone's
rights — a caller's own rights come from `GET /api/v1/me` (M2-A07). Merging the two would make
the response caller-dependent and multiply cache entries by the user base for no benefit. **If
any endpoint in this group ever becomes caller-dependent, it must add the user to the key or
leave the group.**

### Pipeline placement is load-bearing

`app.UseOutputCache()` sits **after** `UseAuthentication()` and `UseAuthorization()`:

- **After `UseAuthentication`** because the key is a claim. Placed earlier, `HttpContext.User`
  is still anonymous when the policy builds the key, the policy fails closed on every request,
  and the cache silently never stores anything.
- **After `UseAuthorization`** so a cache hit is still an authenticated, authorized request.
  The cache short-circuits MVC, not the security pipeline — an expired token gets 401 whether
  or not a cached body exists for that tenant.

### TTL, and the invalidation that deliberately does not exist

| Setting | Value |
|---|---|
| Configuration key | `Caching:ReferenceDataSeconds` |
| Default | **60 seconds** |
| `0` | honoured as "do not cache" — an environment switch with no code change |
| negative / unparseable | falls back to the default rather than disabling caching by accident |

**There is no invalidation, on purpose.** These lists are edited through Blazor screens that
know nothing about this API's cache. A phantom invalidation path that nothing calls is worse
than a short TTL, because it reads as though staleness is handled.

> **Known limitation, stated rather than hidden:** for up to the TTL, a reference list edited in
> the Blazor app is stale over the API. Accepted for this data class; it would **not** be
> acceptable for transactional data, which is why this policy is scoped to one route group and
> is not global.

`Cache-Control: private, max-age=<ttl>` is set on every cached response, so no shared proxy
between the API and the browser holds a tenant-scoped body — the same cross-tenant risk as an
unkeyed server cache, one hop further out.

---

## 3. GST rates: the R-15 fix at the boundary

**The defect.** `CommonConstants.GetIGST`/`GetGST` are `FirstOrDefault(r => r == rate)` over a
`List<decimal>`, so an unknown rate returns `default(decimal)` — **zero** — which is
indistinguishable from the legitimate zero rate. `GetIGST(19m)` returns `0`. A typo becomes a
zero-tax invoice, silently.

**The fix at this layer.** `[GstRate]` (`V.SMART.Api/Contracts/GstRateAttribute.cs`) is a
`DataAnnotations` attribute any request DTO carrying a GST rate applies. It tests **membership**
against the ladder directly — where "absent" and "zero" are distinct answers — and never calls
`GetIGST`/`GetGST`, because their return value *is* the ambiguity being fixed. An off-ladder rate
produces a 400 `application/problem+json` naming the field and listing every permitted value
(ADR-002 §4); `0.000` is still accepted.

**R-15 is `partially resolved`, not closed.** `CommonConstants.cs` is unchanged and still
coerces in-process. Its two methods have **105 call sites** across the Blazor app; changing
their return type or making them throw is a separate decision with a separate blast radius.

### The response exposes the pairing

```json
GET /api/v1/reference/gst-rates
{
  "igst":     [0.000, 0.100, 0.250, 1.000, 1.500, 3.000, 5.000, 6.000, 7.500, 12.000, 18.000, 28.000],
  "cgstSgst": [0.000, 0.050, 0.125, 0.500, 0.750, 1.500, 2.500, 3.000, 3.750,  6.000,  9.000, 14.000]
}
```

Read straight from the domain's own lists, never retyped — a literal here would drift the first
time a rate changed, and the drift would be silent. `cgstSgst[i]` is exactly half of `igst[i]`,
because CGST and SGST each carry half the integrated rate; the relationship is exposed so no
client recomputes it in TypeScript. A unit test asserts the halving across all twelve pairs.

---

## 4. Contracts, and one documented ADR-002 deviation

ADR-002 §2 asks controllers to return the domain's view models. These six return **flat DTOs**
in `V.SMART.Api/Contracts/`, for two reasons that do not generalise:

1. **Navigation properties would serialise a graph.** `Screens.UserRights` is
   `ICollection<UserRight>` — every screen right of every user in the tenant. Returning the
   entity from a cached, authenticated endpoint would put the tenant's whole permission matrix
   on the wire behind a dropdown feed. `Currency.CurrencyRates` is the daily rate feed, on a
   different clock from the rest of the row and the reason the entity is unsafe to cache.
2. **Audit columns are not reference data.** A dropdown needs a code and a label.

A test asserts the **property** — every public property on every reference DTO is a primitive,
`string`, `decimal`, `DateTime` or enum — rather than a field list, so a DTO added later is
covered automatically.

### `/reference/currencies` vs `CurrencyController`

Both exist, deliberately, and are distinguished in Swagger by route group and summary:

| | `/api/v1/currencies` | `/api/v1/reference/currencies` |
|---|---|---|
| Purpose | CRUD surface for the Currency master | populate a selector |
| Shape | paged, sorted, filtered (M2-B02) | flat list |
| Writable | yes | no |
| Cached | no | yes, 60 s |
| Rate feed | available | excluded |

---

## 5. Authorization: `[NoScreenRight]`, and why

Every endpoint is `[Authorize]`. **None carries `[RequireScreen]`**, and that is a decision, not
an omission.

Reference data is what a screen needs **in order to render at all**; no single screen owns "the
list of states". Gating it on a screen right would deadlock the UI for exactly the reason
KB-105 §2.4 gives for `GET /api/v1/me` — a user must be able to fetch the vocabulary before the
app can decide what to show them. The controller therefore carries
`[NoScreenRight("…justification…")]`, the explicit, greppable, auditable opt-out from the
screen-right axis. It is **not** an opt-out from authorization: authentication and tenant
scoping both still apply.

> ⚠ **This interacts with a still-open gap.** `[RequireScreen]` is currently **opt-in, not
> deny-by-default** — an authenticated action on a controller carrying no `[RequireScreen]` is
> allowed through (R-03, closed by **M2-A02**). When M2-A02 makes the filter deny-by-default,
> `[NoScreenRight]` is exactly what it must honour for this controller. That is why the opt-out
> is declared explicitly now rather than left implicit.

---

## 6. What is not verified

- **No end-to-end two-tenant HTTP test.** The cache key and the fail-closed behaviour are
  proven at the policy level against a constructed `OutputCacheContext` — which is where a
  cross-tenant leak would originate — but no test runs two tenants through a live host. That
  needs a `WebApplicationFactory` harness this project does not have, plus two tenant
  databases. **Recorded as the residual risk of this task.**
- **`/reference/terms` against real data**, because no environment reachable from here has any.
- **No live-host measurement.** The 0.05 ms figure is the database's cost, not the endpoint's.
