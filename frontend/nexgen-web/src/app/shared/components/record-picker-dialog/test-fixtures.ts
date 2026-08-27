import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';

import type { DataGridColumn, DataGridPage, DataGridRowId, DataGridWireQuery } from '../data-grid';
import { RecordPickerDialogComponent } from './record-picker-dialog.component';
import type {
  RecordPickerCellStateFn,
  RecordPickerExportRequest,
  RecordPickerFetchPage,
  RecordPickerSelectionMode,
} from './record-picker-dialog.model';

/**
 * **Test fixtures only.** Nothing here is bundled into the application.
 *
 * This is the one file in the directory permitted to name a domain field: the
 * domain-leakage guard in `record-picker-dialog.component.spec.ts` scans every
 * other file for exactly that. The row shape is a generic candidate line, not a
 * copy of any real ERP entity.
 */

export interface PickerRow {
  id: number;
  code: string;
  description: string;
  quantity: number;
}

export const PICKER_ENDPOINT = '/api/v1/test-candidates';
export const PICKER_EXPORT_ENDPOINT = '/api/v1/test-candidates/export';

export const PICKER_COLUMNS: readonly DataGridColumn<PickerRow>[] = [
  { field: 'code', title: 'Code', width: '120px' },
  { field: 'description', title: 'Description', width: '240px' },
  { field: 'quantity', title: 'Quantity', numeric: true, width: '120px' },
];

export const pickerRowId = (row: PickerRow): DataGridRowId => row.id;

export function makePickerRows(count: number, offset = 0): PickerRow[] {
  return Array.from({ length: count }, (_, index) => {
    const id = offset + index + 1;
    return {
      id,
      code: `R${String(id).padStart(4, '0')}`,
      description: `Candidate ${id}`,
      quantity: id * 2,
    };
  });
}

export function pickerPage(
  rows: PickerRow[],
  page: number,
  pageSize: number,
  totalCount: number,
): DataGridPage<PickerRow> {
  return { items: rows, totalCount, pageNumber: page, pageSize };
}

/** Goes through the real `HttpClient`, so `HttpTestingController` sees the request. */
export function pickerFetchPage(http: HttpClient): RecordPickerFetchPage<PickerRow> {
  return (query: DataGridWireQuery) =>
    http.get<DataGridPage<PickerRow>>(PICKER_ENDPOINT, {
      params: query,
    });
}

/** The server produces the file; the dialog only downloads it (ADR-005). */
export function pickerExportRequest(http: HttpClient): RecordPickerExportRequest {
  return (query: DataGridWireQuery) =>
    http.get(PICKER_EXPORT_ENDPOINT, {
      params: query,
      responseType: 'blob',
    });
}

/**
 * A caller-supplied cell call-out. It names a *fixture* field, which is the
 * whole point: the component never does.
 */
export const pickerCellState: RecordPickerCellStateFn<PickerRow> = (row, field) =>
  field === 'quantity' && row.quantity > 6 ? { tone: 'warning', label: 'Above balance' } : null;

/** The host a real screen writes: it owns open/closed state and the result. */
@Component({
  selector: 'app-picker-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RecordPickerDialogComponent],
  template: `
    <button type="button" #trigger (click)="open.set(true)">Pull lines</button>

    <app-record-picker-dialog
      [visible]="open()"
      (visibleChange)="open.set($event)"
      [header]="header"
      [columns]="columns"
      [fetchPage]="fetchPage"
      [getRowId]="rowId"
      [selectionMode]="selectionMode()"
      [initialSelection]="initialSelection()"
      [disabledRowIds]="disabledRowIds()"
      [getCellState]="cellState()"
      [exportRequest]="exportRequest()"
      [searchDebounceMs]="5"
      [pageSize]="pageSize"
      confirmLabel="Add selected"
      (confirmed)="onConfirmed($event)"
      (cancelled)="cancelledCount.set(cancelledCount() + 1)"
    />
  `,
})
export class PickerHostComponent {
  readonly header = 'Pending candidates';
  readonly columns = PICKER_COLUMNS;
  readonly rowId = pickerRowId;
  readonly pageSize = 5;

  readonly open = signal(false);
  readonly selectionMode = signal<RecordPickerSelectionMode>('multiple');
  readonly initialSelection = signal<readonly DataGridRowId[]>([]);
  readonly disabledRowIds = signal<readonly DataGridRowId[]>([]);
  readonly cellState = signal<RecordPickerCellStateFn<PickerRow> | undefined>(undefined);
  readonly exportRequest = signal<RecordPickerExportRequest | undefined>(undefined);

  readonly confirmedRows = signal<readonly PickerRow[]>([]);
  readonly confirmedCount = signal(0);
  readonly cancelledCount = signal(0);

  readonly fetchPage = pickerFetchPage(inject(HttpClient));

  onConfirmed(rows: readonly PickerRow[]): void {
    this.confirmedRows.set(rows);
    this.confirmedCount.set(this.confirmedCount() + 1);
  }
}

/**
 * jsdom implements no `window.matchMedia`, and PrimeNG's overlay layer reads it
 * to decide whether a panel is modal.
 *
 * Duplicated from `overlay/jsdom-overlay-support.ts` rather than imported, for
 * the reason stated there: cross-importing a fixture couples two task-owned
 * directories, and promoting it to `angular.json`'s `setupFiles` is a
 * build-configuration change neither task owns. The duplication is recorded in
 * `docs/kb/risks/technical-debt-register.md`.
 */
export function installPickerJsdomSupport(): void {
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
  if (typeof window.requestAnimationFrame !== 'function') {
    window.requestAnimationFrame = (callback: FrameRequestCallback) =>
      window.setTimeout(() => callback(performance.now()), 0);
  }
}
