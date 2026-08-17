---
doc_id: KB-090
title: Blazor-Only Delivery Plan — Effort, Timeline and Budget Estimate
module: migration
source_files:
  - V.SMART/V.SMART.Shared/Pages/
  - V.SMART/V.SMART.Shared/BusinessLayer/
  - docs/kb/modules/module-inventory.md
  - docs/kb/migration/migration-strategy.md
  - docs/kb/risks/technical-debt-register.md
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: estimate
last_verified: 2026-08-17
dependencies: [KB-001, KB-020, KB-060, KB-070, KB-071, ADR-001, ADR-004, ADR-006]
---

# Blazor-Only Delivery Plan — Effort, Timeline and Budget

> **Decision this document costs:** the React rebuild is dropped ([ADR-006](../decisions/ADR-006-blazor-only-delivery.md),
> superseding [ADR-003](../decisions/ADR-003-react-stack.md)). The product stays on
> **Blazor Server (.NET 9)**, and the client's requirement set is delivered **module by
> module** on the existing application.
>
> **Status: proposal.** Every number is an estimate with a stated basis, not a commitment.
> Rates in §8 are placeholders — replace them with your actual cost or billing rates and
> the totals recompute linearly.

---

## 1. What changed, and why it matters commercially

| | Previous plan (React) | This plan (Blazor-only) |
|---|---|---|
| UI stack | New React 19 + Vite app alongside Blazor | Existing Blazor Server app, evolved |
| Backend reach | New REST layer, 60–80 controllers, before any screen ships | Direct in-process DI calls — already working |
| Screens | ~140 screens rebuilt in a second framework | 364 existing components kept, refactored where touched |
| During delivery | Two stacks in production simultaneously | One stack, one deployment, one team |
| Feature freeze | Explicit non-goal: *"no new ERP functionality during the migration"* | **The opposite — new functionality is the whole point** |
| Estimated engineering | ~2,460 PD to deliver the same scope | **1,417 PD** |

Staying on Blazor removes roughly **1,047 person-days (~74% uplift)** of pure re-platforming
cost — API surface, React foundation, screen rebuild premium, dual-stack parity testing —
and redirects it into the change requests in the client presentation.

**The trade-off, stated honestly:** this plan buys no framework modernisation. The known
debt in [KB-060](../risks/technical-debt-register.md) — ~184,000 lines of business logic
sitting in Razor `@code` blocks, UI-only authorization — does not disappear. This plan pays
it down **only in the modules it touches** (the `refactor` column in §5). Modules nobody
opens stay exactly as they are. That is a deliberate, defensible choice, but it should be a
conscious one.

---

## 2. Scope basis

Two independent inputs define the scope:

**(a) The client presentation** (`ERP_presentation.pptx`, 24 slides) — the process flow
chart, the page-by-page requirement list, and the "Current major issues" slide. Every item
is traced to a delivery wave in §4.

**(b) The measured codebase** (verified 2026-08-17, from source):

| Metric | Value |
|---|---|
| Razor components | 364 (333 under `Pages/`) |
| Razor LOC | 327,884 — of which ~57% is C# inside `@code` |
| Business services | 285 files / 128,518 LOC (reusable as-is) |
| Entity classes | 210 files / 196 `DbSet<>` |
| ViewModels | 274 |
| Stored procedures referenced | 94 (82 DDL captured into `db/stored-procedures/` in M0-01) |
| Automated tests | 0 |

Page counts by module folder, which drive the per-wave estimates:

| Folder | Components | LOC |
|---|---|---|
| `Master_Module_pages` | 78 | 51,759 |
| `SalesAndLabour_pages` | 47 | 69,974 |
| `OutSourcing_Module_pages` | 38 | 54,698 |
| `Report_Module_Pages` | 30 | 33,846 |
| `ProductionModule_pages` | 21 | 25,016 |
| `Inventory(Stock)_Module_Pages` | 20 | 15,672 |
| `Home_Pages` | 18 | 3,925 |
| `Planning_Module_Pages` | 16 | 16,786 |
| `HumanResource_Pages` | 15 | 12,629 |
| `Inspection_Pages` | 11 | 8,702 |
| `Maintenance_Pages` | 10 | 5,122 |
| `CashFlow_Pages` | 9 | 11,156 |
| `Settings_Pages` | 8 | 3,268 |
| others | 12 | 8,572 |

### Scope split

- **Core scope** — everything the client presentation asks for, plus the platform work it
  depends on. Waves W0 … E1. **1,417 engineering PD.**
