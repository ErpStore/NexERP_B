import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { Router, convertToParamMap, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { createDataGridQueryState, type DataGridQueryState } from './data-grid-query-state';
import {
  fromRouteParams,
  fromRouteSort,
  toRouteParams,
  toRouteSort,
  toWireQuery,
  toWireSort,
} from './data-grid-query.adapter';
import { defaultDataGridState } from './data-grid.model';
import {
  DetachedGridHostComponent,
  RouteGridHostComponent,
  TEST_ENDPOINT,
  makeRows,
  pageOf,
  testDataSource,
  type TestRow,
} from './test-fixtures';

/**
 * The wire format is M2-B02's, confirmed against
 * `V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:37-82` and the generated client
 * at `core/api/generated/fn/currency/get-currencies.ts:14-55`. These
 * assertions are the guard that keeps the adapter honest about it.
 */
describe('data-grid query adapter', () => {
  it('serialises paging to M2-B02 wire names', () => {
    const query = toWireQuery({ ...defaultDataGridState(), page: 3, pageSize: 50 });

    expect(query).toEqual({ pageNumber: 3, pageSize: 50 });
  });

  it('serialises descending sort with the - prefix and omits an empty sort', () => {
    expect(toWireSort([{ field: 'createdDate', direction: 'desc' }])).toBe('-createdDate');
    expect(
      toWireSort([
        { field: 'createdDate', direction: 'desc' },
        { field: 'currName', direction: 'asc' },
      ]),
    ).toBe('-createdDate,currName');
    expect(toWireSort([])).toBeUndefined();
  });

  it('clamps pageSize to the server maximum rather than sending a 400', () => {
    expect(toWireQuery({ ...defaultDataGridState(), pageSize: 5000 })['pageSize']).toBe(100);
  });

  it('drops an empty filter instead of sending a blank parameter', () => {
    const query = toWireQuery({
      ...defaultDataGridState(),
      filters: { currName: 'acme', createdBy: '' },
    });

    expect(query).toEqual({ pageNumber: 1, pageSize: 20, currName: 'acme' });
  });

  it('uses the shorter page/size/field:direction shape in the URL', () => {
    const params = toRouteParams({
      page: 3,
      pageSize: 50,
      sort: [{ field: 'name', direction: 'desc' }],
      filters: { code: 'C1' },
    });

    expect(params).toEqual({ page: '3', size: '50', sort: 'name:desc', code: 'C1' });
  });

  it('drops defaults from the URL so a first page has a clean address', () => {
    expect(toRouteParams(defaultDataGridState())).toEqual({ page: null, size: null, sort: null });
  });

  it('round-trips a sort through the URL shape', () => {
    const sort = [
      { field: 'name', direction: 'desc' as const },
      { field: 'code', direction: 'asc' as const },
    ];

    expect(fromRouteSort(toRouteSort(sort))).toEqual(sort);
  });

  it('reads a hand-edited URL defensively rather than throwing', () => {
    const state = fromRouteParams(
      convertToParamMap({ page: 'zero', size: '-4' }),
      ['code'],
      defaultDataGridState(),
    );

    expect(state).toEqual({ page: 1, pageSize: 20, sort: [], filters: {} });
  });

  it('leaves query parameters the grid does not own alone', () => {
    const state = fromRouteParams(
      convertToParamMap({ page: '2', tab: 'archived', code: 'C1' }),
      ['code'],
      defaultDataGridState(),
    );

    expect(state.filters).toEqual({ code: 'C1' });
  });
});

function makeDetachedGrid(
  options: Partial<Parameters<typeof createDataGridQueryState<TestRow>>[0]> = {},
): DataGridQueryState<TestRow> {
  return TestBed.runInInjectionContext(() =>
    createDataGridQueryState<TestRow>({
      source: testDataSource(TestBed.inject(HttpClient)),
      mode: 'detached',
      filterDebounceMs: 5,
      filterNames: ['code', 'name'],
      ...options,
    }),
  );
}

/** The debounce runs on real timers; 5 ms in, 25 ms out. */
const afterDebounce = () => new Promise((resolve) => setTimeout(resolve, 25));

describe('DataGridQueryState', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads the first page on creation', () => {
    const grid = makeDetachedGrid();

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('pageNumber')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('20');
    request.flush(pageOf(makeRows(20), 1, 20, 137));

    expect(grid.rows()).toHaveLength(20);
    expect(grid.totalCount()).toBe(137);
  });

  it('issues a new request with the new page number when the page changes', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 137));

    grid.setPage(3);

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('pageNumber')).toBe('3');
    request.flush(pageOf(makeRows(20, 40), 3, 20, 137));
    expect(grid.rows()[0]?.id).toBe(41);
  });

  it('sorts on the server and never reorders the page it already has', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(3), 1, 20, 3));
    const before = grid.rows().map((row) => row.id);

    grid.setSort([{ field: 'name', direction: 'desc' }]);

    // Still the old order until the server answers - no local reorder.
    expect(grid.rows().map((row) => row.id)).toEqual(before);
    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('sort')).toBe('-name');
    expect(request.request.params.get('pageNumber')).toBe('1');

    // The server's order is rendered verbatim, whatever it is.
    request.flush(pageOf([...makeRows(3)].reverse(), 1, 20, 3));
    expect(grid.rows().map((row) => row.id)).toEqual([3, 2, 1]);
  });

  it('returns to page 1 when the sort changes', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 500));
    grid.setPage(5);
    http.expectOne(() => true).flush(pageOf(makeRows(20, 80), 5, 20, 500));

    grid.setSort([{ field: 'code', direction: 'asc' }]);

    expect(http.expectOne(() => true).request.params.get('pageNumber')).toBe('1');
  });

  it('debounces a filter into one request and returns to page 1', async () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 500));
    grid.setPage(4);
    http.expectOne(() => true).flush(pageOf(makeRows(20, 60), 4, 20, 500));

    grid.setFilter('code', 'C');
    grid.setFilter('code', 'C0');
    grid.setFilter('code', 'C00');
    // The draft updates immediately so the input never lags the typing.
    expect(grid.filterDraft()).toEqual({ code: 'C00' });
    http.expectNone((candidate) => candidate.url === TEST_ENDPOINT);

    await afterDebounce();

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('code')).toBe('C00');
    expect(request.request.params.get('pageNumber')).toBe('1');
    request.flush(pageOf(makeRows(2), 1, 20, 2));
    expect(grid.filters()).toEqual({ code: 'C00' });
  });

  it('keeps the previous page on screen while the next one is fetching', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 137));

    grid.setPage(2);

    expect(grid.rows()).toHaveLength(20);
    expect(grid.rows()[0]?.id).toBe(1);
    expect(grid.loading()).toBe(false);
    expect(grid.refetching()).toBe(true);

    http.expectOne(() => true).flush(pageOf(makeRows(20, 20), 2, 20, 137));
    expect(grid.refetching()).toBe(false);
    expect(grid.rows()[0]?.id).toBe(21);
  });

  it('reports loading, not refetching, before the first page arrives', () => {
    const grid = makeDetachedGrid();

    expect(grid.loading()).toBe(true);
    expect(grid.refetching()).toBe(false);
    http.expectOne(() => true).flush(pageOf(makeRows(1), 1, 20, 1));
  });

  it('exposes the ProblemDetails object exactly as the server sent it', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(5), 1, 20, 5));
    const problem = {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: { sort: ['sort must be one of: currName, createdDate.'] },
      traceId: '00-abc-def-01',
    };

    grid.setSort([{ field: 'nope', direction: 'asc' }]);
    http.expectOne(() => true).flush(problem, { status: 400, statusText: 'Bad Request' });

    expect(grid.error()).toEqual(problem);
    // The rows the user was looking at are still there.
    expect(grid.rows()).toHaveLength(5);
  });

  it('refresh re-issues the current query without changing it', () => {
    const grid = makeDetachedGrid();
    http.expectOne(() => true).flush(pageOf(makeRows(5), 1, 20, 5));

    grid.refresh();

    expect(http.expectOne(() => true).request.params.get('pageNumber')).toBe('1');
  });
});

