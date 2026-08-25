import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';

import { DataGridComponent } from './data-grid.component';
import type { DataGridDataSource } from './data-grid-query-state';
import { createDataGridQueryState } from './data-grid-query-state';
import type { DataGridPage, DataGridWireQuery } from './data-grid-query.adapter';
import type { DataGridColumn, DataGridRowIdFn, DataGridSelectionMode } from './data-grid.model';

/**
 * **Test fixtures only.** Nothing here is bundled into the application.
 *
 * The row shape mirrors the one paged endpoint that exists today,
 * `GET /api/v1/currencies` (`CurrencyController.cs:56-62`), so the fixtures
 * exercise the real contract rather than an invented one.
 */

export interface TestRow {
  id: number;
  code: string;
  name: string;
  rate: number;
  createdBy: string;
}

export const TEST_ENDPOINT = '/api/v1/test-rows';

export const TEST_COLUMNS: readonly DataGridColumn<TestRow>[] = [
  { field: 'code', title: 'Code', width: '120px', filter: 'text' },
  { field: 'name', title: 'Name', width: '240px', filter: 'text' },
  { field: 'rate', title: 'Rate', numeric: true, width: '120px' },
  { field: 'createdBy', title: 'Created by', width: '160px', priority: 'low' },
];

export const testRowId: DataGridRowIdFn<TestRow> = (row) => row.id;

export function makeRows(count: number, offset = 0): TestRow[] {
  return Array.from({ length: count }, (_, index) => {
    const id = offset + index + 1;
    return {
      id,
      code: `C${String(id).padStart(4, '0')}`,
      name: `Row ${id}`,
      rate: 1 + (id % 97) / 100,
      createdBy: `user${id % 7}`,
    };
  });
}

export function pageOf(
  rows: TestRow[],
  page: number,
  pageSize: number,
  totalCount: number,
): DataGridPage<TestRow> {
  return { items: rows, totalCount, pageNumber: page, pageSize };
}

/** A data source over the real `HttpClient`, so `HttpTestingController` sees the request. */
export function testDataSource(http: HttpClient): DataGridDataSource<TestRow> {
  return (query: DataGridWireQuery) =>
    http.get<DataGridPage<TestRow>>(TEST_ENDPOINT, {
      params: query as Record<string, string | number>,
    });
}

/** Route-bound host: the URL is the single source of truth. */
@Component({
  selector: 'app-route-grid-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DataGridComponent],
  template: `
    <app-data-grid
      [columns]="columns"
      [rows]="grid.rows()"
      [totalCount]="grid.totalCount()"
      [state]="grid.state()"
      [loading]="grid.loading()"
      [refetching]="grid.refetching()"
      [error]="grid.error()"
      [getRowId]="rowId"
      [selectionMode]="selectionMode()"
      [(selection)]="selection"
      [filterDebounceMs]="filterDebounceMs"
      (stateChange)="grid.apply($event)"
      (rowActivate)="activated.set($event)"
    />
  `,
})
export class RouteGridHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly rowId = testRowId;
  readonly filterDebounceMs = 10;
  readonly selectionMode = signal<DataGridSelectionMode>('multiple');
  readonly selection = signal<readonly TestRow[]>([]);
  readonly activated = signal<TestRow | null>(null);
  readonly grid = createDataGridQueryState<TestRow>({
    source: testDataSource(inject(HttpClient)),
    mode: 'route',
    filterNames: ['code', 'name'],
    filterDebounceMs: 10,
  });
}

/** Detached host: the same signals, and the URL is never written (M2-C06). */
@Component({
  selector: 'app-detached-grid-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DataGridComponent],
  template: `
    <app-data-grid
      [columns]="columns"
      [rows]="grid.rows()"
      [totalCount]="grid.totalCount()"
      [state]="grid.state()"
      [loading]="grid.loading()"
      [refetching]="grid.refetching()"
      [error]="grid.error()"
      [getRowId]="rowId"
      [filterDebounceMs]="filterDebounceMs"
      (stateChange)="grid.apply($event)"
    />
  `,
})
export class DetachedGridHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly rowId = testRowId;
  readonly filterDebounceMs = 10;
  readonly grid = createDataGridQueryState<TestRow>({
    source: testDataSource(inject(HttpClient)),
    mode: 'detached',
    filterNames: ['code', 'name'],
    filterDebounceMs: 10,
  });
}

/**
 * jsdom applies no stylesheet and lays nothing out, so every element measures
 * zero. PrimeNG's virtual scroller sizes its window from those measurements and
 * would render nothing at all, which would make the virtualisation test pass
 * for the wrong reason. These stubs give the scroller a 600 px viewport to
 * reason about - stated here rather than hidden, because it is the reason the
 * *frame-rate* half of the measurement had to be taken in a real browser
 * instead (KB-050 Performance targets).
 */
const patched: (() => void)[] = [];

