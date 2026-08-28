---
doc_id: KB-051
title: Proposed Design System
module: frontend-new
source_files: []
status: proposal
confidence: n/a
last_verified: 2026-08-28
dependencies: [KB-015, KB-050]
---

# Proposed Design System

> **Proposal.** Designed for an ERP where users spend an eight-hour shift entering and
> reconciling structured data. Density, keyboard flow, and scan-ability beat decoration.
> The goal is a product that reads as **new**, not as a reskinned Blazor app.

## Principles

1. **Density first.** Default row height 36 px (compact 30 px). More rows visible = fewer
   scrolls = faster work.
2. **Keyboard is the primary input.** Every action reachable without a mouse; `Tab` order
   follows visual order; `Enter` commits a row and creates the next one.
3. **Never hide state.** Document status, balance quantities, and approval level are always
   visible, never behind a hover.
4. **One accent colour.** Colour carries meaning (status, validity), not branding flourish.
5. **Predictable geometry.** Same page skeleton on all 150 screens: header → filters →
   content → footer actions.
6. **Errors are specific.** Show the server's business-rule message, not "An error occurred".
7. **Respect the muscle memory.** Users know the current document flow. Modernise the
   surface, keep the sequence.

## Tokens

> **Implementation status (M2-C04-01, Angular, 2026-08-23).** The token layer is implemented in
> `frontend/nexgen-web/src/styles/tokens.css` (the values, two independent palettes) and
> `frontend/nexgen-web/src/app/core/theme/` — `tokens.ts` (the names, as types),
> `theme.preset.ts` (the PrimeNG preset, expressed entirely over `var(--…)`),
> `theme.service.ts` (signals; writes `<html data-theme>`), `density.ts` and
> `breakpoints.ts`, plus `src/app/shared/components/theme-toggle/`.
> Typography sizes and line-heights, the spacing scale,
> the radii, the motion durations and easing, the breakpoints and the two densities shipped
> **verbatim** as specified below. Two shapes differ without changing a value: font weight is
> a four-step scale (`--weight-regular|medium|semibold|bold`) rather than a weight per size
> token, and elevation `2`/`3` needed concrete values because this document names them only
> by role — `0 4px 12px rgba(0,0,0,.1)` and `0 12px 32px rgba(0,0,0,.16)`; `1` is verbatim,
> and all four are `none` in dark, per "dark mode uses lighter surfaces rather than shadows".
> Eight colour values were
> corrected to meet this document's own contrast commitment — see
> [Contrast corrections](#contrast-corrections-m2-c04-01-measured-2026-08-19). Enforcement is
> mechanical: an ESLint rule and `src/app/core/theme/no-raw-colour.spec.ts` reject a colour
> literal anywhere in `src/**` except `tokens.css`, and `src/app/core/theme/tokens.spec.ts`
> fails if `tokens.css` and `tokens.ts` drift apart. Not implemented here: every component that *consumes* the tokens (M2-C03,
> M2-C04-02, M2-C04-03, M2-C05-01).

### Colour

Semantic tokens only — components never reference a raw hue.

**Implemented by M2-C04-01** in `frontend/nexgen-web/src/styles/tokens.css`. The
*Light* and *Dark* columns are what shipped; the eight values that differ from this
document's original proposal are marked **corrected** and explained immediately below.

| Token | Light | Dark | Use |
|---|---|---|---|
| `--bg-canvas` | `#F7F8FA` | `#0E1116` | page background |
| `--bg-surface` | `#FFFFFF` | `#161B22` | cards, panels, grid |
| `--bg-surface-raised` | `#FFFFFF` | `#1C2128` | modals, popovers |
| `--bg-subtle` | `#EEF1F5` | `#21262D` | table header, zebra, disabled |
| `--border` | `#7C8794` **corrected** | `#6B7684` **corrected** | dividers, inputs |
| `--border-strong` | `#5E6874` **corrected** | `#8B95A1` **corrected** | focused/active borders |
| `--text-primary` | `#12181F` | `#E6EDF3` | body |
| `--text-secondary` | `#5A6572` | `#9BA6B2` | labels, hints |
| `--text-disabled` | `#616D7C` **corrected** | `#838E9B` **corrected** | |
| `--accent` | `#2563EB` | `#4C8DFF` | primary actions, links, focus |
| `--accent-subtle` | `#E8F0FE` | `#16243D` | selected row, active nav |
| `--success` | `#147C3B` **corrected** | `#3FB950` | posted, approved, in stock |
| `--warning` | `#B05109` **corrected** | `#D29922` | pending, partial, low stock |
| `--danger` | `#B91C1C` | `#F85149` | cancelled, rejected, over-issue |
| `--info` | `#0E7490` | `#39C5CF` | draft, informational |
| `--focus-ring` | `#2563EB` solid **corrected** | `#4C8DFF` solid **corrected** | 2 px ring, 2 px offset |

Both themes are authored as first-class palettes, not a filter over one another.

#### Contrast corrections (M2-C04-01, measured 2026-08-19)

This document commits, under *Accessibility commitments*, to **≥ 4.5:1 for text and ≥ 3:1 for
UI boundaries in both themes**. Its originally proposed palette did not meet that commitment.
`frontend/nexgen-web/src/app/core/theme/contrast.spec.ts` computes the WCAG 2.x ratio for every
ink × background token pair in both themes; the ratios below are its output, not an estimate.

**The threshold was never lowered.** The failing values were corrected, keeping each token's
hue and role. The matrix is the full cross-product of the five tokens usable as a background
(`--bg-canvas`, `--bg-surface`, `--bg-surface-raised`, `--bg-subtle`, `--accent-subtle`)
against every ink token, because nothing in a token layer restricts which pairing a future
component picks — the worst pair in that matrix is the one the user eventually sees.

| Token | Theme | Proposed | Worst ratio as proposed | Shipped | Worst ratio shipped |
|---|---|---|---|---|---|
| `--border` | light | `#D8DEE6` | **1.18:1** (on `--accent-subtle`) | `#7C8794` | 3.19:1 |
| `--border` | dark | `#2D333B` | **1.19:1** (on `--bg-subtle`) | `#6B7684` | 3.30:1 |
| `--border-strong` | light | `#B4BDC8` | **1.66:1** | `#5E6874` | 4.94:1 |
| `--border-strong` | dark | `#404752` | **1.62:1** | `#8B95A1` | 5.01:1 |
| `--text-disabled` | light | `#98A2AE` | **2.26:1** | `#616D7C` | 4.60:1 |
| `--text-disabled` | dark | `#6B7684` | **3.30:1** | `#838E9B` | 4.57:1 |
| `--success` | light | `#15803D` | **4.38:1** | `#147C3B` | 4.61:1 |
| `--warning` | light | `#B45309` | **4.38:1** | `#B05109` | 4.55:1 |
| `--focus-ring` | light | `#2563EB` @ 40 % | **1.74:1** composited | `#2563EB` solid | 4.51:1 |
| `--focus-ring` | dark | `#4C8DFF` @ 45 % | **2.02:1** composited | `#4C8DFF` solid | 4.76:1 |

**Re-measured 2026-08-23** by the Angular reimplementation
(`src/app/core/theme/contrast.spec.ts`, an independently written WCAG 2.x relative-luminance
computation over the same five-background × every-ink matrix): **every ratio in this section reproduced
to two decimal places, and no pair failed.** No value was changed, so nothing in this table is
re-stated.

Everything not listed shipped **verbatim**: all four background tokens, `--accent-subtle`,
`--text-primary` (15.58:1 light / 12.88:1 dark at worst), `--text-secondary` (5.18 / 6.15),
`--accent` (4.51 / 4.76), `--danger` (5.65 / 4.54), `--info` (4.68 / 7.29), and dark
`--success` (5.99) and `--warning` (6.03).

Two of the corrections are worth understanding rather than just accepting:

- **`--border` is now visibly darker than proposed.** A 1.2:1 hairline is decoration, not a
  boundary. WCAG 1.4.11 exempts a purely decorative divider, so a narrower reading of the
  matrix could have kept `#D8DEE6` — but this token is also the border of inputs and grid
  cells, where it *is* the control's boundary. Scoping the matrix was not a call for an
  implementer to make alone, so the conservative reading was taken. **If the design owner
  wants the lighter hairline back for non-interactive dividers, that needs a separate
  decorative token and an explicit decision here — not a threshold change.**
- **`--focus-ring` became a solid colour.** An alpha wash cannot reach 3:1 against a light
  background at any tint (measured 1.74:1 at 40 %), and WCAG 2.2 §2.4.11/§1.4.11 require the
  focus indicator to.

#### Theme persistence — correcting this document

The sentence previously here read *"Theme is user-persisted (`UserThemePreference` already
exists in the schema) with a `system` default"*. **The parenthesis is false as written**
(Confirmed, M2-C04-01): `UserThemePreference` is a single `bool IsDarkMode`
(`V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/UserThemePreference.cs:20`)
and **cannot represent `system`**; the service behind it has no HTTP surface at all. The
`system` default stands and is implemented — but the preference is persisted **client-side**
(`localStorage`, key `nexgen.theme`), and the SPA's preference is independent of Blazor's during
the strangler period. Server persistence needs a settings endpoint **and** a decision on the
entity: **Q-33** in [`open-questions.md`](../open-questions.md), owned by product + backend,
needed by **M3-3**. See the INV-006 amendment in
[`investigation-registry.md`](../investigation-registry.md).

