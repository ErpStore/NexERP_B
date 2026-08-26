import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { DataGridToolbarComponent } from './data-grid-toolbar.component';
import { defaultDataGridState, type DataGridState } from './data-grid.model';
import { EXPORT_ROW_LIMIT_MESSAGE } from './fixtures/problem-details';
import type { GridExportOperation } from './grid-export.service';

const EXPORT_URL = '/api/v1/currencies/export';

const STATE: DataGridState = {
  ...defaultDataGridState(),
  sort: [{ field: 'currName', direction: 'asc' }],
  filters: { currName: 'acme' },
};

describe('DataGridToolbarComponent', () => {
  let http: HttpTestingController;

  async function setup() {
    const view = await render(DataGridToolbarComponent, {
      providers: [provideHttpClient(), provideHttpClientTesting()],
      inputs: {
        state: STATE,
        fallbackBaseName: 'currencies',
        exportOperation: ((query, format) =>
          TestBed.inject(HttpClient).get(EXPORT_URL, {
            params: { ...query, format },
            observe: 'response',
            responseType: 'blob',
          })) satisfies GridExportOperation,
      },
    });
    http = TestBed.inject(HttpTestingController);
    return { ...view, root: view.fixture.nativeElement as HTMLElement };
  }

  beforeEach(() => {
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);
    URL.createObjectURL = vi.fn(() => 'blob:test/0');
    URL.revokeObjectURL = vi.fn();
  });

  afterEach(() => vi.restoreAllMocks());

  /**
   * Excel only, because Excel is the only format the server produces
   * (`CurrencyExcelController.cs:48`; anything else is a 400 at `:100-104`). A
   * CSV entry here would be a button that always fails.
   */
  it('offers a single Export control by default, not a format menu', async () => {
    const { root } = await setup();

    expect(screen.getByRole('button', { name: 'Export' })).toBeDefined();
    expect(root.querySelector('p-menu')).toBeNull();

    http.verify();
  });

  it('issues one request carrying the current sort and filters', async () => {
    await setup();

    await userEvent.click(screen.getByRole('button', { name: 'Export' }));

    const request = http.expectOne((candidate) => candidate.url === EXPORT_URL);
    expect(request.request.params.get('sort')).toBe('currName');
    expect(request.request.params.get('currName')).toBe('acme');
    expect(request.request.params.get('format')).toBe('xlsx');

    request.flush(new Blob(['workbook']));
    http.verify();
  });

  it('disables the control and states the busy label while the export runs', async () => {
    const { fixture, root } = await setup();

    await userEvent.click(screen.getByRole('button', { name: 'Export' }));
    fixture.detectChanges();

    const button = root.querySelector<HTMLButtonElement>('button');
    expect(button?.disabled).toBe(true);
    expect(button?.getAttribute('aria-busy')).toBe('true');
    // Not a bare spinner: the busy state is announced in words.
    expect(root.querySelector('[role="status"]')?.textContent).toContain('Export in progress');

    http.expectOne(() => true).flush(new Blob(['workbook']));
    http.verify();
  });

  it('renders the server message when the export is refused', async () => {
    const { fixture, root } = await setup();

    await userEvent.click(screen.getByRole('button', { name: 'Export' }));
    http
      .expectOne(() => true)
      .flush(
        new Blob([JSON.stringify({ title: EXPORT_ROW_LIMIT_MESSAGE, status: 409 })], {
          type: 'application/problem+json',
        }),
        { status: 409, statusText: 'Conflict' },
      );
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(root.querySelector('.app-inline-alert__message')).not.toBeNull();
    });

    expect(root.querySelector('.app-inline-alert__message')?.textContent).toBe(
      EXPORT_ROW_LIMIT_MESSAGE,
    );
    http.verify();
  });
});