- **Optional scope** — modules the presentation does not touch (Sales/Manufacturing Work,
  Labour Work, HR/Payroll, Accounts, QC, Maintenance). They keep working untouched; this
  budget line only buys the UI-consistency pass and the same hardening. Waves F1 … F4.
  **318 engineering PD.** Defer or drop without affecting the core plan.

---

## 3. Estimation method

Bottom-up per wave, in **person-days (PD)** = one engineer, one 8-hour day. Each wave is
split into three buckets so the client can see what they are paying for:

| Bucket | What it covers |
|---|---|
| **Refactor** | Extract business logic out of `@code` into services, adopt the shared component kit, apply server-side authorization, fix the module's known defects |
| **CR** | New functionality from the client presentation |
| **Test** | Characterisation/parity tests, regression pack, UAT support for that wave |

Unit rates used to build the buckets (calibrated against measured page sizes):

| Screen class | PD |
|---|---|
| Simple master (list + upsert, < 15 fields) | 3 |
| Medium master (30+ fields, child collections) | 6–8 |
| Complex master (`ItemUpsert` = 4,731 LOC; BOM tree) | 20–25 |
| Document screen — simple | 8–10 |
| Document screen — complex (upstream picker + line grid + calc + approval + print) | 16–22 |
| Report (uniform pattern) | 2.5–4 |
| New cross-cutting service (dup-merge, notifications, year-change) | 8–15 |

**Loading applied on top of engineering PD:** dedicated QA +12%, PM/BA/documentation +10%,
contingency +15% on the subtotal.

---

## 4. Requirement traceability — presentation → wave

Every requirement in the deck, mapped. "New" means no equivalent exists in the codebase today.

| # | Requirement (slide) | Wave | New? |
|---|---|---|---|
| 1 | Dashboard: live/complete projects, PO pending by category & value, line items pending project-wise, overdue (s.3) | A3 | Partly — 4 dashboard pages exist |
| 2 | Cost centre + project creation with team, manager, mobile numbers (s.4) | A3 | **New** (CostCenter/ProjectTypeMaster entities exist; team & contacts do not) |
| 3 | Pop-up message at GRN time, project-wise assignment, authorise messages (s.1, s.4) | W1 + B2 | **New** — needs a notification/assignment engine |
| 4 | Admin masters: user, rights matrix, threshold-based authority (s.5) | A1 | Exists — hardening + server-side enforcement |
| 5 | Inventory masters: category/UOM/stores, item master, **duplicate rejection 100%** (s.5, s.23) | A2 | **New rule** on existing screens |
| 6 | Multi-level BOM + raw-material estimation, old price + stock shown in BOM/PR (s.5, s.6) | A2 | Partly new |
| 7 | Item-code continuity end-to-end, no de-linking (s.23) | W1 + A2 | **New** — cross-module traceability |
| 8 | Duplicate check with pop-up **and merge option** (s.1) | W1 | **New** |
| 9 | Customer/vendor master with bank, GSTIN; machine master; states; contract review (s.5) | A1 | Exists |
| 10 | PR creation with old price + available stock (s.6) | B1 | Enhancement |
| 11 | Send enquiry directly from ERP, cost update, comparison, **final price conclusion history** (s.1) | B1 | Partly new (`/rate-comparison` exists; history does not) |
| 12 | Subcontract PO identified from the beginning, **re-categorisable later** (s.1, s.7) | B1 | **New** |
| 13 | PR partial delete — line-wise and qty-wise (s.7) | B1 | **New** |
| 14 | Multiple PO at a time; purchase direct / multiple (s.7) | B1 | **New** |
| 15 | PO status lifecycle: after SCN → Completed; after authorisation + mail → Authorized (s.9) | B1 | Enhancement |
| 16 | Back navigation on every page (s.9) | W1 | Component exists (`SmartBackButton`) — needs global rollout |
| 17 | GRN: PO-direct **or** DC-based with PO entered later (s.7, s.11) | B2 | **New** |
| 18 | GRN price editable after receipt, locked after invoice; **GRN lock** (s.7, s.11) | B2 | **New** |
| 19 | Multiple invoice numbers in one GRN; multiple PO under one invoice (s.7, s.11) | B2 | **New** |
| 20 | Multiple GRN at a time; multiple SCN at a time (s.11, s.12) | B2 | **New** |
| 21 | SCN interlock — only good parts reach stock; rework parts acceptable later (s.12) | B2 | **New** |
| 22 | Subcontract DC/PO/GRN/SCN on **one common UI** (s.8, s.11, s.12, s.23) | B3 | **New** |
| 23 | Subcontract transfer generates stock entry + prints as delivery challan, exploded BOM (s.8) | B3 | **New** |
| 24 | Inter-stock transfer with better accessibility (s.10) | C1 | Exists — UX rework |
| 25 | Inventory: no multiple line items — club them (s.13, s.23) | C1 | **New rule** |
| 26 | Tool crib issue / return / report (s.14) | C1 | Exists — report is new |
| 27 | Stock check (s.1) | C1 | Enhancement |
| 28 | Job order: project → category → **multi-station consolidated JO** (s.15) | C2 | **New** |
| 29 | JO material availability, completed/pending segregation (s.15) | C2 | **New** |
| 30 | Production issue: pick JO → auto-list available stock; multi-JO clubbed issue (s.16, s.23) | C3 | **New** |
| 31 | Production material issue / GRN / SCN / DC / invoice chain (s.17) | C3 | Exists — hardening |
| 32 | GL item DC; GL items pending (s.18, s.19) | D1 | **New** |
| 33 | Outward DC, **RGP / NRGP** with one-level online approval + history (s.20, s.22) | D1 | **New** — no RGP/NRGP anywhere in the codebase |
| 34 | Free-text DC items without adding to item master (s.22) | D1 | **New** |
| 35 | Outward DC usable **from mobile** (s.22) | D1 | **New** |
| 36 | Customer DC inward + status report (s.21) | D1 | **New** |
| 37 | Reports: BOM, PO/PR pending, SCN rejection, stock at vendor, vendor rating, stock analysis/ledger/item history, cost-centre & project expense, project-wise material, PR→SCN consolidated, JO pending, DC pending (s.19) | D2 | Mixed — ~40 reports exist, ~12 new |
| 38 | **Reports must be fast**, data must be correct (s.23) | D3 | **New** — dedicated tuning programme |
| 39 | PO→PR re-opening must never happen; SCN lost under concurrent use — save-path RCA (s.23) | W1 | **New** — transaction/concurrency hardening |
| 40 | PR item missing (s.23) | W1 | **New** — same root cause family |
| 41 | Year change must be smooth (s.23) | W1 | **New** |
| 42 | Whole-backup duplicate checking in parallel (s.1) | A2 | **New** |
| 43 | Item master via direct xls or individual entry (s.1) | A2 | Exists — hardening |