### Typography

`Inter` (UI) / `JetBrains Mono` (codes, quantities, amounts). Self-hosted, `font-display: swap`.

| Token | Size / line-height | Weight | Use |
|---|---|---|---|
| `--text-xs` | 11 / 16 | 500 | table meta, badges |
| `--text-sm` | 12 / 18 | 400 | **table body, form inputs — the workhorse** |
| `--text-base` | 14 / 20 | 400 | body copy |
| `--text-lg` | 16 / 24 | 600 | section titles |
| `--text-xl` | 20 / 28 | 600 | page title |
| `--text-2xl` | 24 / 32 | 700 | dashboard KPI |

**All numeric columns use tabular figures** (`font-variant-numeric: tabular-nums`) and are
right-aligned. Document numbers and item codes use the mono face.

### Spacing, radius, elevation

4 px base scale: `4 · 8 · 12 · 16 · 24 · 32 · 48`.
Radius: `sm 4 · md 6 · lg 8 · full`. Nothing rounder — this is a data tool.
Elevation: `0` flat, `1` card (`0 1px 2px rgba(0,0,0,.06)`), `2` dropdown, `3` modal.
Dark mode uses lighter surfaces rather than shadows.

### Motion

`fast 120ms` (hover, focus), `base 180ms` (dropdown, drawer), `slow 240ms` (page
transitions), all `cubic-bezier(.2,0,0,1)`. Honour `prefers-reduced-motion`. No decorative
animation in grids.

## Application shell

