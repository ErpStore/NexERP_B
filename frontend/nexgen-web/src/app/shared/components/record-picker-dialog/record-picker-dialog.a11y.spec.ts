import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import axe, { type Result } from 'axe-core';
import { afterEach, beforeAll, describe, expect, it } from 'vitest';

import {
  PICKER_ENDPOINT,
  PickerHostComponent,
  installPickerJsdomSupport,
  makePickerRows,
  pickerPage,
} from './test-fixtures';

/**
 * **Test 17.** A runtime `axe` scan with the dialog open, both empty and
 * populated - a closed dialog proves nothing.
 *
 * The scan runs against `document.body`, not the fixture element: `p-dialog`
 * uses `appendTo="body"`, so the dialog is not inside the host's DOM subtree.
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass remains M5-09's scope.
 */

/** A full axe pass in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;
const PAGE_SIZE = 5;

async function criticalViolations(): Promise<Result[]> {
  const results = await axe.run(document.body, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

async function openPicker(rows: number) {
  const { fixture } = await render(PickerHostComponent, {
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  await userEvent.click(screen.getByRole('button', { name: 'Pull lines' }));
  await screen.findByRole('dialog');
  http
    .expectOne((request) => request.url === PICKER_ENDPOINT)
    .flush(pickerPage(makePickerRows(rows), 1, PAGE_SIZE, rows));
  await new Promise((resolve) => setTimeout(resolve, 25));
  fixture.detectChanges();
  await fixture.whenStable();
  return { fixture, http };
}

describe('record picker dialog accessibility', () => {
  beforeAll(installPickerJsdomSupport);

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    document.documentElement.removeAttribute('data-theme');
  });

  it(
    'has zero critical axe violations with the dialog open and empty',
    { timeout: AXE_IS_SLOW },
    async () => {
      await openPicker(0);

      expect(await criticalViolations()).toEqual([]);
    },
  );

  it(
    'has zero critical axe violations with the dialog open and populated',
    { timeout: AXE_IS_SLOW },
    async () => {
      await openPicker(PAGE_SIZE);

      expect(await criticalViolations()).toEqual([]);
    },
  );

  it('labels the dialog and marks it modal', async () => {
    await openPicker(PAGE_SIZE);

    const dialog = screen.getByRole('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.textContent).toContain('Pending candidates');
  });

  it('announces the selection count politely', async () => {
    await openPicker(PAGE_SIZE);

    const live = screen.getByText(/^\d+ selected$/);
    expect(live.getAttribute('aria-live')).toBe('polite');
  });
});