---

## 5. Effort estimate

### 5.1 Core scope

| ID | Wave | Screens | Refactor | CR | Test | **PD** |
|---|---|---|---|---|---|---|
| W0 | Stabilise & safety net (finish M0: secrets, CI, delete guards, characterisation tests) | — | 14 | 0 | 6 | **20** |
| W1 | **Platform foundation** — shared component kit, server-side authz, save/concurrency hardening, notification engine, duplicate-detect & merge, item-code continuity, year-change, feature flags | — | 95 | 30 | 20 | **145** |
| A1 | Masters — Admin, General, Settings, Accounts | 40 | 45 | 15 | 10 | **70** |
| A2 | Masters — Inventory: Item, BOM, Process, RM, Store, HSN, rate updation | 20 | 55 | 55 | 25 | **135** |
| A3 | Project / Cost-centre + Dashboard | 8 | 22 | 35 | 12 | **69** |
| B1 | Out Sourcing — PR/Indent, Purchase Enquiry, Vendor Quote, Rate comparison, PO | 18 | 52 | 60 | 25 | **137** |
| B2 | Purchase — GRN, SCN, Purchase Invoice, Debit Note | 10 | 38 | 55 | 22 | **115** |
| B3 | Sub Contract — DC-out, GRN, SCN, Invoice (common UI with Purchase) | 10 | 28 | 45 | 20 | **93** |
| C1 | Inventory / Stock — MIN, SCNGen, inter-store, tool crib, stock position, stock check | 20 | 48 | 40 | 25 | **113** |
| C2 | Planning / Job Order — JO, route card, RC release, requirement analysis, approvals | 16 | 44 | 40 | 22 | **106** |
| C3 | Production — assembly + component issue/return/SCN, production log | 21 | 48 | 35 | 22 | **105** |
| D1 | DC & Gate Pass — outward DC, RGP/NRGP, customer DC inward, GL item DC, mobile | 10 | 18 | 55 | 20 | **93** |
| D2 | Reports — 40+ existing reports hardened, ~12 new | 45 | 58 | 45 | 20 | **123** |
| D3 | Report / stored-procedure performance tuning, indexing, caching | — | 25 | 0 | 8 | **33** |
| E1 | Cutover — live-data item de-duplication, training, hypercare | — | 40 | 0 | 20 | **60** |
| | **Core total** | | **630** | **510** | **277** | **1,417** |

