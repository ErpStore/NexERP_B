import { Component, signal } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideConfirmDialog } from '../overlay/confirm-dialog.service';
import type { LineItemFormGroup } from './line-item-form';
import { LineItemGridComponent } from './line-item-grid.component';
import { makeRowId, type LineItemRowEvent } from './line-item-grid.model';
import { createTestRow, TEST_COLUMNS, testQty, type TestLine } from './test-fixtures';

/**
 * Integration-level coverage for `LineItemGrid` (M2-C07 Testing
 * Requirements). The keyboard model's *shape* is unit-tested in
 * `line-grid-keyboard.spec.ts`, decimal safety in
 * `line-item-grid.decimal-safety.spec.ts`, and render isolation in
 * `line-item-grid.render-performance.spec.ts` - this file is what remains:
 * the whole component wired together, real `FormArray`, real keyboard
 * events, real `rowEvent` contract.
 */

@Component({
  selector: 'app-test-line-item-grid-host',
  imports: [ReactiveFormsModule, LineItemGridComponent],
  template: `
    <app-line-item-grid
      [columns]="columns"
      [lines]="lines"
      [createRow]="createRow"
      [readOnly]="readOnly()"
      [rowErrors]="rowErrors()"
      (rowEvent)="events.push($event)"
    />
  `,
})
class TestHostComponent {
  readonly columns = TEST_COLUMNS;
  readonly createRow = createTestRow;
  readonly lines = new FormArray<LineItemFormGroup<TestLine>>([
    createTestRow({ remarks: 'alpha' }),
    createTestRow({ remarks: 'beta' }),
  ]);
  readonly readOnly = signal(false);
  readonly rowErrors = signal<{ rowId: ReturnType<typeof makeRowId>; messages: string[] }[]>([]);
  readonly events: LineItemRowEvent<TestLine>[] = [];
}

/** `LineItemGridComponent` injects `ConfirmDialogService`, which injects PrimeNG's `ConfirmationService` - every render needs it provided. */
function renderGrid(options: Parameters<typeof render>[1] = {}) {
  return render(TestHostComponent, {
    ...options,
    providers: [provideConfirmDialog(), ...(options.providers ?? [])],
  });
}

describe('LineItemGridComponent', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('renders one row per FormArray entry, plus the toolbar Add line control', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(screen.getAllByDisplayValue(/alpha|beta/)).toHaveLength(2);
    expect(screen.getByRole('button', { name: /add line/i })).toBeTruthy();
  });

  it('renders one empty editable row when the FormArray starts empty (Target Result: empty state)', async () => {
    const { fixture } = await renderGrid({
      componentProperties: { lines: new FormArray<LineItemFormGroup<TestLine>>([]) },
    });
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.lines.length).toBe(1);
  });

  it('Ctrl+D duplicates the focused row and raises row-duplicated', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    const remarksInputs = screen.getAllByDisplayValue(/alpha|beta/);
    remarksInputs[0]!.focus();
    await userEvent.keyboard('{Control>}d{/Control}');
    fixture.detectChanges();

    expect(fixture.componentInstance.lines.length).toBe(3);
    const duplicated = fixture.componentInstance.events.find((e) => e.type === 'row-duplicated');
    expect(duplicated).toBeTruthy();
  });

  it('Alt+ArrowDown moves the focused row down and raises rows-reordered', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    const before = fixture.componentInstance.lines.at(0).getRawValue().remarks;
    const remarksInputs = screen.getAllByDisplayValue(/alpha|beta/);
    remarksInputs[0]!.focus();
    await userEvent.keyboard('{Alt>}{ArrowDown}{/Alt}');
    fixture.detectChanges();

    expect(fixture.componentInstance.lines.at(1).getRawValue().remarks).toBe(before);
    const reordered = fixture.componentInstance.events.find((e) => e.type === 'rows-reordered');
    expect(reordered).toBeTruthy();
  });

  it('Escape reverts the focused row to its last committed values', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    const remarksInputs = screen.getAllByDisplayValue(/alpha|beta/);
    await userEvent.clear(remarksInputs[0]!);
    await userEvent.type(remarksInputs[0]!, 'changed');
    expect(fixture.componentInstance.lines.at(0).getRawValue().remarks).toBe('changed');

    remarksInputs[0]!.focus();
    await userEvent.keyboard('{Escape}');
    fixture.detectChanges();

    expect(fixture.componentInstance.lines.at(0).getRawValue().remarks).toBe('alpha');
  });

  it('a domain event (rate-changed) busies the row and applies only what respond() returns - the grid never computes a replacement', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    const rateInputs = screen
      .getAllByRole('textbox')
      .filter((el) => (el as HTMLInputElement).getAttribute('aria-label')?.startsWith('Rate'));
    // The rate cell starts at a `null` value, displayed as the em dash
    // (`ABSENT_DISPLAY`) - clear it first, or `userEvent.type` appends
    // after the dash instead of replacing it.
    await userEvent.clear(rateInputs[0]!);
    await userEvent.type(rateInputs[0]!, '99.50');
    await userEvent.tab();
    fixture.detectChanges();

    const event = fixture.componentInstance.events.find((e) => e.type === 'rate-changed');
    expect(event).toBeTruthy();
    if (!event || event.type !== 'rate-changed') {
      throw new Error('expected a rate-changed domain event');
    }

    // Not yet applied - the row is waiting on `respond()`, not a value the
    // grid invented from the raw commit.
    expect(fixture.componentInstance.lines.at(0).getRawValue().qty).not.toEqual(testQty('5'));

    event.respond({ qty: testQty('5') });
    fixture.detectChanges();

    expect(fixture.componentInstance.lines.at(0).getRawValue().qty).toEqual(testQty('5'));
  });

  it('readOnly disables every cell control', async () => {
    const { fixture } = await renderGrid({
      componentProperties: { readOnly: signal(true) },
    });
    fixture.detectChanges();
    await fixture.whenStable();

    expect(screen.queryByRole('button', { name: /add line/i })).toBeNull();
    // `queryAllByRole`'s declared return type is `HTMLElement[]`; `tsc
    // -p tsconfig.spec.json` (the `typecheck` script) needs the narrowing
    // explicit even though `ng lint`'s type-aware pass resolves it without
    // one - the two use different project configurations for this file.
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-type-assertion
    const inputs = screen.queryAllByRole('textbox') as HTMLInputElement[];
    for (const input of inputs) {
      expect(input.readOnly || input.disabled).toBe(true);
    }
  });

  it('a row-error message renders in the gutter, linked by aria-describedby', async () => {
    const { fixture } = await renderGrid();
    fixture.detectChanges();
    await fixture.whenStable();

    // Deterministic by construction (`LineItemForm`'s own guarantee, `line-item-form.ts`):
    // the first row of a freshly-rendered grid is always `row-1`.
    fixture.componentInstance.rowErrors.set([
      { rowId: makeRowId('row-1'), messages: ['Unit Price must be greater than 0.'] },
    ]);
    fixture.detectChanges();

    expect(screen.getByText('Unit Price must be greater than 0.')).toBeTruthy();
  });
});
