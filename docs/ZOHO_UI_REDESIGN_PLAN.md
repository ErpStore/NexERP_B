# Zoho-Inspired UI Redesign — Execution Plan

## Goal
Replace the current V.SMART ERP UI (Bootstrap cards with dark-red headers, gradient badges, shadow-heavy layout) with a clean, professional design inspired by Zoho CRM / Zoho Books — so the application looks and feels like a distinct, modern product.

---

## Design Language Reference

### What makes Zoho's UI distinctive

| Element | Current V.SMART | Target Zoho-style |
|---|---|---|
| Page background | White card on white | Light gray `#F4F5F7` |
| Page header | Dark-red card header | Clean white bar + module icon |
| Breadcrumb | Inside dark header | Small gray trail above title |
| Table | `QuickGrid` + Bootstrap `.table-bordered` | Plain HTML `<table class="zh-table">` |
| Table header | Default Bootstrap | `#EEF0F4` bg, uppercase 11px labels |
| Status badges | Gradient rounded-pill Bootstrap badges | Flat colored pills (`.zh-chip-*`) |
| Action buttons | `btn-outline-*` with text or icon+text | Icon-only 28×28 icon buttons |
| Pagination | Centered Bootstrap buttons | Left = record count, Right = nav buttons |
| Toolbar | Bootstrap grid row of buttons | White bar with left/right groups |
| Typography | Roboto (MudBlazor default) | Inter / Segoe UI |
| Button style | Outline Bootstrap | Solid blue primary, ghost secondary |

### Zoho Color Tokens (all defined in `zoho-theme.css`)

```
--zh-primary:     #1B7BFF   (Zoho blue)
--zh-bg:          #F4F5F7   (page background)
--zh-surface:     #FFFFFF   (cards, bars)
--zh-border:      #DEE2E6
--zh-table-header:#EEF0F4
--zh-table-hover: #F0F5FF
```

Status chips: flat background + matching dark text color (no gradients, no shadows).

---

## CSS Architecture

### File: `V.SMART.Shared/wwwroot/css/zoho-theme.css`
- All Zoho classes use the `zh-` prefix to avoid conflicts with Bootstrap and MudBlazor
- Loaded globally via `App.razor` — available on every page
- Self-contained: any page can use `zh-` classes without adding any page-level `<style>` block

### Class Map

```
zh-page                    → full page flex container
zh-module-header           → top white bar (title + breadcrumb + action buttons)
zh-module-icon             → colored icon square (top-left)
zh-module-title            → h1 inside the header
zh-breadcrumb              → breadcrumb trail
zh-toolbar                 → second white bar (filters + selects)
zh-toolbar-left/right      → flex groups inside toolbar
zh-btn, zh-btn-primary     → solid blue button
zh-btn-secondary           → white/border button
zh-btn-ghost               → transparent/border button
zh-select                  → styled <select>
zh-table-wrapper           → scrollable table container
zh-table                   → the <table> itself
zh-col-actions             → 96px centered actions column
zh-id-link                 → clickable doc-number style
zh-chip, zh-chip-*         → flat status pills (success/warning/danger/info/purple/neutral)
zh-action-group            → flex row of action icon buttons
zh-action-btn, zh-action-* → 28×28 icon button (edit/view/delete)
zh-pagination              → bottom bar
zh-pagination-info         → "Showing X–Y of Z records"
zh-pagination-nav          → nav buttons + page indicator
zh-page-btn                → 30×30 pagination button
zh-page-indicator          → "3 / 15" text
zh-empty-state             → centered empty-state panel
```

---

## Completed Work

- [x] **`zoho-theme.css`** — created at `V.SMART.Shared/wwwroot/css/zoho-theme.css`
- [x] **`App.razor`** — CSS linked globally
- [x] **`EnquirySalesList.razor`** — fully redesigned as proof-of-concept screen
  - Removed: `<PageHeader>`, Bootstrap card/container wrapper, QuickGrid, gradient badges, old pagination bar, inline `<style>`
  - Added: `zh-page`, `zh-module-header`, `zh-toolbar`, `zh-table`, flat `zh-chip-*` status badges, `zh-pagination`
  - Logic: `@code` block untouched — zero business logic change

---

## Execution Plan — Converting All Remaining Screens

### Phase 1 — Stabilise the Pattern (Week 1)
- Review `EnquirySalesList.razor` in the browser; adjust CSS tokens as needed
- Confirm font loads correctly (add Inter from Google Fonts in `App.razor` if needed)
- Fix any visual issues; update `zoho-theme.css` — every other screen will benefit automatically

