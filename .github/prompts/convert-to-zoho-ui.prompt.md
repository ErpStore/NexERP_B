---
mode: agent
description: Convert a V.SMART ERP list screen to the Zoho-inspired UI. Provide the file path of the target .razor file.
---

You are working inside the V.SMART ERP solution (Blazor Server + MudBlazor + Bootstrap).

The project has a Zoho-inspired design system defined in:
- `V.SMART.Shared/wwwroot/css/zoho-theme.css`  (all `zh-` prefixed classes)
- See `docs/ZOHO_UI_REDESIGN_PLAN.md` for the full class reference and rules

## Task

Convert the target list screen `${input:filePath}` from the old Bootstrap/card layout to the new Zoho-inspired layout.

## Rules — READ BEFORE MAKING ANY CHANGES

1. **NEVER modify the `@code { }` block** — zero C# logic changes.
2. **NEVER remove or rename `@inject` directives** at the top.
3. **NEVER change the `@page` route**.
4. **NEVER change component names** (`MasterModal`, `BsModal`, `InstantSearch`, `ColumnMenu`, `ProcessingOverlay`, `RedirectToLogin`, etc.) — only their parent wrapper HTML.
5. Read the file in full before editing.

## Conversion Steps

### Step 1 — Remove the old inline `<style>` block (if present)
Delete any `<style>...</style>` block at the top of the markup section.

### Step 2 — Replace the outer container with `zh-page`
Old:
```html
<div class="container-fluid px-2 px-md-5 mt-4">
  <div class="card shadow-sm border-1">
    <PageHeader ... />
    <div class="card-body p-4">
```
New:
```html
<div class="zh-page">
```

### Step 3 — Add a `zh-module-header` bar (replaces `<PageHeader>`)
```html
<div class="zh-module-header">
  <div class="zh-module-header-left">
    <span class="zh-module-icon"><i class="bi bi-{ICON}"></i></span>
    <div class="zh-module-title-group">
      <nav class="zh-breadcrumb" aria-label="breadcrumb">
        <i class="bi bi-house-door me-1"></i>
        <SmartBackButton Url="/" RenderAsLink="true" CssClass="zh-breadcrumb-link">
          <ChildContent>Home</ChildContent>
        </SmartBackButton>
        <span class="zh-breadcrumb-sep"><i class="bi bi-chevron-right"></i></span>
        <span>{Module}</span>
        <span class="zh-breadcrumb-sep"><i class="bi bi-chevron-right"></i></span>
        <span class="zh-breadcrumb-active">{SubModule}</span>
      </nav>
      <h1 class="zh-module-title">{Page Title}</h1>
    </div>
  </div>
  <div class="zh-module-header-right">
    @if (CanCreate) {
      <button class="zh-btn zh-btn-primary" @onclick="AddNew{Entity}">
        <i class="bi bi-plus-lg"></i> Add New
      </button>
    }
    <button class="zh-btn zh-btn-secondary" @onclick="HandleExport">
      <i class="bi bi-download"></i> Export
    </button>
  </div>
</div>
```

### Step 4 — Add a `zh-toolbar` bar (replaces the `<div class="row g-2">` filter row)
```html
<div class="zh-toolbar">
  <div class="zh-toolbar-left">
    <ColumnMenu ... />
    <button class="zh-btn zh-btn-ghost" @onclick='() => OpenModal("InstantSearch")'>
      <i class="bi bi-search"></i> Search
    </button>
    <button class="zh-btn zh-btn-ghost" @onclick="RefreshPage" title="Refresh">
      <i class="bi bi-arrow-clockwise"></i>
    </button>
  </div>
  <div class="zh-toolbar-right">
    @* status filter if the page has one *@
    <select class="zh-select" value="@selectedStatus" @onchange="OnStatusChanged">
      ...options...
    </select>
    <select class="zh-select" value="@pageSize" @onchange="OnPageSizeChanged">
      <option value="10" selected>10 / page</option>
      <option value="20">20 / page</option>
      <option value="50">50 / page</option>
    </select>
  </div>
</div>
```

