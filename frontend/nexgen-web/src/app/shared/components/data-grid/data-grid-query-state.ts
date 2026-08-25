import { HttpErrorResponse } from '@angular/common/http';
import {
  DestroyRef,
  computed,
  inject,
  signal,
  type Signal,
  type WritableSignal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, of } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap } from 'rxjs/operators';

import {
  fromRouteParams,
  toRouteParams,
  toWireQuery,
  type DataGridPage,
  type DataGridWireQuery,
} from './data-grid-query.adapter';
import {
  DATA_GRID_FILTER_DEBOUNCE_MS,
  clampPageSizeState,
  defaultDataGridState,
  type DataGridSort,
  type DataGridState,
} from './data-grid.model';

/**
 * Where a grid's rows come from. One call, one page. The caller writes it over
 * the generated OpenAPI client (M2-B10); this directory never names an
 * endpoint.
 */
export type DataGridDataSource<TRow> = (
  query: DataGridWireQuery,
) => import('rxjs').Observable<DataGridPage<TRow>>;

/** How a grid's query state relates to the browser URL. */
export type DataGridQueryMode = 'route' | 'detached';

export interface DataGridQueryStateOptions<TRow> {
  /** Issues one request for one page. Required. */
  readonly source: DataGridDataSource<TRow>;
  /**
   * `'route'` (the default) makes the URL the single source of truth.
   * `'detached'` holds the same signals and never touches the URL - required
   * by **M2-C06**, whose dialog must not mutate the page behind it.
   */
  readonly mode?: DataGridQueryMode;
  readonly pageSize?: number;
  readonly sort?: readonly DataGridSort[];
  readonly filters?: Readonly<Record<string, string>>;
  /**
   * The closed set of query parameters this grid owns as filters. Defaults to
   * the keys of `filters`. Anything outside it is left alone in the URL, so a
   * grid can share a route with a `tab` or a `returnUrl`.
   */
  readonly filterNames?: readonly string[];
  readonly filterDebounceMs?: number;
  /** Set `false` to hold the first request until an explicit `refresh()`. */
  readonly autoLoad?: boolean;
}

type LoadResult<TRow> =
  | { readonly ok: true; readonly page: DataGridPage<TRow> }
  | { readonly ok: false; readonly error: unknown };

/**
 * Page / page-size / sort / filter state for one grid, plus the rows that state
 * resolves to (M2-C05-01).
 *
 * Three properties are the whole point of it, and each is a behaviour the
 * previous Blazor list did not have:
 *
 *  1. **The URL is the state.** In route-bound mode every field round-trips
 *     through the query string, so a pasted link reproduces the grid exactly
 *     and browser back/forward work. The route is the only writer: a setter
 *     navigates, and the resulting `queryParamMap` emission is what updates the
 *     signals and issues the request. One path in, so a URL edited by hand and
 *     a click on a column header cannot diverge.
 *  2. **A refetch never blanks the table.** ADR-007 removed the query-cache
 *     library (`ADR-007-angular-stack.md:134-138`), so holding the last good
 *     page is this class's own job: `rows` is replaced only when the next page
 *     resolves, and `refetching` - not `loading` - is what a caller renders a
 *     progress bar from.
 *  3. **The server's error object survives.** `error` exposes the parsed
 *     `ProblemDetails` body as it arrived (M2-A06). Nothing here stringifies or
 *     genericises it; M2-C05-03 renders it.
 *
 * Requests are `switchMap`ped, so a slow page 2 that resolves after a fast page
 * 3 cannot overwrite it.
 */
export class DataGridQueryState<TRow> {
  readonly #router: Router | null;
  readonly #route: ActivatedRoute | null;
  readonly #source: DataGridDataSource<TRow>;
  readonly #mode: DataGridQueryMode;
  readonly #filterNames: readonly string[];
  readonly #defaults: DataGridState;

  readonly #state: WritableSignal<DataGridState>;
  readonly #filterDraft: WritableSignal<Readonly<Record<string, string>>>;
  readonly #rows = signal<readonly TRow[]>([]);
  readonly #totalCount = signal(0);
  readonly #loading = signal(false);
  readonly #refetching = signal(false);
  readonly #error = signal<unknown>(null);
  readonly #loaded = signal(false);

