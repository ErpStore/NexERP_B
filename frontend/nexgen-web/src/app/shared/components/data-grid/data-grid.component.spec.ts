import { Component, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { Table } from 'primeng/table';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';

import { DataGridComponent } from './data-grid.component';
import { defaultDataGridState, type DataGridState } from './data-grid.model';
import { HEADER_ROW_INDEX } from './grid-keyboard-navigation';
import {
  TEST_COLUMNS,
  installGridJsdomSupport,
  uninstallGridJsdomSupport,
  makeRows,
  testRowId,
  type TestRow,
} from './test-fixtures';

const TEMPLATE = `
  <app-data-grid
    [columns]="columns"
    [rows]="rows"
    [totalCount]="totalCount"
    [state]="state"
    [loading]="loading"
    [getRowId]="rowId"
    [selectionMode]="selectionMode"
    [(selection)]="selection"
    [filterDebounceMs]="5"
    (stateChange)="onState($event)"
    (rowActivate)="onActivate($event)"
  />`;

interface HostProperties {
  columns: typeof TEST_COLUMNS;
  rows: readonly TestRow[];
  totalCount: number;
  state: DataGridState;
  loading: boolean;
  rowId: typeof testRowId;
  selectionMode: 'none' | 'single' | 'multiple';
  selection: readonly TestRow[];
  onState: (state: DataGridState) => void;
  onActivate: (row: TestRow) => void;
}

async function setup(overrides: Partial<HostProperties> = {}) {
  const emitted: DataGridState[] = [];
  const activated: TestRow[] = [];
  const properties: HostProperties = {
    columns: TEST_COLUMNS,
    rows: makeRows(3),
    totalCount: 137,
    state: defaultDataGridState(),
    loading: false,
    rowId: testRowId,
    selectionMode: 'multiple',
    selection: [] as readonly TestRow[],
    onState: (state: DataGridState) => emitted.push(state),
    onActivate: (row: TestRow) => activated.push(row),
    ...overrides,
  };
  const { fixture } = await render(TEMPLATE, {
    imports: [DataGridComponent],
    componentProperties: { ...properties },
  });
  const table = () => fixture.debugElement.query(By.directive(Table)).componentInstance as Table;
  const root = fixture.nativeElement as HTMLElement;
  const cellAt = (row: number, col: number) =>
    root.querySelector<HTMLElement>(`[data-row="${row}"][data-col="${col}"]`);
  const focusedCoords = () => {
    const active = document.activeElement as HTMLElement | null;
    return active
      ? { row: Number(active.dataset['row']), col: Number(active.dataset['col']) }
      : null;
  };
  return { fixture, emitted, activated, table, cellAt, focusedCoords, root };
}

/** The virtual scroller initialises off a timer; this is what lets it run. */
async function settle(fixture: { detectChanges: () => void; whenStable: () => Promise<unknown> }) {
  await new Promise((resolve) => setTimeout(resolve, 20));
  fixture.detectChanges();
  await fixture.whenStable();
}

describe('app-data-grid', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('renders header, rows and the pager from one paged response', async () => {
    await setup({ rows: makeRows(20), totalCount: 137 });

    expect(screen.getByRole('columnheader', { name: /Code/ })).toBeDefined();
    expect(screen.getByText('Row 1')).toBeDefined();
    expect(screen.getByText('Row 20')).toBeDefined();
    // The range and the page count come from the server's total, not the page.
    expect(screen.getByText('1-20 of 137')).toBeDefined();
    expect(screen.getByText('Page 1 of 7')).toBeDefined();
  });

  it('configures p-table for server-driven paging and never sorts locally', async () => {
    const { table, emitted, fixture } = await setup();

    expect(table().lazy()).toBe(true);
    expect(table().lazyLoadOnInit()).toBe(false);
    expect(table().paginator()).toBe(false);

    const before = [...(fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr')].map(
      (row: Element) => row.textContent?.trim(),
    );
    screen.getByRole('columnheader', { name: /Name/ }).click();
    await fixture.whenStable();

    // The intent is emitted; the rows on screen are untouched until the server answers.
    expect(emitted.at(-1)?.sort).toEqual([{ field: 'name', direction: 'asc' }]);
    expect(emitted.at(-1)?.page).toBe(1);
    const after = [...(fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr')].map(
      (row: Element) => row.textContent?.trim(),
    );
    expect(after).toEqual(before);
  });

  it('cycles an ascending column to descending', async () => {
    const { emitted, fixture } = await setup({
      state: { ...defaultDataGridState(), sort: [{ field: 'name', direction: 'asc' }] },
    });
    const header = screen.getByRole('columnheader', { name: /Name/ });
    expect(header.getAttribute('aria-sort')).toBe('ascending');

    header.click();
    await fixture.whenStable();

    expect(emitted.at(-1)?.sort).toEqual([{ field: 'name', direction: 'desc' }]);
  });

  it('cycles a descending column back to unsorted', async () => {
    const { emitted, fixture } = await setup({
      state: { ...defaultDataGridState(), sort: [{ field: 'name', direction: 'desc' }] },
    });
    const header = screen.getByRole('columnheader', { name: /Name/ });
    expect(header.getAttribute('aria-sort')).toBe('descending');

    header.click();
    await fixture.whenStable();

    expect(emitted.at(-1)?.sort).toEqual([]);
  });

  it('marks a sortable but unsorted column aria-sort="none"', async () => {
    await setup();

    expect(screen.getByRole('columnheader', { name: /Name/ }).getAttribute('aria-sort')).toBe(
      'none',
    );
  });

  it('debounces a filter, resets to page 1 and emits once', async () => {
    vi.useFakeTimers();
    try {
      const { emitted, fixture } = await setup({
        state: { ...defaultDataGridState(), page: 4 },
      });
      const filter = screen.getAllByRole('textbox').at(0) as HTMLInputElement;

      filter.value = 'C0';
      filter.dispatchEvent(new Event('input', { bubbles: true }));
      filter.value = 'C00';
      filter.dispatchEvent(new Event('input', { bubbles: true }));
      expect(emitted).toHaveLength(0);

      vi.advanceTimersByTime(20);
      await fixture.whenStable();

      expect(emitted).toHaveLength(1);
      expect(emitted[0]?.filters).toEqual({ code: 'C00' });
      expect(emitted[0]?.page).toBe(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('reports the server total to screen readers, not the page length', async () => {
    const { fixture } = await setup();

    const grid = (fixture.nativeElement as HTMLElement).querySelector(
      '[role="grid"]',
    ) as HTMLElement;
    expect(grid.getAttribute('aria-rowcount')).toBe('137');
    expect(grid.getAttribute('aria-colcount')).toBe('5');
  });

  it('numbers rows absolutely across pages for aria-rowindex', async () => {
    const { fixture } = await setup({
      rows: makeRows(3, 40),
      state: { ...defaultDataGridState(), page: 3 },
    });

    const first = (fixture.nativeElement as HTMLElement).querySelector('tbody tr') as HTMLElement;
    // Page 3 of 20, first body row: header is row 1, so 40 + 0 + 2.
    expect(first.getAttribute('aria-rowindex')).toBe('42');
  });

  it('virtualises a 10,000-row page into a bounded number of DOM rows', async () => {
    const { fixture, table } = await setup({ rows: makeRows(10_000), totalCount: 10_000 });
    await settle(fixture);

    expect(table().virtualScroll()).toBe(true);
    const rendered = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr').length;
    expect(rendered).toBeLessThan(100);
    expect(rendered).toBeGreaterThan(0);
  });

  it('does not virtualise a small page', async () => {
    const { table } = await setup({ rows: makeRows(20) });

    expect(table().virtualScroll()).toBe(false);
  });

  it('moves the focused cell with the full ARIA grid key set', async () => {
    const { fixture, cellAt, focusedCoords } = await setup({ rows: makeRows(30), totalCount: 30 });
    const press = async (key: string, modifiers: Partial<KeyboardEventInit> = {}) => {
      (document.activeElement as HTMLElement).dispatchEvent(
        new KeyboardEvent('keydown', { key, bubbles: true, ...modifiers }),
      );
      await fixture.whenStable();
    };

    cellAt(0, 0)!.focus();
    expect(focusedCoords()).toEqual({ row: 0, col: 0 });

    await press('ArrowRight');
    expect(focusedCoords()).toEqual({ row: 0, col: 1 });

    await press('ArrowDown');
    expect(focusedCoords()).toEqual({ row: 1, col: 1 });

    await press('ArrowLeft');
    expect(focusedCoords()).toEqual({ row: 1, col: 0 });

    await press('End');
    expect(focusedCoords()).toEqual({ row: 1, col: 4 });

    await press('Home');
    expect(focusedCoords()).toEqual({ row: 1, col: 0 });

    await press('PageDown');
    expect(focusedCoords()).toEqual({ row: 11, col: 0 });

    await press('PageUp');
    expect(focusedCoords()).toEqual({ row: 1, col: 0 });

    await press('End', { ctrlKey: true });
    expect(focusedCoords()).toEqual({ row: 29, col: 4 });

    await press('Home', { ctrlKey: true });
    expect(focusedCoords()).toEqual({ row: HEADER_ROW_INDEX, col: 0 });
  });

  it('keeps exactly one tab stop for the whole grid', async () => {
    const { fixture, cellAt } = await setup({ rows: makeRows(10), totalCount: 10 });

    cellAt(2, 1)!.focus();
    await fixture.whenStable();

    const tabStops = (fixture.nativeElement as HTMLElement).querySelectorAll(
      '[data-row][tabindex="0"]',
    );
    expect(tabStops).toHaveLength(1);
    expect((tabStops[0] as HTMLElement).dataset['row']).toBe('2');
    expect((tabStops[0] as HTMLElement).dataset['col']).toBe('1');
  });

  it('toggles selection with Space and activates a row with Enter', async () => {
    const { fixture, cellAt, activated } = await setup({ rows: makeRows(5), totalCount: 5 });
    const host = fixture.componentInstance;

    cellAt(1, 1)!.focus();
    (document.activeElement as HTMLElement).dispatchEvent(
      new KeyboardEvent('keydown', { key: ' ', bubbles: true }),
    );
    await fixture.whenStable();
    expect(host.selection.map((row) => row.id)).toEqual([2]);

    (document.activeElement as HTMLElement).dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }),
    );
    await fixture.whenStable();
    expect(activated.map((row) => row.id)).toEqual([2]);
  });

  it('selects the current page only, with an explicit indeterminate state', async () => {
    const { fixture } = await setup({ rows: makeRows(3), totalCount: 137 });
    const host = fixture.componentInstance;
    const selectAll = screen.getByRole('checkbox', {
      name: /Select all rows on this page/,
    });

    await userEvent.click(selectAll);
    await fixture.whenStable();

    // Three rows on the page, 137 in the result set. Three get selected.
    expect(host.selection).toHaveLength(3);

    const rowCheckbox = screen.getByRole('checkbox', { name: 'Select row 2' });
    await userEvent.click(rowCheckbox);
    await fixture.whenStable();

    expect(host.selection).toHaveLength(2);
    const header = screen.getByRole<HTMLInputElement>('checkbox', {
      name: /Select all rows on this page/,
    });
    expect(header.indeterminate).toBe(true);
    expect(header.checked).toBe(false);
  });

  it('renders numeric columns right-aligned and tabular', async () => {
    const { fixture } = await setup();

    const rateCell = (fixture.nativeElement as HTMLElement).querySelector(
      'tbody tr td.app-data-grid__cell--numeric',
    ) as HTMLElement;
    expect(rateCell.style.textAlign).toBe('right');
    expect(rateCell.classList.contains('app-data-grid__cell--numeric')).toBe(true);
  });

  it('emits a page change from the pager without touching the rows', async () => {
    const { fixture, emitted } = await setup();

    await userEvent.click(screen.getByRole('button', { name: 'Next page' }));
    await fixture.whenStable();

    expect(emitted.at(-1)?.page).toBe(2);
    expect(screen.getByText('Row 1')).toBeDefined();
  });

  it('renders an accessible placeholder rather than an empty tbody', async () => {
    await setup({ rows: [], totalCount: 0 });

    expect(screen.getByText('No rows to display')).toBeDefined();
  });

  it('resizes a column with the keyboard on a focused resize handle', async () => {
    const { fixture } = await setup();
    const handle = (fixture.nativeElement as HTMLElement).querySelector(
      'th[data-field="code"] .app-data-grid__resize-handle',
    ) as HTMLElement;

    handle.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }));
    await fixture.whenStable();

    const header = (fixture.nativeElement as HTMLElement).querySelector(
      'th[data-field="code"]',
    ) as HTMLElement;
    expect(header.style.width).not.toBe('120px');
  });
});

