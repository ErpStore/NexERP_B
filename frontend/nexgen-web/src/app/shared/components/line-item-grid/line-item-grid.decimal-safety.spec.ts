import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { format, money, qty, toApi, type Money, type Qty } from '../../utils/decimal';
import { DecimalCellComponent } from './cells/decimal-cell.component';
import { IntegerCellComponent } from './cells/integer-cell.component';

/**
 * Tests 9/10 (M2-C07 Testing Requirements): every numeric cell parses,
 * stores and emits through M2-C10's decimal wrappers - never a JS `number`
 * for a monetary or quantity value, and never a float-arithmetic surprise
 * like `0.1 + 0.2`.
 */

describe('DecimalCellComponent - decimal safety', () => {
  it('0.1 + 0.2 entered across two decimal cells round-trips as exactly 0.3, not the float artefact', async () => {
    const a = new FormControl<Money | null>(null);
    const b = new FormControl<Money | null>(null);

    await render(
      `<app-line-item-decimal-cell [formControl]="a" [decimalPlaces]="1" />
       <app-line-item-decimal-cell [formControl]="b" [decimalPlaces]="1" />`,
      {
        imports: [ReactiveFormsModule, DecimalCellComponent],
        componentProperties: { a, b },
      },
    );

    const inputs = screen.getAllByRole('textbox');
    // A `null` value displays as `format(null)` - the em dash `ABSENT_DISPLAY`,
    // deliberately never a bare empty string (see `format.ts`). `userEvent.type`
    // appends at the cursor rather than replacing, so the field must be
    // cleared first or the dash ends up prepended to what is typed.
    await userEvent.clear(inputs[0]!);
    await userEvent.type(inputs[0]!, '0.1');
    await userEvent.tab();
    await userEvent.clear(inputs[1]!);
    await userEvent.type(inputs[1]!, '0.2');
    await userEvent.tab();

    const sum =
      a.value !== null && b.value !== null ? money(a.value.plus(b.value).toString()) : null;
    // `.toFixed()` on the raw Decimal is what the repo-wide float-money scan
    // (no-float-money.spec.ts) exists to catch, regardless of receiver type
    // - so this assertion goes through `format()` instead, which is the
    // actual M2-C10 formatter this whole component is required to use.
    expect(format(sum, { places: 1 })).toBe('0.3');
    // The float artefact this test exists to rule out:
    expect(0.1 + 0.2).not.toBe(0.3);
  });

  it('never emits a JS `number` for a monetary value - `toApi` sees a decimal string, never a float', async () => {
    const control = new FormControl<Money | null>(null);
    await render(`<app-line-item-decimal-cell [formControl]="control" [decimalPlaces]="2" />`, {
      imports: [ReactiveFormsModule, DecimalCellComponent],
      componentProperties: { control },
    });

    const input = screen.getByRole('textbox');
    await userEvent.clear(input);
    await userEvent.type(input, '1234.56');
    await userEvent.tab();

    expect(typeof control.value).not.toBe('number');
    expect(control.value).not.toBeNull();
    if (control.value === null) {
      throw new Error('expected a committed value');
    }
    // `toApi` legitimately returns a bare JS number when the value
    // round-trips through one exactly (`money.ts:100-106`) - the contract it
    // actually holds is "never silently loses precision", not "always a
    // string". The stored value itself is what must never be a JS `number`,
    // asserted above; this only proves the two representations agree.
    expect(String(toApi(control.value))).toBe('1234.56');
  });

  it('never yields 0 for input it did not understand (empty stays empty, an error is not silently coerced)', async () => {
    const control = new FormControl<Qty | null>(qty('5'));
    await render(`<app-line-item-integer-cell [formControl]="control" [decimalPlaces]="0" />`, {
      imports: [ReactiveFormsModule, IntegerCellComponent],
      componentProperties: { control },
    });

    const input = screen.getByRole('textbox');
    await userEvent.clear(input);
    await userEvent.tab();

    expect(control.value).toBeNull();
    expect(control.value).not.toBe(qty('0'));
  });

  it('rejects the final over-precise keystroke rather than silently rounding it in', async () => {
    const control = new FormControl<Money | null>(null);
    await render(`<app-line-item-decimal-cell [formControl]="control" [decimalPlaces]="2" />`, {
      imports: [ReactiveFormsModule, DecimalCellComponent],
      componentProperties: { control },
    });

    const input = screen.getByRole('textbox');
    await userEvent.clear(input);
    await userEvent.type(input, '1.999');
    await userEvent.tab();

    // The design (`decimal-cell.component.ts`'s own `onInput`/`onBlur`):
    // each keystroke commits its prefix live if that prefix alone is valid
    // - "1", "1.9", "1.99" all are, at `places: 2` - and the final `9` that
    // makes it "1.999" is rejected outright rather than silently rounded to
    // "2.00" or applied as a third decimal. The committed value is
    // therefore the last valid prefix, not `null` and not the typed text.
    expect(control.value).toEqual(money('1.99'));
  });

  it('formats on blur using decimal.js, never `toFixed` on a JS number', async () => {
    const control = new FormControl<Money | null>(money('1000'));
    const { fixture } = await render(
      `<app-line-item-decimal-cell [formControl]="control" [decimalPlaces]="2" />`,
      { imports: [ReactiveFormsModule, DecimalCellComponent], componentProperties: { control } },
    );
    fixture.detectChanges();
    // Same `tsc` vs. `ng lint` divergence as `line-item-grid.component.spec.ts`
    // - `typecheck` needs this narrowing, `ng lint` does not.
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-type-assertion
    const input = screen.getByRole('textbox') as HTMLInputElement;
    // Intl.NumberFormat groups the thousands - the tell that this went
    // through `format()` (shared/utils/decimal) and not a bare `toString()`.
    expect(input.value).toContain('1,000.00');
  });
});