  readonly #request$ = new Subject<DataGridState>();
  readonly #filterCommit$ = new Subject<void>();

  /** The complete query state. Bind it to `app-data-grid`'s `state` input. */
  readonly state: Signal<DataGridState>;
  readonly page = computed(() => this.state().page);
  readonly pageSize = computed(() => this.state().pageSize);
  readonly sort = computed(() => this.state().sort);
  /** The **committed** filters - what the last request was made with. */
  readonly filters = computed(() => this.state().filters);
  /**
   * What the filter inputs display. Diverges from {@link filters} only for the
   * debounce window, so a keystroke is never swallowed or echoed back late.
   */
  readonly filterDraft: Signal<Readonly<Record<string, string>>>;

  readonly rows: Signal<readonly TRow[]> = this.#rows.asReadonly();
  /** The server's **filtered, unpaged** total (`PagedResult.cs:25-28`). */
  readonly totalCount: Signal<number> = this.#totalCount.asReadonly();
  /** First load only - there is nothing on screen yet. */
  readonly loading: Signal<boolean> = this.#loading.asReadonly();
  /** A reload with rows already on screen. They stay; a progress bar says more is coming. */
  readonly refetching: Signal<boolean> = this.#refetching.asReadonly();
  /** The server's `ProblemDetails`, untouched. `null` when the last request succeeded. */
  readonly error: Signal<unknown> = this.#error.asReadonly();

  readonly totalPages = computed(() => {
    const size = this.state().pageSize;
    return size > 0 ? Math.max(1, Math.ceil(this.totalCount() / size)) : 1;
  });