```
┌───────────────────────────────────────────────────────────────────────────┐
│ HEADER 48px  [☰] Logo │ Global search (⌘K) │ FY ▾ · Tenant · 🔔 · ☾ · Avatar │
├──────────┬────────────────────────────────────────────────────────────────┤
│ SIDEBAR  │  Breadcrumb                                    [page actions]  │
│ 240px    │  Page title                                                    │
│ (rail 56)│  ┌──────────────────────────────────────────────────────────┐  │
│          │  │ Filter bar                                               │  │
│  Master  │  ├──────────────────────────────────────────────────────────┤  │
│  Sales   │  │ Content                                                  │  │
│  Purchase│  │                                                          │  │
│  …       │  └──────────────────────────────────────────────────────────┘  │
│          │  Sticky footer: [Cancel]                    [Save] [Save+New]  │
└──────────┴────────────────────────────────────────────────────────────────┘
```

- **Sidebar** keeps the two-mode idea from `NavMenu.razor` (icon rail ↔ expanded tree)
  because it is genuinely good for a 32-group menu — but rebuilt: single-level accordion,
  filtered by permissions, with a pinned "Recent" and "Favourites" section.
- **Header** carries the financial-year selector (the ERP is FY-scoped — `FinancialYearHelper.cs`),
  the tenant name, and the theme toggle.
- **⌘K command palette** replaces the existing `/instantSearch` page: jump to any of the
  150 screens, or search customers/items/documents inline. This alone will be the most
  visible "this is a new product" signal.

#### Built — `M2-C03` (2026-08-27)

`layout/shell/` composes header, sidebar, `<router-outlet>` and the command palette;
`layout/auth-layout/` and `layout/print-layout/` are the other two layout routes. Against the
description above: **Sidebar** — built exactly as described (single-level accordion,
permission-filtered via `NavFilterService`, pinned Recent/Favourites); rail mode's group
flyout is an `app-popover` anchored to the icon, a deliberate simplification of the Blazor
mini-rail's flyout-into-the-top-bar mechanic, not a port of it. **Header** — built as
described; one disclosed gap: the "tenant name" slot shows `Tenant ${tenantId}`, not a real
display name, because `GET /api/v1/me` deliberately carries no name field (`MeController.cs`'s
own doc comment, R-01) — no task has added a name-only tenant lookup yet. **⌘K palette** —
built; searches only the permission-filtered tree (a screen the caller lacks rights to is
unreachable even by exact name), fuzzy-matches via a small dependency-free subsequence scorer,
recent-first when the query is empty. The full nav data (`core/navigation/navigation.config.ts`)
was mapped by `INV-033` (`docs/kb/investigation-registry.md`) — 145 items, screen names and
routes traced to source, two likely Blazor `ScreenName` copy-paste defects found and
reproduced verbatim rather than silently corrected. Full detail, including the two-part
responsive/`axe` gap disclosed rather than hidden: `docs/kb/execution/tasks/M2-C03.md` §
Close-out and `frontend/nexgen-web/README.md` § Navigation and shell.

## Component inventory

### Navigation
`AppShell` · `Sidebar` (rail/expanded, permission-filtered) · `NavGroup` · `NavItem` ·
`Header` · `Breadcrumbs` · `CommandPalette` · `Tabs` (underline; scrollable overflow) ·
`FinancialYearSelector` · `UserMenu`

### Data display
- **`DataGrid`** — the most important component. Server-paged; sticky header; pinned
  left/right columns; resizable; per-user column show/hide (persisted to
  `UserColumnPreference`); row selection; density toggle; inline row actions; virtualised
  ≥ 500 rows; horizontal scroll never spills to the page; empty/loading/error states built
  in; CSV/Excel export via server endpoint.
- **`LineItemGrid`** — editable variant. Keyboard: `Enter` = commit + new row, `Tab` =
  next cell, `Shift+Tab` = previous, `Esc` = revert row, `Ctrl+D` = duplicate row,
  `Alt+↑/↓` = reorder, `Delete` = remove row (confirmed first if it holds data). Per-row
  validation badge. Running totals in a sticky footer row.
- `DetailPanel` · `KeyValueList` · `StatusBadge` · `Tag` · `Avatar` · `Timeline`
  (approval history) · `AttachmentList` · `AuditTrail`

#### Built — the core, M2-C05-01 (2026-08-25)