### Phase 2 — List Screens (Week 2–3)
All List pages follow the same pattern:
1. Replace outer `container-fluid` card with `<div class="zh-page">`
2. Replace `<PageHeader>` with `zh-module-header` block
3. Replace filter `<div class="row g-2">` with `zh-toolbar`
4. Replace `<QuickGrid>` with `<table class="zh-table">`
5. Replace gradient `.badge` elements with `.zh-chip zh-chip-*`
6. Replace Bootstrap pagination with `zh-pagination`

Use the Copilot prompt at `.github/prompts/convert-to-zoho-ui.prompt.md` to automate each screen.

**Priority order (high-traffic modules first):**

| # | Module | List Page |
|---|--------|-----------|
| 1 | Sales PO | `MfgPOList.razor` |
| 2 | Mfg DC | `MfgDcList.razor` |
| 3 | Mfg Invoice | `MfgInvList.razor` |
| 4 | Mfg Quotation | `MfgQuoteList.razor` |
| 5 | Performa Invoice | `PerformaList.razor` |
| 6 | Purchase Order | Purchase list pages |
| 7 | GRN | GRN list pages |
| 8 | Inventory | Stock list pages |
| 9 | HR | HR list pages |
| 10 | All remaining | ... |

### Phase 3 — Form / Upsert Screens (Week 4–5)
Upsert pages need a second CSS component set — create `zh-form-*` classes:
- `zh-form-page` — same outer shell
- `zh-form-header` — title bar with save/cancel buttons on right
- `zh-form-section` — white panel with section title
- `zh-form-grid` — responsive field grid (2–3 cols on desktop, 1 on mobile)
- `zh-form-label` — uppercase 11px label above each field
- `zh-form-actions` — sticky bottom bar with Save / Cancel

### Phase 4 — Detail / View Screens (Week 6)
- `zh-detail-header` — key document info at top (doc no, date, customer, status)
- `zh-detail-section` — white panel for each section
- `zh-detail-table` — line-items table (same `zh-table` but read-only)
- `zh-detail-meta` — created/modified audit line

### Phase 5 — Global Layout Update (Week 7)
- Update `MainLayout.razor` and `NavMenu.razor` to use Zoho-style sidebar colors
  - Sidebar: `#1F2937` background, white text, grouped nav items
  - AppBar: white, no elevation, subtle border-bottom
  - Mini-sidebar icons: white on dark background

### Phase 6 — Polish (Week 8)
- Dark mode support (`[data-theme="dark"]` overrides for `zh-*` tokens)
- Print stylesheet tweaks
- Accessibility: ensure all interactive elements have visible focus rings
- Final cross-browser / mobile test

---

## Rules for Every Screen Conversion

1. **Never change `@code` blocks** — only the HTML/Razor markup changes
2. **Never delete `@inject` lines** — they are required by the business logic
3. **Never change route `@page` directives**
4. **Never change component names** (`MasterModal`, `BsModal`, `InstantSearch`, etc.) — only their parent container styling
5. **Use `zh-chip-success/warning/danger/info/purple/neutral`** for all status indicators — no more Bootstrap `.badge`
6. **Use `zh-action-btn`** (icon-only) for row actions — no text labels
7. **Keep `ProcessingOverlay` and `RedirectToLogin`** at the top — they are outside the main markup
8. **Remove all inline `<style>` blocks** — move any page-specific overrides to `zoho-theme.css`

---

## Quick Reference — Status Chip Mapping

| Business state | Old class | New class |
|---|---|---|
| Completed / Active / Approved | `badge text-bg-success` | `zh-chip zh-chip-success` |
| Pending / In Progress | `badge text-bg-warning` | `zh-chip zh-chip-warning` |
| Cancelled / Rejected / Error | `badge text-bg-danger` | `zh-chip zh-chip-danger` |
| Info / View-only state | `badge text-bg-info` | `zh-chip zh-chip-info` |
| Manufacturing type | `badge text-bg-primary` | `zh-chip zh-chip-purple` |
| Labour type | `badge text-bg-info` | `zh-chip zh-chip-info` |
| Short Closed / Neutral | `badge text-bg-secondary` | `zh-chip zh-chip-neutral` |
| Overdue alert | `badge text-bg-danger` | `zh-chip zh-chip-danger` |
