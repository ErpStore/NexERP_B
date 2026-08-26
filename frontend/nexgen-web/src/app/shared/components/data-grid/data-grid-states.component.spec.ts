import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import axe, { type Result } from 'axe-core';
import { afterAll, beforeAll, beforeEach, describe, expect, it } from 'vitest';

import { DataGridComponent } from './data-grid.component';
import { DATA_GRID_MAX_SKELETON_ROWS } from './data-grid-skeleton.component';
import { defaultDataGridState, type DataGridState } from './data-grid.model';
import {
  businessRuleProblem,
  screenRightDeniedProblem,
  unhandledProblem,
} from './fixtures/problem-details';
import {
  RouteGridHostComponent,
  TEST_COLUMNS,
  TEST_ENDPOINT,
  installGridJsdomSupport,
  makeRows,
  pageOf,
  testRowId,
  uninstallGridJsdomSupport,
  type TestRow,
} from './test-fixtures';

/**
 * The five states of `DataGrid` (M2-C05-03), each rendered by composing
 * M2-C04-03's primitives. What is asserted here is behaviour a user can
 * observe: which words appear, which action is offered, and what a screen
 * reader is told.
 */
const TEMPLATE = `
  <app-data-grid
    ariaLabel="Currencies"
    [columns]="columns"
    [rows]="rows"
    [totalCount]="totalCount"
    [state]="state"
    [loading]="loading"
    [refetching]="refetching"
    [error]="error"
    [hasActiveFilters]="hasActiveFilters"
    [emptyTitle]="emptyTitle"
    [emptyDescription]="emptyDescription"
    [emptyActionLabel]="emptyActionLabel"
    [getRowId]="rowId"
    (emptyAction)="onEmptyAction()"
    (clearFilters)="onClearFilters()"
    (retry)="onRetry()"
  />`;

interface HostProperties {
  columns: typeof TEST_COLUMNS;
  rows: readonly TestRow[];
  totalCount: number;
  state: DataGridState;
  loading: boolean;
  refetching: boolean;
  error: unknown;
  hasActiveFilters: boolean;
  emptyTitle: string;
  emptyDescription: string | undefined;
  emptyActionLabel: string | undefined;
  rowId: typeof testRowId;
  onEmptyAction: () => void;
  onClearFilters: () => void;
  onRetry: () => void;
}

async function setup(overrides: Partial<HostProperties> = {}) {
  const events: string[] = [];
  const properties: HostProperties = {
    columns: TEST_COLUMNS,
    rows: [],
    totalCount: 0,
    state: defaultDataGridState(),
    loading: false,
    refetching: false,
    error: null,
    hasActiveFilters: false,
    emptyTitle: 'No currencies yet',
    emptyDescription: 'Add the first one to get started.',
    emptyActionLabel: 'New currency',
    rowId: testRowId,
    onEmptyAction: () => events.push('action'),
    onClearFilters: () => events.push('clear'),
    onRetry: () => events.push('retry'),
    ...overrides,
  };
  const { fixture } = await render(TEMPLATE, {
    imports: [DataGridComponent],
    componentProperties: { ...properties },
  });
  return { fixture, events, root: fixture.nativeElement as HTMLElement };
}

