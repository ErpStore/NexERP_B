import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it, vi } from 'vitest';

import {
  PICKER_ENDPOINT,
  PickerHostComponent,
  installPickerJsdomSupport,
  makePickerRows,
  pickerPage,
} from './test-fixtures';

/**
 * **Test 5, and the reason this file exists on its own.**
 *
 * `DetailsModal.razor` returns the ticked rows in the sequence the user ticked
 * them, not in row order: `SelectionOrder` is assigned on each tick
 * (`:186-195`, `:205-213`) and the confirm path sorts by it (`:150-154`).
 * Downstream the callers append the returned rows in iteration order and then
 * renumber - `MfgPOUpsert.razor:4014` `foreach (var item in selectedItems)`,
 * `:4072` `Add(subVm)`, `:4077` `ResetSlno()` - so the ticking sequence *is*
 * the line order of the document being built.
 *
 * A rewrite that returns grid order looks correct in every screenshot and is
 * discovered when a customer receives a delivery challan with its lines in the
 * wrong sequence. Hence a dedicated file, so the assertion is hard to delete by
 * accident.
 */

/**
 * This test renders the whole dialog in jsdom and drives it through
 * `userEvent`; every interaction costs a full change-detection pass over the
 * `p-dialog` and the grid inside it, and the file was observed finishing at
 * 5293 ms against vitest's 5 s default - so whether it goes red is decided by
 * how loaded the machine is, not by anything it asserts.
 *
 * A timeout is a harness ceiling, not an assertion: raising it relaxes nothing.
 * The ordering expectation below still has to hold, and a genuine hang still
 * fails, just later. `record-picker-dialog.component.spec.ts` and
 * `record-picker-dialog.a11y.spec.ts:30-31` say the same thing.
 */
vi.setConfig({ testTimeout: 30_000 });

const PAGE_SIZE = 10;

describe('record picker selection order', () => {
  beforeAll(installPickerJsdomSupport);

  it('confirms rows in the order they were selected, not in grid order', async () => {
    const { fixture } = await render(PickerHostComponent, {
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const host = fixture.componentInstance;
    const http = TestBed.inject(HttpTestingController);

    await userEvent.click(screen.getByRole('button', { name: 'Pull lines' }));
    await screen.findByRole('dialog');
    http
      .expectOne((request) => request.url === PICKER_ENDPOINT)
      .flush(pickerPage(makePickerRows(PAGE_SIZE), 1, PAGE_SIZE, PAGE_SIZE));
    await new Promise((resolve) => setTimeout(resolve, 25));
    fixture.detectChanges();
    await fixture.whenStable();

    // The checkbox label carries the ARIA row index, which counts the header
    // row: `ariaRowIndex` is `pageOffset + row + 2`
    // (`grid-keyboard-navigation.ts:151-153`). Row 5, then row 2, then row 9.
    await userEvent.click(screen.getByLabelText('Select row 6'));
    await userEvent.click(screen.getByLabelText('Select row 3'));
    await userEvent.click(screen.getByLabelText('Select row 10'));

    await userEvent.click(screen.getByRole('button', { name: 'Add selected' }));
    await new Promise((resolve) => setTimeout(resolve, 25));
    fixture.detectChanges();

    expect(host.confirmedRows().map((row) => row.id)).toEqual([5, 2, 9]);

    http.verify();
  });
});