describe('DataGridQueryState, route-bound', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideLocationMocks(),
        provideRouter([
          { path: 'rows', component: RouteGridHostComponent },
          { path: 'dialog', component: DetachedGridHostComponent },
        ]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('reads the whole query state out of the URL on activation', async () => {
    await RouterTestingHarness.create('/rows?page=3&size=50&sort=name:desc&code=C00');

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('pageNumber')).toBe('3');
    expect(request.request.params.get('pageSize')).toBe('50');
    expect(request.request.params.get('sort')).toBe('-name');
    expect(request.request.params.get('code')).toBe('C00');
    request.flush(pageOf(makeRows(50, 100), 3, 50, 400));
  });

  it('writes the state back to the URL, so the address bar is the state', async () => {
    const harness = await RouterTestingHarness.create('/rows');
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 400));
    const host = harness.routeDebugElement!.componentInstance as RouteGridHostComponent;

    host.grid.setPage(4);
    await harness.fixture.whenStable();

    expect(TestBed.inject(Router).url).toContain('page=4');
    http.expectOne(() => true).flush(pageOf(makeRows(20, 60), 4, 20, 400));
    expect(host.grid.page()).toBe(4);
  });

  it('detached mode issues the same requests and writes nothing to the URL', async () => {
    const harness = await RouterTestingHarness.create('/dialog');
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 400));
    const host = harness.routeDebugElement!.componentInstance as DetachedGridHostComponent;
    const urlBefore = TestBed.inject(Router).url;

    host.grid.setPage(4);
    await harness.fixture.whenStable();

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('pageNumber')).toBe('4');
    request.flush(pageOf(makeRows(20, 60), 4, 20, 400));

    expect(TestBed.inject(Router).url).toBe(urlBefore);
    expect(TestBed.inject(Router).url).not.toContain('page=');
  });
});