export function installGridJsdomSupport(): void {
  if (typeof window.matchMedia !== 'function') {
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      configurable: true,
      value: (query: string): MediaQueryList =>
        ({
          matches: false,
          media: query,
          onchange: null,
          addListener: () => undefined,
          removeListener: () => undefined,
          addEventListener: () => undefined,
          removeEventListener: () => undefined,
          dispatchEvent: () => false,
        }) as MediaQueryList,
    });
  }

  patchComputedStyle();
  patchOffsetParent();
  defineMetric('offsetHeight', 600);
  defineMetric('clientHeight', 600);
  defineMetric('offsetWidth', 1200);
  defineMetric('clientWidth', 1200);

  if (typeof window.requestAnimationFrame !== 'function') {
    window.requestAnimationFrame = (callback: FrameRequestCallback) =>
      window.setTimeout(() => callback(performance.now()), 0);
  }
}

/**
 * Undoes {@link installGridJsdomSupport}. Call it from `afterAll`: the patches
 * are on `HTMLElement.prototype` and `window`, and vitest may reuse the same
 * jsdom environment for the next spec file - where a `<p-select>` that suddenly
 * believes it has been laid out behaves differently from one that knows it has
 * not.
 */
export function uninstallGridJsdomSupport(): void {
  while (patched.length > 0) {
    patched.pop()?.();
  }
}

/**
 * jsdom implements no `offsetParent`, and PrimeNG's `isVisible` is
 * `!!(el && el.offsetParent != null)` (`@primeuix/utils` `dom/index.mjs:13`).
 * Without this, the virtual scroller decides it is hidden and never
 * initialises, so the grid renders zero rows and the virtualisation test would
 * pass for entirely the wrong reason.
 */
function patchOffsetParent(): void {
  // jsdom *does* define the property - it just always answers `null` - so the
  // guard is a marker, not the presence of a getter.
  const marker = window as unknown as { __gridOffsetParentPatched?: boolean };
  if (marker.__gridOffsetParentPatched) {
    return;
  }
  marker.__gridOffsetParentPatched = true;
  const original = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'offsetParent');
  patched.push(() => {
    marker.__gridOffsetParentPatched = false;
    if (original) {
      Object.defineProperty(HTMLElement.prototype, 'offsetParent', original);
    }
  });
  Object.defineProperty(HTMLElement.prototype, 'offsetParent', {
    configurable: true,
    get(this: HTMLElement): Element | null {
      return this.isConnected ? this.ownerDocument.body : null;
    },
  });
}

/**
 * jsdom's computed style returns `''` for any property no stylesheet sets, and
 * PrimeNG's scroller does `parseFloat(style.paddingTop)` on it - which is
 * `NaN`, which silently collapses its viewport calculation to zero items. The
 * defaults below are what a browser would have returned.
 */
function patchComputedStyle(): void {
  const marker = window as unknown as { __gridComputedStylePatched?: boolean };
  if (marker.__gridComputedStylePatched) {
    return;
  }
  marker.__gridComputedStylePatched = true;
  const nativeGetComputedStyle = window.getComputedStyle;
  patched.push(() => {
    marker.__gridComputedStylePatched = false;
    window.getComputedStyle = nativeGetComputedStyle;
  });
  const original = window.getComputedStyle.bind(window);
  const ZERO_DEFAULTS = new Set([
    'paddingLeft',
    'paddingRight',
    'paddingTop',
    'paddingBottom',
    'left',
    'right',
    'top',
    'bottom',
  ]);
  window.getComputedStyle = (element: Element, pseudo?: string | null) => {
    const style = original(element, pseudo ?? undefined);
    return new Proxy(style, {
      get(target, property) {
        const value: unknown = Reflect.get(target, property, target);
        if (typeof value === 'function') {
          return (value as (...args: unknown[]) => unknown).bind(target);
        }
        if (typeof property === 'string' && ZERO_DEFAULTS.has(property) && value === '') {
          return '0px';
        }
        return value;
      },
    });
  };
}

function defineMetric(
  property: 'offsetHeight' | 'clientHeight' | 'offsetWidth' | 'clientWidth',
  value: number,
): void {
  const existing = Object.getOwnPropertyDescriptor(HTMLElement.prototype, property);
  if (existing?.get && (existing.get as { patched?: boolean }).patched) {
    return;
  }
  const getter = function (this: HTMLElement): number {
    // Only the table's own scroll chain needs a size; giving every element one
    // confuses PrimeNG's overlay positioning in the other specs.
    return /virtualscroller|datatable/.test(this.className) ? value : 0;
  };
  (getter as { patched?: boolean }).patched = true;
  patched.push(() => {
    if (existing) {
      Object.defineProperty(HTMLElement.prototype, property, existing);
    } else {
      delete (HTMLElement.prototype as unknown as Record<string, unknown>)[property];
    }
  });
  Object.defineProperty(HTMLElement.prototype, property, { configurable: true, get: getter });
}