`DataGridComponent<TRow>` and `DataGridQueryState` live in
`frontend/nexgen-web/src/app/shared/components/data-grid/`, standalone and `OnPush`, wrapping
PrimeNG's `p-table` in `lazy` mode per [ADR-007](../decisions/ADR-007-angular-stack.md).
Exported from the `shared/components` barrel; **nothing imports that barrel eagerly**, so the
initial bundle is unchanged at 571.20 kB raw (R-69's lesson, held).

What the core actually does, against the list above:

| Requirement | State |
|---|---|
| Server-paged | **Built.** `[lazy]="true"`, `[lazyLoadOnInit]="false"`, `[paginator]="false"`; no `pSortableColumn` and no PrimeNG filter directive anywhere, so PrimeNG never holds a second opinion about the page number. A spec asserts a sort produces a *request*, not a local reorder. |
| Sticky header | **Built.** The header row is `position: sticky` and stays outside the virtual window. |
| Pinned left/right columns | **Built.** `frozen: 'left' \| 'right'` on the column model. |
| Resizable | **Built.** Pointer drag, plus `←`/`→` on a focused `role="separator"` handle carrying `aria-valuenow`/`min`/`max`. Clamped to 48–1200 px. |
| Per-user column show/hide | **Seam only — M2-C05-02.** The two-way `columnVisibility` input is typed and honoured; nothing persists it yet. |
| Row selection | **Built.** `none` / `single` / `multiple`; the header checkbox selects **the current page only** and shows an explicit indeterminate state; selection is keyed by `getRowId` and survives a refetch. Owned by the caller as a two-way binding. |
| Density toggle | **Built.** `comfortable` 36 px / `compact` 30 px, resolved from `--row-height-*`. |
| Inline row actions | **Built.** A `#rowActions` template slot. |
| Virtualised ≥ 500 rows | **Built and measured.** 10,000 rows → 35 rendered `<tr>`, 16.7 ms median frame. See [KB-050 §Performance targets](react-architecture.md#performance-targets). |
| Horizontal scroll contained | **Built.** `overflow-x: auto` on the grid's own viewport; the page body never scrolls sideways. |
| Empty / loading / error built in | **Seams only — M2-C05-03.** `#empty`, `#error` and `#toolbar` templates are typed and reachable; the default placeholder is `role="status"`, never an empty `<tbody>`; the `ProblemDetails` object reaches the error slot untouched. First load is a shape-matched skeleton, and a refetch keeps the previous page under a 2 px progress bar. |
| CSV/Excel export via server endpoint | **Not built — M2-C05-03.** The `#toolbar` slot is where it lands. |

Accessibility: `role="grid"` with one grid-level tab stop and a roving `tabindex`;
`aria-rowcount`/`aria-colcount` from the **server** total; `aria-rowindex` absolute across pages;
`aria-sort` on every sortable header; the full arrow / `Home` / `End` / `Ctrl+Home` / `Ctrl+End` /
`PageUp` / `PageDown` model, with focus restored to the same cell coordinates after a refetch;
`aria-live="polite"` announcement of the visible range. `axe` reports no critical violation on a
populated or an empty grid in either theme.

Two deviations, each with its reason:

- **The two selection checkboxes are native `<input type="checkbox">`, not `app-checkbox`.**
  `app-checkbox` (M2-C04-02) exposes no indeterminate state, and "some rows on this page are
  selected" has to be distinguishable from "none are". Adding the input to `app-checkbox` would
  edit a file this task does not own. The filter row and the page-size selector **do** use
  M2-C04-02's controls, so there is no second input vocabulary.
- **`DataGridHeaderComponent`'s selector is `tr[appDataGridHeader]`**, an attribute rather than
  an `app-` element. A `<thead>` may contain only `<tr>`; an element between them is invalid
  table markup that the browser hoists out of the table. The `component-selector` lint rule is
  waived on that one line and nowhere else.

#### Built — LineItemGrid, M2-C07 (2026-08-27)

`LineItemGridComponent<TLine>` and its supporting modules live in
`frontend/nexgen-web/src/app/shared/components/line-item-grid/`, standalone and `OnPush`,
built on PrimeNG's editable `p-table` reusing `DataGrid`'s column model and cell-navigation
primitives, per [ADR-007 §Key rationales, Addendum (Q-83)](../decisions/ADR-007-angular-stack.md#addendum--the-lineitemgrid-see-below-pointer-resolved-q-83-2026-08-27).

| Requirement | State |
|---|---|
| Full keyboard model | **Built.** `Enter`/`Tab`/`Shift+Tab`/`Esc`/`Ctrl+D`/`Alt+↑↓`/arrows/`Delete`, each covered by a test. `Tab`/`Shift+Tab` are native DOM tab order, not intercepted — a `readonly` column renders no focusable element at all, so the browser already skips it. |
| Row isolation at 200 rows | **Built and measured (isolation, not latency — see below).** `line-item-grid.render-performance.spec.ts` proves typing in one row re-renders only that row. |
| Decimal-safe cells | **Built.** Every numeric cell parses/formats through `shared/utils/decimal` directly, not through `app-currency-input`/`app-number-input` — see this task's Close-out for why (`DECIMAL_PORT` has no real implementation anywhere in the app yet). |
| `rowEvent` domain-event contract | **Built.** A discriminated union with a `respond(patch)` callback per domain event; the grid busies the row and applies only what the caller returns. |
| Row-error gutter | **Built.** Icon + text, `aria-describedby`, `aria-live`. |
| Sticky footer | **Built as a pure slot.** The grid computes no aggregate — enforced by `line-item-grid.no-business-logic.spec.ts`. |
| Clipboard paste | **Built.** Tab/newline-separated grid parsed against the target columns, previewed in a modal, decimal-safe. |
| Responsive (<768px) | **Built.** Renders a stacked read-only list; editing is not offered below the breakpoint. |
| 200-row typing latency < 50 ms | **Unknown — not measured live.** See [KB-050 §Performance targets](react-architecture.md#performance-targets) and [INV-061](../investigation-registry.md). |

**One selector deviation, same reasoning as `DataGridHeaderComponent` above:**
`LineItemRowComponent`'s selector is `tr[appLineItemRow]` for the identical reason — a `<tr>`
is the only legal child of `<tbody>`, and the `component-selector` lint rule is waived on that
one line, following the existing precedent rather than inventing a new one.

`axe` reports no critical violation on a populated grid (with a row error) or the one-row
empty state, in either theme.

The URL is the grid's state in route-bound mode — `?page=3&size=50&sort=name:desc&code=C1`,
which round-trips and survives back/forward. A detached mode holds the same signals and never
writes the URL, which is what **M2-C06**'s dialog needs.

### Forms
`FormLayout` (1/2/3-column responsive) · `FormSection` · `FormField` (label, hint, error,
required marker) · `TextInput` · `NumberInput` (tabular, step, min/max, decimal places
from server settings) · `CurrencyInput` · `Select` · `MultiSelect` · `Combobox`
(async typeahead — the `SearchCustomers`/`SearchItems` pattern) · `DatePicker` ·
`DateRangePicker` · `Checkbox` · `RadioGroup` · `Switch` · `Textarea` · `FileUpload` ·
`AmountOrPercentInput` (the recurring `…AmtOrPer` toggle from `ICalculationDocument`)

Rules: label **above** the field (scannable in dense forms), errors inline below, required
marked with `*` plus `aria-required`, disabled ≠ readonly (readonly stays copyable),
autofocus the first field on create.


#### Built — M2-C04-02 (2026-08-23)

All 17 are implemented in `frontend/nexgen-web/src/app/shared/components/form/` as standalone
`OnPush` components over PrimeNG 22, each a `ControlValueAccessor`, each rendering its
validation through **`app-form-field`** — the single display mechanism. Selector prefix `app-`,
kebab-case; `FormLayout` → `app-form-layout`, and so on. Two names were added to the inventory
above and are not deviations from it, only from its brevity: `app-form-layout` also owns the
loading skeleton, the form-level error alert and the sticky footer slot.

Deviations, each with its reason:

- **`app-form-layout` does not render the `<form [formGroup]>` element.** The screen does, and
  puts the layout inside it. Angular resolves a projected `formControlName` through the
  *declaration* injector tree, so a `FormGroupDirective` inside the layout would be invisible to
  the projected fields and every one of them would throw `NG01050`. The typed group is still an
  input; the layout reads its state.
- **The numeric trio ships behind a `TODO(M2-C10)`.** `app-number-input`, `app-currency-input`
  and `app-amount-or-percent-input` hold branded `Money`/`Qty` values and parse only through an
  injected `DECIMAL_PORT`, which **M2-C10 has not yet implemented**. No local parsing was added:
  a `parseFloat` there is the defect M2-C10 exists to prevent.
- **The form-level alert is a minimal local placeholder**, marked `TODO(M2-C04-03)`, to be
  replaced by the shared `InlineAlert` rather than duplicated.
- **Date format defaults to ISO** through an injectable `DATE_FORMAT` token — no endpoint
  exposes the tenant's format and `Companydetails` carries no such column (Q-75).
- **`app-file-upload`'s loading row is caller-driven.** The control performs no transport —
  `customUpload` is on and no `url` is set, because M2-B06 owns the upload endpoints — so it
  cannot know when a transfer is in flight. The screen that wires those endpoints sets
  `[loading]`; the control then renders `Uploading…`, suppresses the empty row and disables the
  chooser. The same shape as `app-select`, whose `loading` is likewise set by its caller.

Keyboard model, split by what is **asserted** and what the review pass carries — the split is
the honest part, and `form/README.md` § Keyboard model holds the per-control detail and the
spec file that proves each row.

**Asserted by `userEvent` specs (2026-08-23):** text and textarea are native, with trim on
commit; `app-select` opens on `ArrowDown`, moves with the arrows, jumps with `Home`/`End`,
selects with `Enter` and restores the previous value on `Esc`; `app-multi-select` adds
`Enter`-toggles-and-stays-open and `Backspace`-removes-the-most-recent-chip (PrimeNG 22.1.0's
`MultiSelect` has no `Backspace` case, so the control adds one, guarded so the filter box keeps
its own `Backspace`); `app-combobox` moves with the arrows, selects with `Enter` and closes with
`Esc`, while `Home`/`End` move the **caret** — it is a text entry, and PrimeNG's `AutoComplete`
treats them that way; `app-radio-group` is a single tab stop and `Space` selects the focused
option; `app-checkbox` and `app-switch` toggle with `Space`; both date controls accept typed
input and open the calendar on focus — the calendar is never the only entry path; the
file-upload choose control is a real button that both `Enter` and `Space` open, and every file
row's remove is keyboard-reachable.

**Not asserted, and carried by the keyboard pass required at review** — three, each with a
measured reason rather than an excuse: radio-group **arrow movement** (user-agent behaviour that
jsdom does not synthesise); the date-picker **calendar grid** keys — arrows by day,
`PageUp`/`PageDown` by month, `Esc` — because PrimeNG 22.1.0 reads the legacy `event.which` /
`event.keyCode` while `@testing-library/user-event` v14 sends `which === 0` under jsdom (probed
2026-08-23), so every handler falls through whatever key is sent; and **masked typing** in
`p-inputnumber`.

**`app-switch` is for immediate-effect toggles only; a field saved on submit uses
`app-checkbox`.** Stated here as well as in the component docs, because across ~140 screens the
two will otherwise be used interchangeably.

**`disabled` ≠ `readonly`, and it is asserted control by control** in `form/readonly.spec.ts`
(2026-08-23). A readonly control keeps its value focusable, in the tab order and copyable; only
the editing affordance goes. Where PrimeNG 22 offers `readonly` / `readonlyInput` the wrapper
uses it; where the surface has none the distinction is drawn explicitly — the radio group keeps
its buttons enabled under `aria-readonly` and cancels the click, the amount-or-percent **mode**
renders as a label instead of a `p-selectbutton`, and `app-file-upload` drops the chooser and
the per-file Remove while keeping the attachment list as selectable text. Routing `readonly`
into `[disabled]` is the trap this replaced: `primeng-select.mjs` computes
`tabindex = !$disabled() ? tabindex() : -1`, so a "readonly" select left the tab order entirely.
One measured PrimeNG gap is worked around rather than accepted — `MultiSelect.onKeyDown` and
`onOptionSelect` consult `$disabled()` but not `readonly`, so `app-multi-select` cancels
keystrokes in the capture phase while readonly, letting `Tab` and clipboard chords through.

Runtime `axe` scan over every control in both themes: `form/a11y.spec.ts`, zero critical
violations observed 2026-08-23. jsdom applies no stylesheet, so `color-contrast` cannot run
there; contrast is covered by computation in `core/theme/contrast.spec.ts`.

### Overlays
`Modal` (sm/md/lg/full) · `Drawer` (right, resizable — for record detail without losing
list context) · `ConfirmDialog` (with optional **required reason** — BR-SO-003) ·
`RecordPickerDialog` (the `DetailsModal.razor` replacement: searchable, multi-select,
column-configurable picker over an upstream document) · `QuickCreateDialog`
(`MasterModal.razor` replacement) · `Popover` · `Tooltip` · `ContextMenu`

### Feedback
`Toast` (success 4 s, error sticky with dismiss) · `InlineAlert` (info/warn/error/success) ·
`BusyOverlay` · `Skeleton` · `ProgressBar` · `EmptyState` (illustration + explanation +
primary action) · `ErrorState` (message + `traceId` + retry) · `PermissionDeniedState`

**Built by `M2-C04-03` (2026-08-23), Angular + PrimeNG, in
`src/app/shared/components/overlay/` and `.../feedback/`.** All 14 shipped:
`app-modal` (`p-dialog`), `app-drawer` (`p-drawer`), `app-confirm-dialog`
(`p-confirmdialog` + `ConfirmationService`), `app-popover` (`p-popover`), `[appTooltip]`
(`[pTooltip]`), `app-context-menu` (`p-contextmenu`), `ToastService` (`p-toast`),
`app-inline-alert` (`p-message`), `app-busy-overlay` (`p-blockui` + `p-progressspinner`),
`app-skeleton` / `-table` / `-form` (`p-skeleton`), `app-progress-bar` (`p-progressbar`),
`app-empty-state`, `app-error-state`, `app-permission-denied-state`.
`RecordPickerDialog` and `QuickCreateDialog` stay with `M2-C06` — they are *contents* placed
inside this layer's modal and drawer.

**`RecordPickerDialog` built by `M2-C06` (2026-08-26)**, in
`src/app/shared/components/record-picker-dialog/`: `app-record-picker-dialog`, generic over
`TRow`, composing `app-modal` (this layer) and `app-data-grid` (M2-C05-01) — no second table
and no second component library. It takes a caller-supplied `fetchPage` function rather than
a resource, so one component serves the 33 `DetailsModal.razor` call sites; its query state
is `DataGridQueryState` in **detached** mode, so it never writes to the page URL behind it.
Selections are returned **in the order the user ticked them**, guaranteed by an
insertion-ordered `Map` in `record-selection.ts` and asserted by `selection-order.spec.ts` —
this is not cosmetic: 34 Blazor call sites append the returned rows in iteration order and
48 renumber afterwards, so the ticking sequence is the document's line order (INV-054).

`QuickCreateDialog` remains unbuilt; per INV-054, `MasterModal.razor` is plain modal chrome
and maps to `app-modal`, not to a picker.

**Four deliberate divergences from `DetailsModal.razor`, each recorded rather than silent:**

| Divergence | Old behaviour | Why |
|---|---|---|
| **`Esc` cancels the dialog** | `data-bs-keyboard="false"` and a `static` backdrop (`DetailsModal.razor:10-11`) — `Esc` did nothing | The accessibility commitments below require a modal to be escapable. A picker is a non-destructive selection dialog. |
| **Select-all is page-scoped and labelled with its count** ("Select all 25 on this page") | Select-all covered the client-side *filtered* set, which was the whole candidate set (`:181-198`) | With server paging "all" is ambiguous; silently selecting thousands of unseen rows is a data-integrity hazard. |
| **The button says Export, not "Print"** | Labelled "Print", called `ExcelExportService.ExportPendingListToExcel` and downloaded an `.xlsx` (`:241-244`) | It never printed. A mislabel corrected, not a feature changed. Export stays server-side per ADR-005; only the base64-through-JS-interop hop is dropped, in favour of a blob. |
| **Confirm is disabled while nothing is selected**, with an accessible explanation | Update was always enabled (`:90`); the only guard was unreachable (`:156-168`) | The reachable case — an empty selection — was never handled. The Blazor defect is recorded in KB-015 and **not** fixed. |

The hardcoded domain highlighting at `:218-230` is replaced by a generic, caller-supplied
`getCellState(row, field)` returning a `tone` **and a required `label`** — a shared component
that knows ERP field names is domain leakage into presentation, and per the status vocabulary
below a tone may never travel without words. A directory scan in
`record-picker-dialog.component.spec.ts` fails the build if a domain field name reappears.

**Confirmed keyboard model** (asserted by test, not inherited from PrimeNG's defaults): the
modal, drawer and confirm dialog move focus in on open, trap it, close on `Esc`, return focus
to the exact invoking element and lock background scroll; the confirm dialog maps `Esc`, the
backdrop and the close icon to **cancel** and disables Confirm until a required reason is
non-empty after trim; the drawer resize handle is a focusable `role="separator"` driven by
`←`/`→`/`Home`/`End`; the tooltip opens on focus; the context menu has a visible trigger and
answers `Shift+F10`. Runtime `axe` scan over every overlay while open and every feedback
component, in both themes: `overlay/a11y.spec.ts` and `feedback/a11y.spec.ts`, zero critical
violations observed 2026-08-23. jsdom applies no stylesheet, so `color-contrast` cannot run
there; contrast is `core/theme/contrast.spec.ts`.

**Deviations, each with its reason:**

| Deviation | Reason |
|---|---|
| `app-confirm-dialog` **disables** Confirm on an empty required reason, where `BsModal.razor:76-93` allows the click and answers with a toastr warning | Specified by the task. Same outcome, state visible before the click; the reason field is marked required and carries its own error so a screen-reader user is not left at an unexplained disabled button |
| Toast announcement forced to `aria-live="polite"` through a PrimeNG pass-through | PrimeNG 22.1.0 hard-codes `role="alert" aria-live="assertive"` on every toast item; a success toast that interrupts a screen-reader user is worse than one heard a moment later |
| `app-inline-alert` raises the shared `p-message` live region to `assertive` for errors only, instead of nesting its own | Two nested live regions double-announce |
| `app-confirm-dialog` moves focus into the dialog itself, from `afterEveryRender` | Measured PrimeNG 22.1.2: `p-confirmdialog` sets `[focusOnShow]="false"` on the dialog it renders and relies on `pAutoFocus` on its own accept/reject buttons, which a custom footer replaces, so focus never enters and the focus trap has nothing to hold; it exposes no `(onShow)`, and `p-dialog` moves its wrapper to `document.body` as the transition starts, blurring anything focused earlier |
| `app-drawer` implements `Esc` itself and drives close from `visible` rather than `(onHide)` | Measured PrimeNG 22.1.0 defects: `Drawer.onKeyDown` answers `Escape` with `hide(false)`, which never clears `visible`; `onHide` is not emitted for a programmatic close; `unbindDocumentEscapeListener()` calls itself |
| `app-drawer` overwrites the drawer root's `role="complementary"` with `role="dialog"` + `aria-modal` + `aria-label` | PrimeNG hard-codes `complementary`; a modal record-detail overlay is a dialog and needs a name |
| `aria-level` / `aria-setsize` / `aria-posinset` cleared from `p-contextmenu` items, and `aria-level` from `p-progressbar`, through pass-throughs | PrimeNG emits ARIA attributes those roles do not allow; axe rates both critical |
| The overlay layer duplicates `form/jsdom-overlay-support.ts` rather than promoting it to a global `setupFiles` entry | `form/**` is outside this task's scope to edit and `setupFiles` is a build-configuration change; recorded in KB-060 |

**Two token gaps reported, not filled** (`M2-C04-01` owns `tokens.css` and the preset):
there is no z-index/stacking or scrim token, so overlays rely on PrimeNG's own layering; and
`theme.preset.ts` has no `mask` key, so the dialog and drawer backdrop keeps Aura's
untokenised default. Recorded in KB-004.

## State patterns

| State | Presentation |
|---|---|
| **Loading (first)** | Skeleton matching final layout — never a spinner on a blank page |
| **Loading (refetch)** | Keep previous data, subtle top progress bar |
| **Empty (no data yet)** | Illustration + "No sales orders yet" + `[New Sales Order]` |
| **Empty (filtered out)** | "No results for these filters" + `[Clear filters]` |
| **Error** | `ErrorState` with the server message, `traceId`, and Retry |
| **Permission denied** | Inline panel explaining which screen right is missing |
| **Saving** | Disable the footer actions, show inline spinner on the button; never block the page |
| **Dirty form** | Footer shows "Unsaved changes"; navigation blocked via `useBlocker` (replaces `UnsavedChangesModal.razor`) |

**Implemented for `DataGrid` on 2026-08-26 by `M2-C05-03`** — every row above except *Saving* and
*Dirty form*, which belong to the form layer. The states compose M2-C04-03's primitives
(`shared/components/feedback/`); nothing under `shared/components/data-grid/` re-implements an
`EmptyState` or an `ErrorState`. What each row became, concretely:

- **Loading (first)** — `app-data-grid-skeleton`: `min(pageSize, 12)` rows whose cells carry the
  _resolved_ column widths, `aria-busy="true"` on the grid, and one visually-hidden
  `role="status"` announcing _"Loading results"_ — not one per bar.
- **Loading (refetch)** — the previous rows stay and a 2 px indeterminate `app-progress-bar` sits
  above the header. The table is neither dimmed nor disabled and focus is not moved.
- **Empty (no data yet) / Empty (filtered out)** — chosen by `DataGridQueryState.hasActiveFilters`,
  a `computed` over the **committed** filter set, **never by guessing**. Reading the filter _draft_
  instead would flip the state mid-keystroke.
- **Error** — `403` renders the permission-denied panel inline (no redirect), `409` renders the
  server's `title` **verbatim** in an inline alert, everything else renders `app-error-state` with
  the message, the `detail`, the copyable `traceId` and Retry.

**A deliberate behaviour change, recorded rather than assumed.** The Blazor generic picker renders
a _single, undifferentiated_ spanning row reading **"No data found."** for both empty cases
(`V.SMART/V.SMART.Shared/Components/DetailsModal.razor:75-82`, Confirmed, re-verified 2026-08-26).
That conflates _"nothing exists yet"_ with _"your filters exclude everything"_ — two situations that
need different words and different actions — so this table's split replaces it on purpose. The
superseded evidence is kept here so the change reads as a decision, not a regression.

**Two gaps this implementation could not close, both outside its scope.** The correlation id is read
from the problem **body**'s `traceId` rather than the `X-Correlation-Id` header, and an exported file
saves under a client-side fallback name, because `V.SMART/V.SMART.Api/Program.cs:165-171` exposes no
CORS response headers (**R-79**, **Q-96**). And there is no HTTP error interceptor to normalise
`ProblemDetails` — M2-C02 is Blocked — so the grid normalises once, locally (**Q-94**).

**Export is not a state, but it lives here too.** `app-data-grid-toolbar` asks the server for the
bytes and saves them; the client builds no spreadsheet, CSV or PDF (ADR-005), and
`no-client-file-generation.spec.ts` fails the build if such a dependency is ever added. Only Excel is
offered, because `xlsx` is the only format the export endpoint accepts
(`V.SMART/V.SMART.Api/Controllers/CurrencyExcelController.cs:48`) — see **Q-95**.

**The document-editor skeleton is built — `M2-C08-01` (2026-08-28).** Principle 5's *"same page
skeleton on all 150 screens"* is now structural for the document half of the app:
`<app-document-editor>` composes page header → header form → `LineItemGrid` → totals slot → side
region → sticky command bar, and a feature module supplies a `DocumentEditorConfig`, not a layout.
Responsive behaviour follows the table below: totals beside the lines at ≥1440, below them under
it, the side region as an accordion below 1024, and the whole editor **read-only below 768**.

**The blocking overlay is deliberately gone, and this is the record of that decision.** The
*Saving* row above — *"disable the footer actions, show inline spinner on the button; never block
the page"* — is what the shell implements, replacing `ProcessingOverlay.razor`, which covered the
whole screen while a save was in flight. Two things make this a decision rather than a regression:
the legacy behaviour is **already inconsistent** — `MfgInvUpsert.razor` renders no
`ProcessingOverlay` at all while the other four sampled screens do (INV-065) — and the risk the
overlay guarded against (a second submit) is handled by disabling the actions, which is what the
overlay was really doing. A spec pins it: *"saving disables the footer actions and shows an inline
spinner; the page is **not** overlaid"*.

**The *Dirty form* row's mechanism is settled too, and it is not `app-confirm-dialog`.** The
prompt has **three** outcomes — Save / Discard / Stay — and `app-confirm-dialog` models two by
contract (INV-006's M2-C04-03 amendment), so the editor composes `app-modal` and the route carries
a functional `CanDeactivateFn` (`unsavedChangesGuard`). `beforeunload` is registered **only while
dirty** and removed on destroy. This is a **behaviour improvement, not preservation**: the existing
guard is a single global JS boolean (`wwwroot/js/navigationGuard.js:1-39`) read by exactly one call
site (`SmartBackButton.razor:64`), so navigating by the menu, the browser back button or a typed
URL is unguarded today, and no `beforeunload` listener exists anywhere in it. (**R-72** — this
document's own "via `useBlocker`" wording in the table above is the stale React remnant, still
unfixed and still not a specification.)

## Status vocabulary

One badge component, one shared vocabulary across all documents:

| Status | Colour | Icon |
|---|---|---|
| Draft | info | pencil |
| Pending approval | warning | clock |
| Approved | success | check |
| Rejected | danger | x |
| Partially executed | warning | half-circle |
| Completed / Closed | success | check-circle |
| Short-closed | secondary | minus-circle |
| Cancelled | danger | ban |
| On hold | warning | pause |

**Never colour alone** — always icon + text, for accessibility and for print.

## Dashboard layout

12-column responsive grid, 8 px gutters. Card sizes: KPI (3 col), chart (6 col), table
(12 col). Cards declare their own loading/empty/error state. User-reorderable, persisted
per user (extends the existing `UserPreference` idea).

## Responsive behaviour

| Breakpoint | Behaviour |
|---|---|
| `≥1440` (primary target) | full sidebar, 3-column forms, all grid columns |
| `1024–1439` | sidebar collapses to rail, 2-column forms |
| `768–1023` | sidebar becomes an overlay drawer, 1-column forms, grid drops low-priority columns |
| `<768` | **read + approve only.** Document editing is not supported on phone. Shop-floor screens (Daily Production Log, Stock Position, Approvals) get dedicated touch-first layouts |

The existing MAUI app already targets tablets for shop-floor use; the Angular app should
serve those flows through responsive routes rather than a second codebase.

## Accessibility commitments (WCAG 2.2 AA)

- Every interactive element keyboard-reachable, with a visible 2 px focus ring.
- Grids implement the ARIA grid pattern with arrow-key navigation.
- Form controls have programmatic labels; errors linked via `aria-describedby` and
  announced by `aria-live="polite"`.
- Modals trap focus and restore it on close; `Esc` closes.
- Contrast ≥ 4.5:1 for text, ≥ 3:1 for UI boundaries, in **both** themes.
- Motion respects `prefers-reduced-motion`.
- Zoom to 200% without loss of function.
- Automated axe checks in CI plus a manual keyboard pass per screen at review.

## Do not

- Mix two component libraries (the current app mixes MudBlazor and Bootstrap — that is
  part of what makes it feel dated).
- Put primary actions in a hidden overflow menu.
- Use modals for anything that needs the list behind it — use a drawer.
- Animate table rows.
- Encode meaning in colour alone.
- Recreate 440 routes one-for-one; consolidate create/update/details.
