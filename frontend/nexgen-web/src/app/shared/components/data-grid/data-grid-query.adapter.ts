import type { ParamMap, Params } from '@angular/router';

import {
  DATA_GRID_DEFAULT_PAGE_SIZE,
  clampPageSize,
  type DataGridSort,
  type DataGridState,
} from './data-grid.model';

/**
 * The **only** module that knows a wire format (M2-C05-01, implementation
 * requirement 6). Two formats meet here and they are deliberately not the same
 * one:
 *
 *  1. **The API contract** (M2-B02). `pageNumber`, `pageSize`, and a
 *     comma-separated `sort` whose descending terms carry a `-` prefix -
 *     `V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:37-82`, confirmed against
 *     the generated client at
 *     `src/app/core/api/generated/fn/currency/get-currencies.ts:14-55`.
 *  2. **The browser URL**. `page`, `size`, and a `field:direction` sort - the
 *     shape the task file states and its test 5 asserts. It is shorter and
 *     reads better in an address bar that a user copies to a colleague, and it
 *     is free to stay stable if the API contract ever versions.
 *
 * Filters need no translation in either direction: `DataGridState.filters` is
 * already keyed by query-parameter name, and M2-B02 puts per-resource filters
 * on the wire under those same names (`currName`, `createdBy`, `fromDate`,
 * `toDate` - `get-currencies.ts:19-53`). Every other module in this directory
 * therefore handles state, never parameters.
 */

/** URL query-parameter name for the 1-based page index. */
export const ROUTE_PAGE_PARAM = 'page';
/** URL query-parameter name for the page size. */
export const ROUTE_SIZE_PARAM = 'size';
/** URL query-parameter name for the sort specification. */
export const ROUTE_SORT_PARAM = 'sort';

/** Wire query-parameter name - `PagedQuery.PageNumberParameter` (`PagedQuery.cs:37`). */
export const WIRE_PAGE_NUMBER_PARAM = 'pageNumber';
/** Wire query-parameter name - `PagedQuery.PageSizeParameter` (`PagedQuery.cs:40`). */
export const WIRE_PAGE_SIZE_PARAM = 'pageSize';
/** Wire query-parameter name - `PagedQuery.SortParameter` (`PagedQuery.cs:43`). */
export const WIRE_SORT_PARAM = 'sort';

/** The reserved URL parameter names, which can never be a filter name. */
const RESERVED_ROUTE_PARAMS: readonly string[] = [
  ROUTE_PAGE_PARAM,
  ROUTE_SIZE_PARAM,
  ROUTE_SORT_PARAM,
];

/**
 * The paged-list response envelope, `{ items, totalCount, pageNumber, pageSize }` -
 * `V.SMART/V.SMART.Api/Contracts/PagedResult.cs:31-35`.
 *
 * **Why this is declared rather than imported.** M2-B10 has landed, and it does
 * generate this envelope - but once per resource and never generically:
 * `src/app/core/api/generated/models/currency-vm-paged-result.ts` exports
 * `CurrencyVMPagedResult`, and the next list endpoint will produce
 * `CustomerVMPagedResult` beside it. OpenAPI 3.0 has no generics, so a
 * component that is generic over its row type cannot consume any of them.
 * Recorded as INV-052 in `docs/kb/investigation-registry.md`, with the
 * conformance proof: this interface is structurally identical to the generated
 * one, field for field, optionality included.
 *
 * A caller passes a `DataGridDataSource` that calls the generated client and
 * hands the result straight here; the generated per-resource type is assignable
 * to `DataGridPage<TRow>` with no cast and no mapping.
 */
export interface DataGridPage<TRow> {
  readonly items?: readonly TRow[] | null;
  /** The **filtered, unpaged** count (`PagedResult.cs:25-28`). */
  readonly totalCount?: number;
  readonly pageNumber?: number;
  readonly pageSize?: number;
}

/** A ready-to-send query string, in M2-B02's vocabulary. */
export type DataGridWireQuery = Readonly<Record<string, string | number>>;

/**
 * State to API query parameters. Omits `sort` entirely when there is no sort
 * term, which is what M2-B02 documents as "keep the resource's existing default
 * ordering" (`PagedQuery.cs:69-76`) - an empty string would be a 400.
 */
