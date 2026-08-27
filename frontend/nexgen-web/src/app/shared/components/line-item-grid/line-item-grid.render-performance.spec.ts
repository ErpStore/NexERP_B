import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  input,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it } from 'vitest';

import { TextCellComponent } from './cells/text-cell.component';

/**
 * Test 7 (M2-C07 Testing Requirements). Same technique as
 * `shared/components/form/render-count.spec.ts` - "the test that protects
 * M2-C07" - scaled from 50 form fields to 200 grid rows: a probe component
 * per row counts its own view evaluations, and typing in one row must move
 * only that row's counter.
 *
 * This does not exercise `LineItemGridComponent` itself (a full 200-row
 * `p-table` render inside jsdom is prohibitively slow for a unit test) - it
 * proves the mechanism `LineItemRowComponent` and every cell are built on:
 * `OnPush` plus a per-control `FormControl` binding, which is exactly what
 * `LineItemGridComponent`'s row template composes 200 times over.
 */

const renders: number[] = [];

@Component({
  selector: 'app-probe-row',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, TextCellComponent],
  template: `<span class="probe" aria-hidden="true">{{ count() }}</span>
    <app-line-item-text-cell [formControl]="control()" [ariaLabel]="'Remarks ' + index()" />`,
})
class ProbeRowComponent {
  private readonly cdr = inject(ChangeDetectorRef);

  readonly index = input.required<number>();
  readonly control = input.required<FormControl<string>>();

  count(): string {
    renders[this.index()] = (renders[this.index()] ?? 0) + 1;
    return '';
  }

  poke(): void {
    this.cdr.markForCheck();
  }
}

const ROW_COUNT = 200;
const THIS_IS_SLOW = 60_000;

const TEMPLATE = `
  @for (control of controls; track $index) {
    <app-probe-row [index]="$index" [control]="control" />
  }`;

async function renderRows() {
  const controls = Array.from(
    { length: ROW_COUNT },
    () => new FormControl('', { nonNullable: true }),
  );
  const view = await render(TEMPLATE, {
    imports: [ProbeRowComponent],
    componentProperties: { controls },
  });
  return { view, controls };
}

describe('LineItemGrid row isolation across 200 rows', () => {
  beforeEach(() => {
    renders.length = 0;
  });

  it(
    'typing in row 187 re-renders only row 187',
    async () => {
      await renderRows();

      const target = screen.getByRole('textbox', { name: 'Remarks 187' });
      const baseline = [...renders];
      expect(baseline).toHaveLength(ROW_COUNT);

      await userEvent.type(target, 'x');

      const siblingDeltas = renders
        .map((value, index) => ({ index, delta: value - (baseline[index] ?? 0) }))
        .filter(({ index }) => index !== 187)
        .filter(({ delta }) => delta !== 0);

      expect(siblingDeltas).toEqual([]);
    },
    THIS_IS_SLOW,
  );

  it(
    'the typed character actually reached the control, so the isolation assertion above means something',
    async () => {
      const { controls } = await renderRows();
      await userEvent.type(screen.getByRole('textbox', { name: 'Remarks 187' }), 'x');
      expect(controls[187]?.value).toBe('x');
    },
    THIS_IS_SLOW,
  );

  it('counts a re-check when one genuinely happens, so the probe is not inert', async () => {
    const { view } = await renderRows();
    const before = renders[42] ?? 0;
    const instances = view.fixture.debugElement.queryAll(By.directive(ProbeRowComponent));
    (instances[42]?.componentInstance as ProbeRowComponent).poke();
    view.fixture.detectChanges();
    expect(renders[42] ?? 0).toBeGreaterThan(before);
  });
});