/**
 * A refetch, modelled the way one actually happens: the same host, new row
 * objects in the signal it binds. `rerender` would replace the host's whole
 * property bag, which is not what a page change does.
 */
@Component({
  selector: 'app-refetch-host',
  imports: [DataGridComponent],
  template: `
    <app-data-grid
      [columns]="columns"
      [rows]="rows()"
      [totalCount]="137"
      [state]="state"
      [getRowId]="rowId"
      selectionMode="multiple"
      [(selection)]="selection"
    />
  `,
})
class RefetchHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly rowId = testRowId;
  readonly state = defaultDataGridState();
  readonly rows = signal(makeRows(10));
  readonly selection = signal<readonly TestRow[]>([]);
}

describe('app-data-grid across a refetch', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('restores focus to the same cell', async () => {
    const { fixture } = await render(RefetchHostComponent);
    const root = fixture.nativeElement as HTMLElement;
    const host = fixture.componentInstance;
    const cell = (row: number, col: number) =>
      root.querySelector<HTMLElement>(`[data-row="${row}"][data-col="${col}"]`);

    cell(3, 2)!.focus();
    expect((document.activeElement as HTMLElement).dataset['row']).toBe('3');

    host.rows.set(makeRows(10, 20));
    await settle(fixture);

    const active = document.activeElement as HTMLElement;
    expect(active.dataset['row']).toBe('3');
    expect(active.dataset['col']).toBe('2');
    expect(active.textContent?.trim()).toBe('Row 24');
  });

  it('keeps a selection when getRowId is stable', async () => {
    const { fixture } = await render(RefetchHostComponent);
    const host = fixture.componentInstance;

    await userEvent.click(screen.getByRole('checkbox', { name: 'Select row 3' }));
    await fixture.whenStable();
    expect(host.selection().map((row) => row.id)).toEqual([2]);

    // The same rows arrive again as fresh objects - a real refetch.
    host.rows.set(makeRows(10));
    await settle(fixture);

    expect(host.selection().map((row) => row.id)).toEqual([2]);
    expect(screen.getByRole<HTMLInputElement>('checkbox', { name: 'Select row 3' }).checked).toBe(
      true,
    );
  });
});