### 5.2 Optional scope (modules outside the presentation)

| ID | Wave | Screens | Refactor | CR | Test | **PD** |
|---|---|---|---|---|---|---|
| F1 | Sales & Manufacturing Work — enquiry → quote → SO → DC → invoice, e-Invoice/e-Way | 47 | 70 | 25 | 30 | **125** |
| F2 | Labour Work — GRN, SCN, DC-out, invoice, credit note | 20 | 45 | 10 | 18 | **73** |
| F3 | HR / Payroll, Accounts & Cash Flow | 24 | 45 | 10 | 18 | **73** |
| F4 | Inspection/QC, Maintenance, Utilities | 24 | 30 | 5 | 12 | **47** |
| | **Optional total** | | **190** | **50** | **78** | **318** |

### 5.3 Loaded effort

| | Engineering | QA +12% | PM/BA +10% | Subtotal | Contingency +15% | **Loaded PD** |
|---|---|---|---|---|---|---|
| **Core** | 1,417 | 170 | 142 | 1,729 | 259 | **1,988** |
| Optional | 318 | 38 | 32 | 388 | 58 | **446** |
| Core + Optional | 1,735 | 208 | 174 | 2,117 | 318 | **2,435** |

---

## 6. Staffing options

Capacity assumption: **18 effective PD per FTE per month** (≈21 working days less leave,
ceremonies and support interruptions).

| Option | Team | FTE | Capacity | Core duration |
|---|---|---|---|---|
| **A — recommended** | 1 lead, 2 senior + 2 mid Blazor devs, 1 SQL/report dev, 1 QA, 0.5 BA/PM, 0.25 DevOps | 7.75 | 140 PD/mo | **~14 months** |
| B — lean | 1 lead, 1 senior + 1 mid dev, 1 SQL dev, 0.5 QA, 0.5 BA/PM, 0.25 DevOps | 5.25 | 95 PD/mo | ~21 months |
| C — accelerated | 1 lead, 3 senior + 3 mid devs, 1.5 SQL, 1.5 QA, 1 BA/PM, 0.5 DevOps | 11.50 | 207 PD/mo | ~11 months (incl. 12% coordination penalty) |

Option C does not scale linearly: the platform wave (W1) is a hard serial dependency for
every module wave, and A2 (Item/BOM) gates the whole document chain. Above ~8 engineers the
critical path, not headcount, sets the date.

Option A is recommended. Option C is worth it only if the client's pain (item duplication,
lost SCNs, slow reports) is costing them more per month than the extra ~₹6 L/month of team.

---

## 7. Timeline — Option A, module by module

Three parallel tracks. Each wave ships to production behind a per-tenant feature flag; the
old screen stays reachable until the new one passes UAT.

| Month | Track 1 — platform | Track 2 — modules | Track 3 — reports & data |
|---|---|---|---|
| M1 | W0 stabilise · W1 kit + authz | — | Report inventory & baseline timings |
| M2 | W1 save/concurrency RCA + fix | A1 masters (admin/general/settings) | SP profiling |
| M3 | W1 dup-merge · notifications · year-change | A1 → **Release R1** | D3 indexing round 1 |
| M4 | W1 close-out | A2 Item/BOM (duplication + continuity) | D2 reports wave 1 |
| M5 | Support | A2 → **Release R2** · A3 project/dashboard | D2 reports wave 2 |
| M6 | Support | A3 → **Release R3** · B1 out-sourcing | D2 reports wave 3 |
| M7 | Support | B1 → **Release R4** | D3 tuning round 2 |
| M8 | Support | B2 purchase GRN/SCN/invoice | D2 new reports |
| M9 | Support | B2 → **Release R5** · B3 sub-contract | D2 vendor rating / stock analysis |
| M10 | Support | B3 → **Release R6** · C1 inventory | D3 tuning round 3 |
| M11 | Support | C1 → **Release R7** · C2 planning/JO | D2 project & cost-centre reports |
| M12 | Support | C2 → **Release R8** · C3 production | D2 close-out |
| M13 | Support | C3 → **Release R9** · D1 DC/RGP/NRGP | E1 live-data de-duplication prep |
| M14 | Support | D1 → **Release R10** | E1 cutover + training |
| M15 | Hypercare | Hypercare | Hypercare |