  constructor(options: DataGridQueryStateOptions<TRow>) {
    const destroyRef = inject(DestroyRef);
    this.#source = options.source;
    this.#mode = options.mode ?? 'route';
    this.#router = inject(Router, { optional: true });
    this.#route = inject(ActivatedRoute, { optional: true });

    const initialFilters = { ...(options.filters ?? {}) };
    this.#defaults = clampPageSizeState({
      ...defaultDataGridState(options.pageSize),
      sort: options.sort ?? [],
      filters: initialFilters,
    });
    this.#filterNames = options.filterNames ?? Object.keys(initialFilters);
    this.#state = signal(this.#defaults);
    this.#filterDraft = signal(initialFilters);
    this.state = this.#state.asReadonly();
    this.filterDraft = this.#filterDraft.asReadonly();

    if (this.#mode === 'route' && (!this.#router || !this.#route)) {
      throw new Error(
        'DataGridQueryState: route-bound mode needs Router and ActivatedRoute. ' +
          "Create it inside a routed component, or pass mode: 'detached'.",
      );
    }

    this.#request$
      .pipe(
        tap(() => this.#beginRequest()),
        switchMap((state) =>
          this.#source(toWireQuery(state)).pipe(
            map((page): LoadResult<TRow> => ({ ok: true, page })),
            catchError((error: unknown) => of<LoadResult<TRow>>({ ok: false, error })),
          ),
        ),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe((result) => this.#settle(result));

    this.#filterCommit$
      .pipe(
        debounceTime(options.filterDebounceMs ?? DATA_GRID_FILTER_DEBOUNCE_MS),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(() => {
        // A filter change always returns to page 1: page 7 of the old result
        // set is a different, usually empty, page of the new one.
        this.#commit({ ...this.#state(), filters: this.#filterDraft(), page: 1 });
      });

    if (this.#mode === 'route') {
      // Emits synchronously with the current parameters, so this subscription
      // is also what performs the first load.
      this.#route!.queryParamMap.pipe(takeUntilDestroyed(destroyRef)).subscribe((params) => {
        const next = fromRouteParams(params, this.#filterNames, this.#defaults);
        this.#state.set(next);
        this.#filterDraft.set(next.filters);
        if (options.autoLoad !== false) {
          this.#request$.next(next);
        }
      });
    } else if (options.autoLoad !== false) {
      this.#request$.next(this.#state());
    }
  }

  /** Go to a 1-based page. Out-of-range values are the server's to reject. */
  setPage(page: number): void {
    this.#commit({ ...this.#state(), page: Math.max(1, Math.trunc(page)) });
  }

  /** Change rows per page. Returns to page 1 - the old offset means nothing at a new size. */
  setPageSize(pageSize: number): void {
    this.#commit(clampPageSizeState({ ...this.#state(), pageSize, page: 1 }));
  }

  /** Replace the sort. Returns to page 1. */
  setSort(sort: readonly DataGridSort[]): void {
    this.#commit({ ...this.#state(), sort, page: 1 });
  }

  /**
   * Cycle one column between ascending, descending and unsorted - the order a
   * user expects from clicking the same header three times. Single-column;
   * multi-column sort is set through {@link setSort}.
   */
  toggleSort(field: string): void {
    const current = this.#state().sort.find((term) => term.field === field);
    if (!current) {
      this.setSort([{ field, direction: 'asc' }]);
    } else if (current.direction === 'asc') {
      this.setSort([{ field, direction: 'desc' }]);
    } else {
      this.setSort([]);
    }
  }

  /**
   * Set one filter. Debounced, because this is called per keystroke; the draft
   * updates at once so the input never lags behind the typing.
   */
  setFilter(name: string, value: string | null): void {
    const next = { ...this.#filterDraft() };
    if (value === null || value === '') {
      delete next[name];
    } else {
      next[name] = value;
    }
    this.#filterDraft.set(next);
    this.#filterCommit$.next();
  }

  /** Drop every filter. Immediate - a "clear filters" button is not typing. */
  clearFilters(): void {
    this.#filterDraft.set({});
    this.#commit({ ...this.#state(), filters: {}, page: 1 });
  }

  /** Apply a partial state change. The `stateChange` output of the grid lands here. */
  apply(patch: Partial<DataGridState>): void {
    const next = clampPageSizeState({ ...this.#state(), ...patch });
    if (patch.filters) {
      this.#filterDraft.set(next.filters);
    }
    this.#commit(next);
  }

  /**
   * Re-issue the current query. Explicit, because ADR-007 replaced the query
   * cache with exactly this (`ADR-007-angular-stack.md:134-138`) - there is no
   * cache key to invalidate.
   */
  refresh(): void {
    this.#request$.next(this.#state());
  }

  #commit(next: DataGridState): void {
    if (this.#mode === 'detached') {
      this.#state.set(next);
      this.#request$.next(next);
      return;
    }
    // Route-bound: write the URL and let the queryParamMap subscription do the
    // rest. `merge` keeps query parameters this grid does not own.
    void this.#router!.navigate([], {
      relativeTo: this.#route!,
      queryParams: toRouteParams(next),
      queryParamsHandling: 'merge',
    });
  }

  #beginRequest(): void {
    this.#error.set(null);
    if (this.#loaded()) {
      this.#refetching.set(true);
    } else {
      this.#loading.set(true);
    }
  }

  #settle(result: LoadResult<TRow>): void {
    this.#loading.set(false);
    this.#refetching.set(false);
    if (!result.ok) {
      // The ProblemDetails body, exactly as the server sent it. HttpClient wraps
      // it in an HttpErrorResponse; anything else (a network failure, a thrown
      // TypeError) is surfaced as-is rather than dressed up as a server error.
      this.#error.set(
        result.error instanceof HttpErrorResponse && result.error.error != null
          ? (result.error.error as unknown)
          : result.error,
      );
      return;
    }
    const items = result.page.items ?? [];
    this.#rows.set([...items]);
    this.#totalCount.set(result.page.totalCount ?? items.length);
    this.#loaded.set(true);
  }
}

/**
 * Creates a {@link DataGridQueryState}. Must be called in an injection context
 * - a field initialiser or a constructor - because route-bound mode injects
 * `Router` and `ActivatedRoute`, and both modes take a `DestroyRef` so their
 * subscriptions end with the component.
 */
export function createDataGridQueryState<TRow>(
  options: DataGridQueryStateOptions<TRow>,
): DataGridQueryState<TRow> {
  return new DataGridQueryState<TRow>(options);
}