### Step 5 — Replace `<QuickGrid>` with `<table class="zh-table">`
```html
<div class="zh-table-wrapper">
  <table class="zh-table">
    <thead>
      <tr>
        @foreach (var col in Columns.Where(c => c.IsVisible))
        {
          <th>@col.Title</th>
        }
        <th class="zh-col-actions">Actions</th>
      </tr>
    </thead>
    <tbody>
      @foreach (var item in {ItemList})
      {
        <tr>
          @foreach (var col in Columns.Where(x => x.IsVisible))
          {
            <td>
              @switch (col.Field)
              {
                // map each column field — copy the display logic from the old QuickGrid PropertyColumn / TemplateColumn blocks
              }
            </td>
          }
          <td class="zh-col-actions">
            <div class="zh-action-group">
              @if (CanEdit)   { <a href="..." class="zh-action-btn zh-action-edit"   title="Edit">  <i class="bi bi-pencil"></i></a> }
              @if (CanView)   { <a href="..." class="zh-action-btn zh-action-view"   title="View">  <i class="bi bi-eye"></i></a> }
              @if (CanDelete) { <button class="zh-action-btn zh-action-delete" title="Delete" @onclick="..."><i class="bi bi-trash"></i></button> }
            </div>
          </td>
        </tr>
      }
    </tbody>
  </table>
</div>
```

### Step 6 — Replace Bootstrap `.badge` with `zh-chip`

| Old | New |
|-----|-----|
| `badge text-bg-success` | `zh-chip zh-chip-success` |
| `badge text-bg-warning` | `zh-chip zh-chip-warning` |
| `badge text-bg-danger` | `zh-chip zh-chip-danger` |
| `badge text-bg-info` | `zh-chip zh-chip-info` |
| `badge text-bg-primary` | `zh-chip zh-chip-purple` |
| `badge text-bg-secondary` | `zh-chip zh-chip-neutral` |

Remove `bg-gradient`, `shadow-sm`, `rounded-pill` from all badge classes — the `zh-chip` class handles all of this.

### Step 7 — Replace the Bootstrap pagination with `zh-pagination`
```html
<div class="zh-pagination">
  <span class="zh-pagination-info">
    Showing @((currentPage - 1) * pageSize + 1)–@(Math.Min(currentPage * pageSize, totalItems)) of @totalItems records
  </span>
  <div class="zh-pagination-nav">
    <button class="zh-page-btn" @onclick="FirstPage"    disabled="@(currentPage == 1)"              title="First">    <i class="bi bi-chevron-double-left"></i></button>
    <button class="zh-page-btn" @onclick="PreviousPage" disabled="@(currentPage == 1)"              title="Previous"> <i class="bi bi-chevron-left"></i></button>
    <span class="zh-page-indicator">@currentPage / @totalPages</span>
    <button class="zh-page-btn" @onclick="NextPage"     disabled="@(currentPage >= totalPages)"     title="Next">     <i class="bi bi-chevron-right"></i></button>
    <button class="zh-page-btn" @onclick="LastPage"     disabled="@(currentPage >= totalPages)"     title="Last">     <i class="bi bi-chevron-double-right"></i></button>
  </div>
</div>
```

### Step 8 — Replace the empty-state fallback
```html
<div class="zh-empty-state">
  <i class="bi bi-inbox zh-empty-icon"></i>
  <p class="zh-empty-title">No {items} found</p>
  <p class="zh-empty-subtitle">Try adjusting your filters or add a new record.</p>
  @if (CanCreate) {
    <button class="zh-btn zh-btn-primary mt-3" @onclick="AddNew{Entity}">
      <i class="bi bi-plus-lg"></i> Add First {Entity}
    </button>
  }
</div>
```

### Step 9 — Close `zh-page` and `else` block
```html
    </div>   ← closes zh-page
}            ← closes the top-level else block
```

### Step 10 — Verify
- Confirm the file still has the `@page`, all `@inject` lines, `<MasterModal>`, `<BsModal>` unchanged at the top
- Confirm the `@code { }` block is byte-for-byte identical to the original
- The file should have NO `container-fluid`, `card`, `card-body`, `card-header`, `table-bordered`, `QuickGrid`, `PropertyColumn`, `TemplateColumn`, or Bootstrap `badge` classes remaining in the markup section