Nine production releases before month 14, first one at **month 3**. If the optional waves
(F1–F4) are commissioned, add **~3.2 months** at Option-A capacity, or run them on a
separate pair of engineers in parallel from M6 with no impact on the core date.

### Critical path

`W1 platform` → `A2 Item/BOM` → `B1 Out-sourcing` → `B2 Purchase` → `C1 Inventory` →
`C2/C3 Planning + Production` → `E1 cutover`.

Everything else can slip without moving the end date. W1 and A2 cannot.

---

## 8. Budget

> **All rates below are placeholders.** Substitute your real cost or billing rates; every
> total is a linear function of them.

### 8.1 Rate card assumption (monthly, INR, fully-loaded cost)

| Role | Count | Monthly | Cost |
|---|---|---|---|
| Tech lead / architect | 1 | 2,20,000 | 2,20,000 |
| Senior Blazor/.NET developer | 2 | 1,40,000 | 2,80,000 |
| Mid .NET developer | 2 | 90,000 | 1,80,000 |
| SQL / report developer | 1 | 1,00,000 | 1,00,000 |
| QA engineer | 1 | 75,000 | 75,000 |
| BA / project manager | 0.5 | 1,50,000 | 75,000 |
| DevOps | 0.25 | 1,20,000 | 30,000 |
| **Team A monthly run rate** | **7.75** | | **₹9,60,000** |

Blended internal cost = **₹6,882 per person-day** (₹9.6 L ÷ 140 PD).

### 8.2 Programme cost (internal / cost basis)

| Line | Loaded PD | Cost (INR) |
|---|---|---|
| Core scope | 1,988 | ₹1.37 Cr |
| Non-labour — infra, SQL Server, dev tools, FastReport, e-Invoice sandbox, training material | — | ₹12,00,000 |
| **Core total** | | **₹1.49 Cr** (≈ USD 169k at ₹88/$) |
| Optional scope (F1–F4) | 446 | ₹0.31 Cr |
| **Core + optional** | 2,434 | **₹1.82 Cr** |

### 8.3 Indicative price if this is quoted to a customer

| Billing rate | Core | Core + optional |
|---|---|---|
| ₹10,000 / PD | ₹1.99 Cr | ₹2.43 Cr |
| ₹12,000 / PD | ₹2.39 Cr | ₹2.92 Cr |
| ₹14,000 / PD | ₹2.78 Cr | ₹3.41 Cr |

At ₹12,000/PD the core programme carries a gross margin of roughly 43%.

### 8.4 Per-wave price — for module-by-module contracting

Since delivery is module by module, each wave can be a separately signed and separately
invoiced work order. Loaded PD = engineering × 1.403.

| Wave | Loaded PD | Cost basis | @ ₹12,000/PD |
|---|---|---|---|
| W0 Stabilise | 28 | ₹1.9 L | ₹3.4 L |
| W1 Platform foundation | 203 | ₹14.0 L | ₹24.4 L |
| A1 Masters — admin/general/settings | 98 | ₹6.7 L | ₹11.8 L |
| A2 Masters — Item / BOM | 189 | ₹13.0 L | ₹22.7 L |
| A3 Project & dashboard | 97 | ₹6.7 L | ₹11.6 L |
| B1 Out sourcing | 192 | ₹13.2 L | ₹23.0 L |
| B2 Purchase | 161 | ₹11.1 L | ₹19.3 L |
| B3 Sub contract | 130 | ₹8.9 L | ₹15.6 L |
| C1 Inventory / stock | 159 | ₹10.9 L | ₹19.1 L |
| C2 Planning / job order | 149 | ₹10.3 L | ₹17.9 L |
| C3 Production | 147 | ₹10.1 L | ₹17.6 L |
| D1 DC & gate pass (RGP/NRGP) | 130 | ₹8.9 L | ₹15.6 L |
| D2 Reports | 173 | ₹11.9 L | ₹20.8 L |
| D3 Report performance | 46 | ₹3.2 L | ₹5.5 L |
| E1 Cutover & hypercare | 84 | ₹5.8 L | ₹10.1 L |
| **Core** | **1,988** | **₹1.37 Cr** | **₹2.39 Cr** |
| F1 Sales & manufacturing work | 175 | ₹12.0 L | ₹21.0 L |
| F2 Labour work | 102 | ₹7.0 L | ₹12.3 L |
| F3 HR / accounts | 102 | ₹7.0 L | ₹12.3 L |
| F4 QC / maintenance / utilities | 66 | ₹4.5 L | ₹7.9 L |

