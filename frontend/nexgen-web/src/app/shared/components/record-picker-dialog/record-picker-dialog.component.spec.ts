import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { afterEach, beforeAll, describe, expect, it, vi } from 'vitest';

import {
  PICKER_ENDPOINT,
  PICKER_EXPORT_ENDPOINT,
  PickerHostComponent,
  installPickerJsdomSupport,
  makePickerRows,
  pickerCellState,
  pickerExportRequest,
  pickerPage,
  type PickerRow,
} from './test-fixtures';

/**
 * `RecordPickerDialog` behaviour, against the M2-C06 test list.
 *
 * Everything here goes through `HttpTestingController`: if an assertion can be
 * satisfied without a request having been made, the dialog is filtering
 * client-side and the whole point of the replacement is lost.
 */

const PAGE_SIZE = 5;
const TOTAL = 20;

interface Rendered {
  fixture: Awaited<ReturnType<typeof render<PickerHostComponent>>>['fixture'];
  host: PickerHostComponent;
  http: HttpTestingController;
  trigger: HTMLElement;
}

/**
 * `p-dialog` renders with `appendTo="body"`, outside the fixture's own subtree,
 * and a dialog left open when a test ends outlives Testing Library's cleanup.
 * Purging here keeps each test's queries unambiguous.
 */
function purgeStaleDialogs(): void {
  document.querySelectorAll('.p-dialog-mask, .p-dialog').forEach((node) => node.remove());
}

async function setup(): Promise<Rendered> {
  purgeStaleDialogs();
  const { fixture } = await render(PickerHostComponent, {
    providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
  });
  return {
    fixture,
    host: fixture.componentInstance,
    http: TestBed.inject(HttpTestingController),
    trigger: screen.getByRole('button', { name: 'Pull lines' }),
  };
}

/** Lets the debounce, the dialog animation and the post-render decoration run. */
async function settle(view: Rendered, ms = 25): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, ms));
  view.fixture.detectChanges();
  await view.fixture.whenStable();
}

function expectPageRequest(view: Rendered) {
  return view.http.expectOne((request) => request.url === PICKER_ENDPOINT);
}

async function flushPage(
  view: Rendered,
  page: number,
  rows: PickerRow[] = makePickerRows(PAGE_SIZE, (page - 1) * PAGE_SIZE),
): Promise<void> {
  expectPageRequest(view).flush(pickerPage(rows, page, PAGE_SIZE, TOTAL));
  await settle(view);
}

/** Opens the dialog and answers its first request. */
async function open(view: Rendered): Promise<void> {
  await userEvent.click(view.trigger);
  await screen.findByRole('dialog');
  await flushPage(view, 1);
}

function selectedText(): string {
  return screen.getByText(/^\d+ selected$/).textContent ?? '';
}

/**
 * The checkbox for the fixture row with this `id`. The label carries the ARIA
 * row index, which counts the header row - `pageOffset + row + 2`
 * (`grid-keyboard-navigation.ts:151-153`) - and the fixture numbers its rows
 * from 1, so the label is always the id plus one.
 */
function rowCheckbox(id: number): HTMLInputElement {
  return screen.getByLabelText(`Select row ${id + 1}`);
}

function selectAllCheckbox(): HTMLInputElement {
  const label = screen.queryByLabelText('Select all rows on this page');
  return (label ?? screen.getByLabelText('Deselect all rows on this page')) as HTMLInputElement;
}

