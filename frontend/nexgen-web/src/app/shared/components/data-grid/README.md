# `DataGrid` — the server-paged list grid

**Task:** `M2-C05-01` (core). **Specification:** [KB-051 § Data display](../../../../../../../docs/kb/frontend-new/design-system.md#data-display).
**Stack decision:** [ADR-007](../../../../../../../docs/kb/decisions/ADR-007-angular-stack.md) —
Angular 22 + PrimeNG, `p-table` wrapped rather than owned.

This directory replaces the 93 `<QuickGrid>`-and-Bootstrap list screens the Blazor app renders
today (INV-053). One grid, one component library — which is how R-22's two-design-systems
problem is retired for list screens.

## Three rules

1. **Server-driven.** Paging, sorting and filtering are _requests_. Nothing here sorts or
   filters a result set locally, and `data-grid.component.spec.ts` asserts it.
2. **The URL is the state**, unless the host asks for a detached grid.
3. **No business rule.** The grid renders rows. No totals from the visible page, no money
   arithmetic, no decision about whether a row may be edited, no filtering for a domain reason.

## The files

| File                               | Owns                                                                                                                                                                               |
| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `data-grid.model.ts`               | The column and query-state vocabulary. Mirrors `GridColumn.cs:3-13`.                                                                                                               |
| `data-grid-query.adapter.ts`       | **The only module that knows a wire format** — M2-B02's API contract _and_ the browser URL shape, which are deliberately different. Also declares `DataGridPage<TRow>`; see below. |
| `data-grid-query-state.ts`         | Page / size / sort / filter signals, the route binding, and the request pipeline.                                                                                                  |
| `data-grid.component.*`            | The grid itself, over `p-table` in `lazy` mode.                                                                                                                                    |
| `data-grid-header.component.*`     | The column-header row. A component on a `<tr>` — see the deviation below.                                                                                                          |
| `data-grid-pagination.component.*` | The pager. `app-select` for page size, never a second dropdown vocabulary.                                                                                                         |
| `grid-keyboard-navigation.ts`      | The ARIA `grid` keyboard model, as pure functions.                                                                                                                                 |
| `test-fixtures.ts`                 | **Test only.** Rows, columns, hosts, and the jsdom patches the virtual scroller needs.                                                                                             |

## Two wire formats, on purpose

|                                         | Paging                   | Sort                         |
| --------------------------------------- | ------------------------ | ---------------------------- |
| **API** (M2-B02, `PagedQuery.cs:37-82`) | `pageNumber`, `pageSize` | `sort=-createdDate,currName` |
| **URL**                                 | `page`, `size`           | `sort=name:desc,code:asc`    |

Filters need no translation either way: `DataGridState.filters` is keyed by query-parameter name
already. Both serialisations live in `data-grid-query.adapter.ts` and nowhere else.

## `DataGridPage<TRow>` is declared, not imported

M2-B10 landed, and it _does_ generate this envelope — but once per resource and never
generically (`CurrencyVMPagedResult`, and `CustomerVMPagedResult` next to it). OpenAPI 3.0 has
no generics, so a component generic over `TRow` cannot import any of them. The adapter declares a
structurally identical interface instead — field for field, optionality included — so a generated
per-resource type is assignable to it with no cast and no mapping. Recorded as **INV-052**.

## Seams left for the next two tasks

- **M2-C05-02** — the two-way `columnVisibility` input is typed and honoured; nothing persists it.
- **M2-C05-03** — `#empty`, `#error` and `#toolbar` template slots are typed and reachable. The
  `ProblemDetails` object reaches the error slot untouched. Export lands in the toolbar.

Both are marked `TODO(<task id>)` at the declaration.

## Deviations, each with its reason

- **The two selection checkboxes are native `<input type="checkbox">`.** `app-checkbox`
  (M2-C04-02) has no indeterminate state, and "some rows on this page are selected" must be
  distinguishable from "none are". Adding the input to `app-checkbox` would edit a file this task
  does not own. The filter row and the page-size selector do use M2-C04-02's controls.
- **`DataGridHeaderComponent`'s selector is `tr[appDataGridHeader]`.** A `<thead>` may contain
  only `<tr>`; an element between them is invalid table markup the browser hoists out of the
  table. `@angular-eslint/component-selector` is waived on that one line and nowhere else.
- **Stylesheets are `.scss`, not the `.css` the task file names.** `angular.json` sets
  `inlineStyleLanguage: scss` and every other component directory here is `.scss`.

## Measured, not assumed

10,000 rows render **35** `<tr>`, at a **16.7 ms** median frame — 60 fps — in headless Chromium.
Method and the full table: [KB-050 § Performance targets](../../../../../../../docs/kb/frontend-new/react-architecture.md#performance-targets).
The DOM-row half of that is reproduced in jsdom by `data-grid.component.spec.ts`, so a regression
to client-side rendering fails CI.