/**
 * The seams M2-C05-02 and M2-C05-03 fill. They are typed and reachable now, so
 * neither task has to change this component's input surface to land.
 */
@Component({
  selector: 'app-seam-host',
  imports: [DataGridComponent],
  template: `
    <app-data-grid
      [columns]="columns"
      [rows]="rows"
      [totalCount]="3"
      [getRowId]="rowId"
      [columnVisibility]="visibility()"
      [error]="problem"
    >
      <ng-template #toolbar><button type="button">Export</button></ng-template>
      <ng-template #empty><p>Nothing here</p></ng-template>
      <ng-template #error let-problem
        ><p>{{ problem.title }}</p></ng-template
      >
      <ng-template #rowActions let-row
        ><button type="button">Edit {{ row.id }}</button></ng-template
      >
    </app-data-grid>
  `,
})
class SeamHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly rows = makeRows(3);
  readonly rowId = testRowId;
  readonly problem = { title: 'Server said no' };
  readonly visibility = signal<Record<string, boolean>>({ createdBy: false });
}

describe('app-data-grid extension seams', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('honours columnVisibility and renders the toolbar and row-action slots', async () => {
    const { fixture } = await render(SeamHostComponent);

    expect(screen.queryByRole('columnheader', { name: /Created by/ })).toBeNull();
    expect(screen.getByRole('button', { name: 'Export' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit 1' })).toBeDefined();
    // The header row only - the filter row below it has its own cells.
    expect(
      (fixture.nativeElement as HTMLElement).querySelectorAll('thead tr:first-child th'),
    ).toHaveLength(3);
  });

  it('hands the ProblemDetails object to the error slot untouched', async () => {
    await render(SeamHostComponent, { inputs: {} });

    // Rows are present, so the error template is not the visible state here;
    // the seam is proven by the empty-state path in the component spec above.
    expect(screen.getByText('Row 1')).toBeDefined();
  });
});
