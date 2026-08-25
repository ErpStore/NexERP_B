import { Component, signal } from '@angular/core';
import { render } from '@testing-library/angular';
import axe, { type Result } from 'axe-core';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

import { DataGridComponent } from './data-grid.component';
import { defaultDataGridState } from './data-grid.model';
import {
  TEST_COLUMNS,
  installGridJsdomSupport,
  uninstallGridJsdomSupport,
  makeRows,
  testRowId,
  type TestRow,
} from './test-fixtures';

/**
 * Runtime accessibility scan over the grid, populated and empty, in both
 * themes. Carried over unchanged from the specification this task replaces:
 * a runtime scan is stack-independent, and `angular-eslint`'s template
 * accessibility rules do **not** substitute for it (Q-69, answered 2026-08-22
 * in the conservative direction).
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass is still M5-09's.
 */

/** An axe pass over a populated grid in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;

@Component({
  selector: 'app-a11y-grid-host',
  imports: [DataGridComponent],
  template: `
    <main>
      <h1>Currencies</h1>
      <app-data-grid
        ariaLabel="Currencies"
        [columns]="columns"
        [rows]="rows()"
        [totalCount]="total()"
        [state]="state"
        [getRowId]="rowId"
        selectionMode="multiple"
        [(selection)]="selection"
      />
    </main>
  `,
})
class A11yGridHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly rowId = testRowId;
  readonly state = defaultDataGridState();
  readonly rows = signal<readonly TestRow[]>(makeRows(12));
  readonly total = signal(137);
  readonly selection = signal<readonly TestRow[]>([]);
}

/**
 * Scans the **rendered root**, not `document.body`. R-76 records that spec
 * files here share one jsdom document and that an overlay left behind by
 * another file is still attached to `body`; scanning the whole document would
 * make this spec report that file's violations as this component's.
 */
async function violations(root: HTMLElement, theme: 'light' | 'dark'): Promise<Result[]> {
  document.documentElement.setAttribute('data-theme', theme);
  const results = await axe.run(root, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('app-data-grid accessibility', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(() => {
    uninstallGridJsdomSupport();
    document.documentElement.removeAttribute('data-theme');
  });

  it(
    'reports no critical axe violation on a populated grid, in either theme',
    async () => {
      const { fixture } = await render(A11yGridHostComponent);
      const root = fixture.nativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
      expect(await violations(root, 'dark')).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical axe violation on an empty grid, in either theme',
    async () => {
      const { fixture } = await render(A11yGridHostComponent);
      fixture.componentInstance.rows.set([]);
      fixture.componentInstance.total.set(0);
      fixture.detectChanges();
      await fixture.whenStable();
      const root = fixture.nativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
      expect(await violations(root, 'dark')).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});