export function toWireQuery(state: DataGridState): DataGridWireQuery {
  const query: Record<string, string | number> = {
    [WIRE_PAGE_NUMBER_PARAM]: state.page,
    [WIRE_PAGE_SIZE_PARAM]: clampPageSize(state.pageSize),
  };
  const sort = toWireSort(state.sort);
  if (sort) {
    query[WIRE_SORT_PARAM] = sort;
  }
  for (const [name, value] of Object.entries(state.filters)) {
    if (value !== '') {
      query[name] = value;
    }
  }
  return query;
}

/** `[{name,'desc'},{code,'asc'}]` becomes `-name,code`. Empty becomes `undefined`. */
export function toWireSort(sort: readonly DataGridSort[]): string | undefined {
  if (sort.length === 0) {
    return undefined;
  }
  return sort.map((term) => (term.direction === 'desc' ? `-${term.field}` : term.field)).join(',');
}

/** State to URL query parameters, in the `page` / `size` / `field:dir` shape. */
export function toRouteParams(state: DataGridState): Params {
  const params: Params = {
    [ROUTE_PAGE_PARAM]: state.page === 1 ? null : String(state.page),
    [ROUTE_SIZE_PARAM]:
      state.pageSize === DATA_GRID_DEFAULT_PAGE_SIZE ? null : String(state.pageSize),
    [ROUTE_SORT_PARAM]: toRouteSort(state.sort),
  };
  for (const [name, value] of Object.entries(state.filters)) {
    params[name] = value === '' ? null : value;
  }
  return params;
}

/** `[{name,'desc'}]` becomes `name:desc`. Empty becomes `null` - the parameter is dropped. */
export function toRouteSort(sort: readonly DataGridSort[]): string | null {
  if (sort.length === 0) {
    return null;
  }
  return sort.map((term) => `${term.field}:${term.direction}`).join(',');
}

/** `name:desc,code` becomes `[{name,'desc'},{code,'asc'}]`. Ascending is the default. */
export function fromRouteSort(raw: string | null): readonly DataGridSort[] {
  if (!raw) {
    return [];
  }
  return raw
    .split(',')
    .map((term) => term.trim())
    .filter((term) => term.length > 0)
    .map((term) => {
      const [field, direction] = term.split(':');
      return {
        field: field ?? '',
        direction: direction === 'desc' ? 'desc' : 'asc',
      } satisfies DataGridSort;
    })
    .filter((term) => term.field.length > 0);
}

/**
 * URL query parameters to state.
 *
 * `filterNames` is the closed set of parameters this grid treats as filters, so
 * an unrelated query parameter on the same route - a `tab`, a `returnUrl` - is
 * neither read as a filter nor erased when the grid next writes the URL.
 * Anything unparseable falls back to the default rather than throwing: a URL is
 * user input, and a hand-edited `page=zero` should show page 1, not a stack
 * trace.
 */
export function fromRouteParams(
  params: ParamMap,
  filterNames: readonly string[],
  fallback: DataGridState,
): DataGridState {
  const filters: Record<string, string> = {};
  for (const name of filterNames) {
    if (RESERVED_ROUTE_PARAMS.includes(name)) {
      continue;
    }
    const value = params.get(name);
    if (value !== null && value !== '') {
      filters[name] = value;
    }
  }
  return {
    page: readPositiveInt(params.get(ROUTE_PAGE_PARAM), fallback.page),
    pageSize: clampPageSize(readPositiveInt(params.get(ROUTE_SIZE_PARAM), fallback.pageSize)),
    sort: params.has(ROUTE_SORT_PARAM)
      ? fromRouteSort(params.get(ROUTE_SORT_PARAM))
      : fallback.sort,
    filters,
  };
}

function readPositiveInt(raw: string | null, fallback: number): number {
  if (raw === null || raw.trim() === '') {
    return fallback;
  }
  const parsed = Number(raw);
  if (!Number.isFinite(parsed) || parsed < 1) {
    return fallback;
  }
  return Math.trunc(parsed);
}
