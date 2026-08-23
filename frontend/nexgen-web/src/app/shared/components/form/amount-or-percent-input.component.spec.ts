import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { AmountOrPercentInputComponent } from './amount-or-percent-input.component';
import { asText, fakeDecimalPort, money } from './fake-decimal-port';
import { FormFieldComponent } from './form-field.component';
import { DECIMAL_PORT, PRECISION_POLICY, type AmountOrPercent } from './types';

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Discount">
      <app-amount-or-percent-input formControlName="disc" />
    </app-form-field>
  </form>`;

async function setup(initial: AmountOrPercent | null = null) {
  const form = new FormGroup({ disc: new FormControl<AmountOrPercent | null>(initial) });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, AmountOrPercentInputComponent],
    componentProperties: { form },
    providers: [
      { provide: PRECISION_POLICY, useValue: { currencyScale: 2, quantityScale: 3 } },
      { provide: DECIMAL_PORT, useValue: fakeDecimalPort },
    ],
  });
  const control = view.fixture.debugElement.query(By.directive(AmountOrPercentInputComponent))
    .componentInstance as AmountOrPercentInputComponent;
  return { form, view, control };
}

describe('app-amount-or-percent-input', () => {
  it('emits a value and a mode flag as one pair', async () => {
    const { form, control } = await setup();

    control.onNumberChange(150);

    expect(asText(form.value.disc?.value ?? null)).toBe('150');
    expect(form.value.disc?.isAmount).toBe(true);
  });

  it('defaults to amount, matching every ...AmtOrPer column default of true', async () => {
    // DebitNote.cs:95, :109, :117, :146 - all bool, all defaulting to true,
    // and CalculationService.cs:29-31 shows true means a fixed amount.
    const { control } = await setup();

    expect(control.isAmount()).toBe(true);
  });

  it('does not convert the number when the mode is toggled', async () => {
    const { form, control } = await setup({ value: money('100'), isAmount: true });

    control.onModeChange(false);

    // 100 as an amount becomes 100 as a percent - not 1. Applying the flag is
    // BR-CALC-001 in CalculationService.UpdateTotalsAsync, server-side.
    expect(asText(form.value.disc?.value ?? null)).toBe('100');
    expect(form.value.disc?.isAmount).toBe(false);
  });

  it('keeps the mode when the number changes', async () => {
    const { form, control } = await setup({ value: money('5'), isAmount: false });

    control.onNumberChange(7);

    expect(form.value.disc?.isAmount).toBe(false);
    expect(asText(form.value.disc?.value ?? null)).toBe('7');
  });

  it('offers the two modes as a keyboard-operable choice named by the field label', async () => {
    await setup();

    const amount = screen.getByRole('button', { name: 'Amount' });
    const percent = screen.getByRole('button', { name: 'Percent' });
    expect(amount).toBeDefined();
    expect(percent).toBeDefined();

    percent.focus();
    expect(document.activeElement).toBe(percent);
  });

  it('switches mode from the keyboard without touching the number', async () => {
    const { form, view, control } = await setup({ value: money('42'), isAmount: true });

    const percent = screen.getByRole('button', { name: 'Percent' });
    percent.focus();
    await userEvent.keyboard('{Enter}');
    view.fixture.detectChanges();

    expect(control.isAmount()).toBe(false);
    expect(asText(form.value.disc?.value ?? null)).toBe('42');
  });

  it('contains no arithmetic at all - asserted mechanically, not by review alone', () => {
    const source = readFileSync(
      resolve(process.cwd(), 'src/app/shared/components/form/amount-or-percent-input.component.ts'),
      'utf8',
    );

    // Strip comments first (they discuss arithmetic in prose), then string
    // and template literals (import paths and labels), and require that what
    // remains contains no arithmetic operator whatsoever.
    const withoutComments = source.replace(/\/\*[\s\S]*?\*\//g, '').replace(/^[ \t]*\/\/.*$/gm, '');
    const code = withoutComments
      .replace(/'(?:\\.|[^'\\])*'/g, "''")
      .replace(/"(?:\\.|[^"\\])*"/g, '""')
      .replace(/`(?:\\.|[^`\\])*`/g, '``');

    const offending = code
      .split(/\r?\n/)
      .map((line, index) => ({ line, index }))
      .filter(({ line }) => /[*/%+-]/.test(line))
      .map(({ line, index }) => `${index + 1}: ${line.trim()}`);

    expect(offending).toEqual([]);
  });

  it('contains no float-parsing or rounding helper', () => {
    const source = readFileSync(
      resolve(process.cwd(), 'src/app/shared/components/form/amount-or-percent-input.component.ts'),
      'utf8',
    );

    expect(source).not.toMatch(/parseFloat|toFixed|Math\./);
  });
});
