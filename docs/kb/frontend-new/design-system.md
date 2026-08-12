---
doc_id: KB-051
title: Proposed Design System
module: frontend-new
source_files: []
status: proposal
confidence: n/a
last_verified: 2026-08-12
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

### Colour

Semantic tokens only — components never reference a raw hue.

| Token | Light | Dark | Use |
|---|---|---|---|
| `--bg-canvas` | `#F7F8FA` | `#0E1116` | page background |
| `--bg-surface` | `#FFFFFF` | `#161B22` | cards, panels, grid |
| `--bg-surface-raised` | `#FFFFFF` | `#1C2128` | modals, popovers |
| `--bg-subtle` | `#EEF1F5` | `#21262D` | table header, zebra, disabled |
| `--border` | `#D8DEE6` | `#2D333B` | dividers, inputs |
| `--border-strong` | `#B4BDC8` | `#404752` | focused/active borders |
| `--text-primary` | `#12181F` | `#E6EDF3` | body |
| `--text-secondary` | `#5A6572` | `#9BA6B2` | labels, hints |
| `--text-disabled` | `#98A2AE` | `#6B7684` | |
| `--accent` | `#2563EB` | `#4C8DFF` | primary actions, links, focus |
| `--accent-subtle` | `#E8F0FE` | `#16243D` | selected row, active nav |
| `--success` | `#15803D` | `#3FB950` | posted, approved, in stock |
| `--warning` | `#B45309` | `#D29922` | pending, partial, low stock |
| `--danger` | `#B91C1C` | `#F85149` | cancelled, rejected, over-issue |
| `--info` | `#0E7490` | `#39C5CF` | draft, informational |
| `--focus-ring` | `#2563EB` @ 40% | `#4C8DFF` @ 45% | 2 px ring, 2 px offset |

Both themes are authored as first-class palettes, not a filter over one another.
Theme is user-persisted (`UserThemePreference` already exists in the schema) with a
`system` default.

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
  `Alt+↑/↓` = reorder. Per-row validation badge. Running totals in a sticky footer row.
- `DetailPanel` · `KeyValueList` · `StatusBadge` · `Tag` · `Avatar` · `Timeline`
  (approval history) · `AttachmentList` · `AuditTrail`

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

The existing MAUI app already targets tablets for shop-floor use; the React app should
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