describe('DataGrid states', () => {
  beforeAll(() => installGridJsdomSupport());
  afterAll(() => uninstallGridJsdomSupport());

  /** Test 1. A spinner on a blank page is the failure KB-051 names outright. */
  it('renders skeleton rows, not a spinner, on first load', async () => {
    const { root } = await setup({ loading: true });

    const skeleton = root.querySelector('.app-data-grid__skeleton');
    expect(skeleton).not.toBeNull();
    expect(skeleton?.getAttribute('role')).toBe('status');
    expect(root.querySelectorAll('.app-data-grid__skeleton-row').length).toBeGreaterThan(0);
    expect(root.querySelector('p-progressspinner')).toBeNull();
  });

  it('marks the grid aria-busy while the first load is running', async () => {
    const { root } = await setup({ loading: true });

    expect(root.querySelector('table[aria-busy="true"]')).not.toBeNull();
  });

  it('is not aria-busy once the rows have arrived', async () => {
    const { root } = await setup({ rows: makeRows(3), totalCount: 3 });

    expect(root.querySelector('table[aria-busy="true"]')).toBeNull();
  });

  it('caps the skeleton at min(pageSize, 12) rows', async () => {
    const { root } = await setup({
      loading: true,
      state: { ...defaultDataGridState(), pageSize: 100 },
    });

    expect(root.querySelectorAll('.app-data-grid__skeleton-row')).toHaveLength(
      DATA_GRID_MAX_SKELETON_ROWS,
    );
  });

  it('uses the page size when it is smaller than the cap', async () => {
    const { root } = await setup({
      loading: true,
      state: { ...defaultDataGridState(), pageSize: 5 },
    });

    expect(root.querySelectorAll('.app-data-grid__skeleton-row')).toHaveLength(5);
  });

  /** Test 2. The layout must not jump when the real rows arrive. */
  it('sizes skeleton cells to the resolved column widths', async () => {
    const { root } = await setup({ loading: true });

    const cells = root.querySelectorAll<HTMLElement>(
      '.app-data-grid__skeleton-row:first-of-type .app-data-grid__skeleton-cell',
    );
    const widths = Array.from(cells).map((cell) => cell.style.width);
    expect(widths).toEqual(TEST_COLUMNS.map((column) => column.width));
  });

  it('announces the load once, not once per placeholder bar', async () => {
    const { root } = await setup({ loading: true });

    const announced = root.querySelectorAll('.app-data-grid__skeleton [role="status"]');
    expect(announced).toHaveLength(0);
    expect(root.querySelector('.app-data-grid__skeleton')?.textContent).toContain(
      'Loading results',
    );
  });

  /** Test 3. */
  it('keeps the previous rows and shows a progress bar on a refetch', async () => {
    const { root } = await setup({ rows: makeRows(3), totalCount: 137, refetching: true });

    expect(screen.getByText('Row 1')).toBeDefined();
    expect(root.querySelector('.app-data-grid__progress p-progressbar')).not.toBeNull();
    expect(root.querySelector('.app-data-grid__skeleton')).toBeNull();
  });

  it('does not disable the table while refetching', async () => {
    const { root } = await setup({ rows: makeRows(3), totalCount: 137, refetching: true });

    expect(root.querySelector('table[aria-disabled="true"]')).toBeNull();
    expect(root.querySelector('[data-row="0"][data-col="0"]')).not.toBeNull();
  });

  it('does not move focus when a refetch begins', async () => {
    const { fixture, root } = await setup({ rows: makeRows(3), totalCount: 137 });
    const cell = root.querySelector<HTMLElement>('[data-row="1"][data-col="1"]');
    cell?.focus();

    fixture.componentInstance.refetching = true;
    fixture.detectChanges();
    await fixture.whenStable();

    expect(document.activeElement).toBe(cell);
  });

  /** Test 4. */
  it('renders the "no data yet" variant with the primary action when no filter is active', async () => {
    const { events } = await setup({ hasActiveFilters: false });

    expect(screen.getByRole('heading', { name: 'No currencies yet' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'Clear filters' })).toBeNull();

    await userEvent.click(screen.getByRole('button', { name: 'New currency' }));
    expect(events).toContain('action');
  });

  /** Test 5. */
  it('renders the "filtered to nothing" variant with Clear filters when a filter is active', async () => {
    const { events } = await setup({ hasActiveFilters: true });

    expect(screen.getByRole('heading', { name: 'No results for these filters' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'New currency' })).toBeNull();

    await userEvent.click(screen.getByRole('button', { name: 'Clear filters' }));
    expect(events).toContain('clear');
  });

  it('names the "no data yet" variant distinctly in the markup', async () => {
    const { root } = await setup({ hasActiveFilters: false });

    expect(root.querySelector('.app-empty-state[data-variant="no-data"]')).not.toBeNull();
    expect(root.textContent).toContain('No currencies yet');
    expect(root.textContent).not.toContain('No results for these filters');
  });

  it('names the "filtered to nothing" variant distinctly in the markup', async () => {
    const { root } = await setup({ hasActiveFilters: true });

    expect(root.querySelector('.app-empty-state[data-variant="filtered"]')).not.toBeNull();
    expect(root.textContent).toContain('No results for these filters');
    expect(root.textContent).not.toContain('No currencies yet');
  });

  it('renders the error state instead of an empty state when the request failed', async () => {
    const { events } = await setup({ error: unhandledProblem() });

    expect(screen.queryByRole('heading', { name: 'No currencies yet' })).toBeNull();
    expect(screen.getByText('An unexpected error occurred.')).toBeDefined();

    await userEvent.click(screen.getByRole('button', { name: 'Retry' }));
    expect(events).toContain('retry');
  });

  /** Test 8, through the grid rather than the error component alone. */
  it('renders a 403 as the inline permission-denied state', async () => {
    const { root } = await setup({ error: screenRightDeniedProblem('Currency', 'View') });

    expect(root.querySelector('.app-permission-denied-state')).not.toBeNull();
  });

  /** Test 9, through the grid rather than the error component alone. */
  it('renders a 409 as the server sentence, byte-for-byte', async () => {
    const { root } = await setup({ error: businessRuleProblem() });

    expect(root.querySelector('.app-inline-alert__message')?.textContent).toBe(
      'Cannot delete this Sales Order as a Sales DC transaction exists.',
    );
  });

  /* Test 16, one state per test - the TestBed may be configured only once. */

  it('announces the first-load state', async () => {
    const { root } = await setup({ loading: true });

    expect(root.querySelector('.app-data-grid__skeleton[role="status"]')).not.toBeNull();
  });

  it('announces the empty state and keeps its action keyboard-reachable', async () => {
    const { root } = await setup();

    expect(root.querySelector('.app-empty-state[role="status"]')).not.toBeNull();
    const action = screen.getByRole('button', { name: 'New currency' });
    action.focus();
    expect(document.activeElement).toBe(action);
  });

  it('announces the filtered-empty state and keeps Clear filters keyboard-reachable', async () => {
    const { root } = await setup({ hasActiveFilters: true });

    expect(root.querySelector('.app-empty-state[role="status"]')).not.toBeNull();
    const clear = screen.getByRole('button', { name: 'Clear filters' });
    clear.focus();
    expect(document.activeElement).toBe(clear);
  });

  it('announces the error state politely and keeps Retry keyboard-reachable', async () => {
    const { root } = await setup({ error: unhandledProblem() });

    expect(root.querySelector('[role="alert"]')).not.toBeNull();
    expect(root.querySelector('[aria-live="polite"]')).not.toBeNull();
    const retry = screen.getByRole('button', { name: 'Retry' });
    retry.focus();
    expect(document.activeElement).toBe(retry);
  });

  it('announces the permission-denied state', async () => {
    const { root } = await setup({ error: screenRightDeniedProblem() });

    expect(root.querySelector('.app-permission-denied-state[role="status"]')).not.toBeNull();
    expect(root.querySelector('[aria-live="assertive"]')).not.toBeNull();
  });
});

