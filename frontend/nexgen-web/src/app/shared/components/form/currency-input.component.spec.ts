import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { CurrencyInputComponent } from './currency-input.component';
import { asText, fakeDecimalPort, money } from './fake-decimal-port';
import { FormFieldComponent } from './form-field.component';
import { DECIMAL_PORT, PRECISION_POLICY, SERVER_DEFAULT_DECIMAL_PLACES, type Money } from './types';

// See the jsdom note in number-input.component.spec.ts: p-inputnumber's
// masked entry cannot be driven by userEvent under jsdom.

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Discount amount">
      <app-currency-input formControlName="amount" />
    </app-form-field>
  </form>`;

async function setup(options?: { initial?: Money | null; currencyScale?: number }) {
  const form = new FormGroup({ amount: new FormControl<Money | null>(options?.initial ?? null) });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, CurrencyInputComponent],
    componentProperties: { form },
    providers: [
      {
        provide: PRECISION_POLICY,
        useValue: { currencyScale: options?.currencyScale ?? 2, quantityScale: 3 },
      },
      { provide: DECIMAL_PORT, useValue: fakeDecimalPort },
    ],
  });
  const control = view.fixture.debugElement.query(By.directive(CurrencyInputComponent))
    .componentInstance as CurrencyInputComponent;
  return { form, view, control, input: screen.getByRole<HTMLInputElement>('spinbutton') };
}

describe('app-currency-input', () => {
  it('is labelled by app-form-field', async () => {
    const { input } = await setup();

    expect(screen.getByLabelText('Discount amount')).toBe(input);
  });

  it('holds its value as Money, never as a number', async () => {
    const { form } = await setup({ initial: money('1250.75') });

    expect(typeof form.value.amount).not.toBe('number');
    expect(asText(form.value.amount ?? null)).toBe('1250.75');
  });

  it('rejects a raw number at compile time', () => {
    const control = new FormControl<Money | null>(null);

    // @ts-expect-error - Money is not a number.
    control.setValue(1250.75);
  });

  it('defaults to the server decimal places, traceable to Companydetails.cs:208', async () => {
    // `public int DecimalPlaces { get; set; } = 2;` - the constant lives in
    // types.ts, once, and the component reads the injected policy.
    expect(SERVER_DEFAULT_DECIMAL_PLACES).toBe(2);

    const { control } = await setup();

    expect(control.scale()).toBe(2);
  });

  it('follows a tenant that configures three decimal places', async () => {
    const { control } = await setup({ currencyScale: 3 });

    expect(control.scale()).toBe(3);
  });

  it('never produces a float artefact when 0.1 is followed by 0.2', async () => {
    const { form, control } = await setup();

    control.onModelChange(0.1);
    expect(asText(form.value.amount ?? null)).toBe('0.1');

    control.onModelChange(0.2);

    expect(asText(form.value.amount ?? null)).toBe('0.2');
  });

  it('is reachable with a single Tab and right-aligned with tabular figures', async () => {
    const { input } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
    expect(input.className).toContain('app-control--numeric');
  });
});
