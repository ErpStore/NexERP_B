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

## Seams left for the next task

- **M2-C05-02** — the two-way `columnVisibility` input is typed and honoured; nothing persists it.
  It is marked `TODO(M2-C05-02)` at the declaration.

## The five states and export (M2-C05-03, 2026-08-26)

The `#empty`, `#error` and `#toolbar` seams are filled. Each stays **overridable**: a caller that
supplies `#empty` or `#error` gets its own template instead, so nothing here has to be forked.

- `data-grid-states.component.ts` — the state switch. It composes M2-C04-03's primitives and adds
  no visuals of its own; `data-grid-states.component.css` is layout only.
- `data-grid-skeleton.component.ts` — `min(pageSize, 12)` rows sized to the **resolved** column
  widths, so the layout does not jump. The bars are `aria-hidden`; the region announces once.
- `data-grid-error.component.ts` — `403` → the inline permission-denied panel, `409` → the server's
  `title` verbatim, everything else → `app-error-state` with `traceId` and Retry. It also holds
  `toGridProblem`, the single normalisation point (see the deviations below).
- `grid-export.service.ts` + `data-grid-toolbar.component.ts` — the client asks the server for the
  bytes and saves them. It builds no file (ADR-005), and `no-client-file-generation.spec.ts` fails
  the build if a spreadsheet/CSV/PDF dependency is ever added.

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
- **M2-C05-03's one stylesheet is `.css`, which the M2-C05-01 note above does not cover.**
  `M2-C05-03.md:288` names `data-grid-states.component.css` explicitly, and the task file is
  binding. `inlineStyleLanguage` governs _inline_ styles only, so a `styleUrl` pointing at a
  `.css` file compiles and lints unchanged — the file holds layout and nothing else, so there is
  no Sass to lose.
- **`toGridProblem` normalises a `ProblemDetails` inside `data-grid-error.component.ts`, which
  `M2-C05-03.md:248-250` told it not to do.** That instruction says to consume what
  `core/http/error.interceptor.ts` (M2-C02) normalised — and that file does not exist:
  `src/app/core/http/` is empty and `app.config.ts:29` registers `withInterceptors([])`. The
  alternative was rendering a raw `HttpErrorResponse`, which cannot be branched on. It is one
  function, reusing `ProblemDetailsLike` from `form/server-validation.ts:22` rather than
  describing the contract a third time, and **Q-94** records that M2-C02 must call or replace it
  rather than stack a second normaliser on top.
- **Export offers Excel only, not the `Excel / CSV` menu `M2-C05-03.md:226` specifies.** The only
  list-export endpoint accepts `xlsx` exclusively and answers 400 for anything else
  (`V.SMART/V.SMART.Api/Controllers/CurrencyExcelController.cs:48`, `:100-104`), so a CSV entry
  would be a control that always fails. The format list is an input, so a resource whose endpoint
  gains CSV can offer it without a component change. **Q-95**.
- **`DataGridQueryState.#commit` now nulls the filter parameters a new state drops.**
  `queryParamsHandling: 'merge'` keeps any parameter the new object does not mention, and
  `toRouteParams` mentions only the filters still set — so before this change **Clear filters**
  wrote a URL that still carried the filter, the `queryParamMap` emission read it straight back,
  and the button appeared to do nothing. Only names this grid owns are nulled, so an unrelated
  `tab` or `returnUrl` on the same route is still left alone.

## Measured, not assumed

10,000 rows render **35** `<tr>`, at a **16.7 ms** median frame — 60 fps — in headless Chromium.
Method and the full table: [KB-050 § Performance targets](../../../../../../../docs/kb/frontend-new/react-architecture.md#performance-targets).
The DOM-row half of that is reproduced in jsdom by `data-grid.component.spec.ts`, so a regression
to client-side rendering fails CI.