/**
 * Test 17. A runtime `axe` scan over each of the five states.
 *
 * jsdom limitation, stated rather than hidden: no stylesheet is applied and no
 * layout is computed, so `color-contrast` cannot run here. Contrast is covered
 * by computation in `src/app/core/theme/contrast.spec.ts` (M2-C04-01), and the
 * repository-wide axe-in-CI pass is still M5-09's.
 */
describe('DataGrid states - axe', () => {
  /** An axe pass over a grid in jsdom is slow. Not a hang. */
  const AXE_IS_SLOW = 60_000;

  beforeAll(() => installGridJsdomSupport());
  afterAll(() => uninstallGridJsdomSupport());

  async function critical(root: HTMLElement): Promise<Result[]> {
    const results = await axe.run(root, {
      resultTypes: ['violations'],
      rules: { 'color-contrast': { enabled: false } },
    });
    return results.violations.filter((violation) => violation.impact === 'critical');
  }

  it(
    'reports no critical violation in the first-load state',
    async () => {
      const { root } = await setup({ loading: true });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the refetch state',
    async () => {
      const { root } = await setup({ rows: makeRows(6), totalCount: 137, refetching: true });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the empty state',
    async () => {
      const { root } = await setup();
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the filtered-empty state',
    async () => {
      const { root } = await setup({ hasActiveFilters: true });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the error state',
    async () => {
      const { root } = await setup({ error: unhandledProblem() });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the permission-denied state',
    async () => {
      const { root } = await setup({ error: screenRightDeniedProblem() });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical violation in the business-rule state',
    async () => {
      const { root } = await setup({ error: businessRuleProblem() });
      expect(await critical(root)).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});

/**
 * Test 6, at the level it actually matters: route-bound, where **Clear
 * filters** has to reset the URL as well as the signals. Before M2-C05-03,
 * `queryParamsHandling: 'merge'` kept the cleared parameter in the address bar
 * and the next `queryParamMap` emission read it straight back.
 */
describe('DataGridQueryState.hasActiveFilters and clearFilters', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideLocationMocks(),
        provideRouter([{ path: 'rows', component: RouteGridHostComponent }]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('reports no active filter for a clean URL', async () => {
    const harness = await RouterTestingHarness.create('/rows');
    http.expectOne(() => true).flush(pageOf(makeRows(20), 1, 20, 400));
    const host = harness.routeDebugElement!.componentInstance as RouteGridHostComponent;

    expect(host.grid.hasActiveFilters()).toBe(false);
  });

  it('reports an active filter when one arrived in the URL', async () => {
    const harness = await RouterTestingHarness.create('/rows?code=C00');
    http.expectOne(() => true).flush(pageOf([], 1, 20, 0));
    const host = harness.routeDebugElement!.componentInstance as RouteGridHostComponent;

    expect(host.grid.hasActiveFilters()).toBe(true);
  });

  it('clears the query state, the URL and refetches', async () => {
    const harness = await RouterTestingHarness.create('/rows?page=3&code=C00');
    http.expectOne(() => true).flush(pageOf([], 3, 20, 0));
    const host = harness.routeDebugElement!.componentInstance as RouteGridHostComponent;

    host.grid.clearFilters();
    await harness.fixture.whenStable();

    const url = TestBed.inject(Router).url;
    expect(url).not.toContain('code=');
    expect(url).not.toContain('page=3');
    expect(host.grid.hasActiveFilters()).toBe(false);
    expect(host.grid.page()).toBe(1);

    const request = http.expectOne((candidate) => candidate.url === TEST_ENDPOINT);
    expect(request.request.params.get('code')).toBeNull();
    request.flush(pageOf(makeRows(20), 1, 20, 400));
  });
});