describe('app-record-picker-dialog', () => {
  beforeAll(installPickerJsdomSupport);

  afterEach(() => {
    // The directory scan test renders nothing, so there is no controller to ask.
    TestBed.inject(HttpTestingController, null)?.verify();
  });

  // Test 1
  it('opens with focus in the search field, and Escape closes it and restores focus', async () => {
    const view = await setup();
    view.trigger.focus();

    await open(view);

    expect(document.activeElement).toBe(screen.getByLabelText('Search records'));

    await userEvent.keyboard('{Escape}');
    await settle(view);

    expect(screen.queryByRole('dialog')).toBeNull();
    expect(view.host.open()).toBe(false);
    expect(view.host.cancelledCount()).toBe(1);
    expect(document.activeElement).toBe(view.trigger);
  });

  // Test 2
  it('debounces typing into a server query and does not filter the result locally', async () => {
    const view = await setup();
    await open(view);

    await userEvent.type(screen.getByLabelText('Search records'), 'zzz');
    await settle(view);

    const request = expectPageRequest(view);
    expect(request.request.params.get('search')).toBe('zzz');
    expect(request.request.params.get('pageNumber')).toBe('1');

    // The server is authoritative about what matches: whatever it returns is
    // rendered, even a row whose visible text does not contain the term.
    request.flush(pickerPage(makePickerRows(2), 1, PAGE_SIZE, 2));
    await settle(view);

    expect(screen.getByText('Candidate 1')).toBeTruthy();
  });

  // Test 3
  it('keeps a selection made on page 1 while paging to page 3 and back', async () => {
    const view = await setup();
    await open(view);

    await userEvent.click(rowCheckbox(1));
    await userEvent.click(rowCheckbox(2));
    expect(selectedText()).toBe('2 selected');

    await userEvent.click(screen.getByRole('button', { name: 'Next page' }));
    await flushPage(view, 2);
    await userEvent.click(screen.getByRole('button', { name: 'Next page' }));
    await flushPage(view, 3);

    expect(selectedText()).toBe('2 selected');
    await userEvent.click(rowCheckbox(11));
    expect(selectedText()).toBe('3 selected');

    await userEvent.click(screen.getByRole('button', { name: 'First page' }));
    await flushPage(view, 1);

    expect(selectedText()).toBe('3 selected');
    expect(rowCheckbox(1).checked).toBe(true);
    expect(rowCheckbox(2).checked).toBe(true);
  });

  // Test 7
  it('displays the total selection count, and the total survives a page change', async () => {
    const view = await setup();
    await open(view);

    expect(selectedText()).toBe('0 selected');
    await userEvent.click(rowCheckbox(2));
    expect(selectedText()).toBe('1 selected');

    await userEvent.click(screen.getByRole('button', { name: 'Next page' }));
    await flushPage(view, 2);

    // Page 2 shows none of the selected rows, and the count still says one.
    expect(selectedText()).toBe('1 selected');
    await userEvent.click(rowCheckbox(7));
    expect(selectedText()).toBe('2 selected');
  });

  // Test 4
  it('keeps a selection across a search change', async () => {
    const view = await setup();
    await open(view);

    await userEvent.click(rowCheckbox(3));
    await userEvent.type(screen.getByLabelText('Search records'), 'a');
    await settle(view);
    expectPageRequest(view).flush(pickerPage(makePickerRows(2, 40), 1, PAGE_SIZE, 2));
    await settle(view);

    expect(selectedText()).toBe('1 selected');
  });

  // Test 6
  it('selects only the current page from the header checkbox, and says so', async () => {
    const view = await setup();
    await open(view);

    const header = selectAllCheckbox();
    expect(header.getAttribute('aria-label')).toBe('Select all rows on this page');
    expect(screen.getByRole('button', { name: 'Select all 5 on this page' })).toBeTruthy();

    await userEvent.click(rowCheckbox(1));
    expect(selectAllCheckbox().indeterminate).toBe(true);

    await userEvent.click(selectAllCheckbox());

    // Five, not the twenty the server says exist.
    expect(selectedText()).toBe('5 selected');
    expect(selectAllCheckbox().indeterminate).toBe(false);
  });

  // Test 8
  it('disables confirm with nothing selected and explains why', async () => {
    const view = await setup();
    await open(view);

    const confirm = screen.getByRole('button', { name: 'Add selected' });
    expect((confirm as HTMLButtonElement).disabled).toBe(true);

    const describedBy = confirm.getAttribute('aria-describedby');
    expect(describedBy).toBeTruthy();
    expect(document.getElementById(describedBy!)?.textContent).toContain(
      'Select at least one record',
    );

    await userEvent.click(rowCheckbox(1));

    expect(screen.getByRole<HTMLButtonElement>('button', { name: 'Add selected' }).disabled).toBe(
      false,
    );
  });

  // Test 9
  it('pre-selects the rows named by initialSelection', async () => {
    const view = await setup();
    view.host.initialSelection.set([2, 4]);

    await open(view);

    expect(selectedText()).toBe('2 selected');
    expect(rowCheckbox(2).checked).toBe(true);
    expect(rowCheckbox(4).checked).toBe(true);
    expect(rowCheckbox(1).checked).toBe(false);
  });

  // Test 10
  it('refuses disabled rows, marks them aria-disabled, and excludes them from select-all', async () => {
    const view = await setup();
    view.host.disabledRowIds.set([2]);

    await open(view);

    const disabled = rowCheckbox(2);
    expect(disabled.disabled).toBe(true);
    expect(disabled.closest('tr')?.getAttribute('aria-disabled')).toBe('true');
    expect(disabled.closest('tr')?.getAttribute('title')).toBe('Already added to this document');

    await userEvent.click(screen.getByRole('button', { name: 'Select all 4 on this page' }));

    expect(selectedText()).toBe('4 selected');
    expect(rowCheckbox(2).checked).toBe(false);
  });

  // Test 11
  it('single-select mode selects and confirms in one action', async () => {
    const view = await setup();
    view.host.selectionMode.set('single');

    await open(view);

    const row = screen.getByText('Candidate 3').closest('tr')!;
    row.dispatchEvent(new MouseEvent('dblclick', { bubbles: true }));
    await settle(view);

    expect(view.host.confirmedCount()).toBe(1);
    expect(view.host.confirmedRows().map((item) => item.id)).toEqual([3]);
    expect(view.host.open()).toBe(false);
  });

  // Test 12
  it('renders the permission-denied state for a 403 and stays open', async () => {
    const view = await setup();
    await userEvent.click(view.trigger);
    await screen.findByRole('dialog');

    expectPageRequest(view).flush(
      {
        status: 403,
        title: 'Screen right denied.',
        detail: "You do not have the 'view' right for the 'Sales Order' screen.",
        screen: 'Sales Order',
        right: 'view',
      },
      { status: 403, statusText: 'Forbidden' },
    );
    await settle(view);

    expect(screen.getByRole('dialog')).toBeTruthy();
    expect(screen.getByText(/Sales Order/)).toBeTruthy();
    expect(screen.getByText(/view/)).toBeTruthy();
  });

  // Test 13
  it('keeps the existing selection when a request fails', async () => {
    const view = await setup();
    await open(view);

    await userEvent.click(rowCheckbox(1));
    await userEvent.click(rowCheckbox(3));

    await userEvent.click(screen.getByRole('button', { name: 'Next page' }));
    await settle(view);
    expectPageRequest(view).flush(
      { status: 500, title: 'The report service is unavailable.' },
      { status: 500, statusText: 'Server Error' },
    );
    await settle(view);

    expect(screen.getByRole('dialog')).toBeTruthy();
    expect(selectedText()).toBe('2 selected');
    expect(screen.getByText('The report service is unavailable.')).toBeTruthy();
  });

  // Test 14
  it('exports through a server endpoint, generating no file locally', async () => {
    const view = await setup();
    view.host.exportRequest.set(pickerExportRequest(TestBed.inject(HttpClient)));

    await open(view);
    await userEvent.click(screen.getByRole('button', { name: 'Export' }));
    await settle(view);

    const request = view.http.expectOne((candidate) => candidate.url === PICKER_EXPORT_ENDPOINT);
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    request.flush(new Blob(['x']));
    await settle(view);
  });

  // Test 15
  it('never writes to the page URL', async () => {
    const view = await setup();
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate');
    const before = window.location.search;

    await open(view);
    await userEvent.type(screen.getByLabelText('Search records'), 'abc');
    await settle(view);
    expectPageRequest(view).flush(pickerPage(makePickerRows(1), 1, PAGE_SIZE, 1));
    await settle(view);

    expect(navigate).not.toHaveBeenCalled();
    expect(window.location.search).toBe(before);
  });

  // Cell call-outs, and that a tone never travels without words.
  it('renders a caller-supplied cell call-out as text, not as colour alone', async () => {
    const view = await setup();
    view.host.cellState.set(pickerCellState);

    await open(view);

    expect(screen.getAllByText('Above balance').length).toBeGreaterThan(0);
  });

  // Test 16
  it('names no domain field anywhere in the component directory', () => {
    // Assembled from fragments so this file does not itself match the scan.
    const banned = [
      ['item', 'code'].join(''),
      ['util', 'qty'].join(''),
      ['is', 'new', 'item'].join(''),
      ['is', 'qty', 'changed'].join(''),
    ];
    const directory = __dirname;
    const offences: string[] = [];

    for (const name of readdirSync(directory)) {
      if (name.endsWith('.spec.ts') || name === 'test-fixtures.ts') {
        continue;
      }
      const content = readFileSync(join(directory, name), 'utf8').toLowerCase();
      for (const term of banned) {
        if (content.includes(term)) {
          offences.push(`${name}: ${term}`);
        }
      }
    }

    expect(offences).toEqual([]);
  });
});
