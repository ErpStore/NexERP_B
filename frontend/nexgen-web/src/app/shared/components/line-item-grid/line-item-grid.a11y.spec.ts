import { Component } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { render } from '@testing-library/angular';
import axe, { type Result } from 'axe-core';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideConfirmDialog } from '../overlay/confirm-dialog.service';
import { LineItemGridComponent } from './line-item-grid.component';
import { createTestRow, TEST_COLUMNS } from './test-fixtures';

/**
 * Test 21 (M2-C07 Testing Requirements). Carried over unchanged from the
 * specification this task replaces (Q-69, answered 2026-08-22): a runtime
 * `axe` scan is stack-independent and the template-lint rules do not
 * substitute for it. jsdom applies no stylesheet, so `color-contrast` is
 * disabled here for the same reason `data-grid.a11y.spec.ts` disables it -
 * covered elsewhere by computation, not by this scan.
 */

const AXE_IS_SLOW = 60_000;

@Component({
  selector: 'app-a11y-line-item-grid-host',
  imports: [ReactiveFormsModule, LineItemGridComponent],
  template: `
    <main>
      <h1>Purchase order lines</h1>
      <app-line-item-grid
        ariaLabel="Purchase order lines"
        [columns]="columns"
        [lines]="lines"
        [createRow]="createRow"
        [rowErrors]="errors"
      />
    </main>
  `,
})
class A11yLineItemGridHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly createRow = createTestRow;
  readonly lines = new FormArray<ReturnType<typeof createTestRow>>([
    createTestRow({ remarks: 'first line' }),
    createTestRow({ remarks: 'second line' }),
  ]);
  readonly errors = [{ rowId: 'row-1' as never, messages: ['Quantity must be greater than 0.'] }];
}

async function violations(root: HTMLElement, theme: 'light' | 'dark'): Promise<Result[]> {
  document.documentElement.setAttribute('data-theme', theme);
  const results = await axe.run(root, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('app-line-item-grid accessibility', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(() => {
    uninstallGridJsdomSupport();
    document.documentElement.removeAttribute('data-theme');
  });

  it(
    'reports no critical axe violation on a populated grid, with a row error, in either theme',
    async () => {
      const { fixture } = await render(A11yLineItemGridHostComponent, {
        providers: [provideConfirmDialog()],
      });
      fixture.detectChanges();
      await fixture.whenStable();
      const root = fixture.nativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
      expect(await violations(root, 'dark')).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical axe violation on the empty state (one auto-appended row)',
    async () => {
      const { fixture } = await render(A11yLineItemGridHostComponent, {
        providers: [provideConfirmDialog()],
        componentProperties: { lines: new FormArray<ReturnType<typeof createTestRow>>([]) },
      });
      fixture.detectChanges();
      await fixture.whenStable();
      const root = fixture.nativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});