### 8.5 Suggested payment milestones

| Milestone | Trigger | % of core |
|---|---|---|
| Mobilisation | Work order signed | 10% |
| R1 | Masters + platform live | 15% |
| R2 | Item/BOM with duplicate control live | 15% |
| R4 | Out-sourcing (PR → PO) live | 15% |
| R6 | Purchase + sub-contract live | 15% |
| R8 | Inventory + planning live | 10% |
| R10 | Production + DC/RGP live | 10% |
| Go-live acceptance | Cutover + 30 days hypercare | 10% |

---

## 9. Assumptions

1. Business services (`V.SMART.Shared/BusinessLayer`, 128,518 LOC) are reused, not
   rewritten ([ADR-001](../decisions/ADR-001-keep-existing-backend.md)).
2. No database schema replacement. Additive tables/columns only (project team, RGP/NRGP,
   duplicate-merge audit, notification queue).
3. EF Core, AutoMapper, FastReport and the 94 stored procedures stay.
4. One product line, one codebase, all tenants — no per-tenant forks.
5. The client provides a product owner for ~1 day/week and UAT users per wave. UAT
   turnaround ≤ 5 working days per release; delay here moves the date one-for-one.
6. Requirements are frozen per wave at wave start. Mid-wave changes go to the next wave or
   a change order.
7. Legacy data quality is the client's to fix with our tooling: E1 buys the de-duplication
   tooling and one supervised run, not unlimited data cleansing.
8. Blazor Server keeps its current hosting model. Mobile access (requirement 35) is
   delivered as a responsive Blazor screen, not a new native app; a MAUI packaging of the
   DC screens would be an additional ~35 PD.
9. Rates, FX (₹88/USD) and the 18 PD/FTE/month capacity are placeholders.
10. GST/taxes, travel and on-site training days are excluded from §8.

---

## 10. Risks and their cost impact

| Risk | Impact if it lands | Mitigation |
|---|---|---|
| **Zero existing tests** — every refactor is unguarded | +10–15% across all waves | W0 characterisation tests on `ICalculationService` and `IStockManagerService` before any wave touches them |
| **Save-path RCA is open-ended** — PO→PR re-opening and lost SCNs are reported symptoms, root cause unproven | W1 could double (145 → ~200 PD) | Time-boxed 10-day RCA in M1; re-baseline W1 at its end. This is the single most likely estimate breaker |
| **Item de-duplication in live data** — merging item codes in a live ERP rewrites history across every transaction table | E1 +20–40 PD | Dry-run on a tenant copy; merge is reversible via audit table; no merge without owner sign-off |
| **Report performance may be schema-bound**, not query-bound | D3 +20 PD, or a schema change | Profile before committing (M1–M3); if indexing does not close the gap, escalate to a reporting read-model as a change order |
| `@code` extraction cost is unmeasured — the 184k-LOC figure is real but per-module cost is not | Waves B1–C3 ±20% | A2 is the first measurement; re-baseline the whole plan after it |
| Scope drift from an evolving requirement list (the deck is a wish list, not a spec) | Unbounded | Wave-level requirement freeze + change-order process (§9.6) |
| Single-tenant assumptions surface in multi-tenant rollout | +15 PD per surprise | Roll each release to one pilot tenant first |
| Key-person dependency on the lead | Schedule | Two engineers on every wave; ADRs for every non-obvious decision |

---

## 11. What is explicitly out of scope

- Any React, Angular or other JavaScript SPA work. The Angular pilot in
  `frontend/vsmart-erp/` is archived, not converted.
- Database platform change, ORM change, reporting-engine change.
- Rewriting business services that are not touched by a commissioned wave.
- New statutory integrations beyond the existing e-Invoice / e-Way Bill.
- Native mobile applications (see assumption 8).
- Modules F1–F4 unless separately commissioned.

---

## 12. Recommendation

1. Sign **W0 + W1 + A1 + A2** as phase one (₹35.6 L cost basis / ₹62.3 L at ₹12k/PD,
   ~5 months). It delivers the two things the client's own "Current major issues" slide puts
   first — item duplication and item continuity — and it produces the measurement that
   re-baselines everything after it.
2. Re-estimate waves B1 onward at the end of A2, using actual extraction and CR velocity.
3. Hold the optional waves (F1–F4) until the core is live. They are real work, but nothing
   in the presentation asks for them.
