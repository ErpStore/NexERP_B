import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { asText, fakeDecimalPort, qty } from './fake-decimal-port';
import { FormFieldComponent } from './form-field.component';
import { NumberInputComponent } from './number-input.component';
import { DECIMAL_PORT, PRECISION_POLICY, type Qty } from './types';

/*
 * jsdom limitation, stated rather than hidden: `p-inputnumber` is a masked
 * input that rebuilds its own value from key events and selection ranges,
 * neither of which jsdom implements faithfully, so `userEvent.type` produces
 * no value in it. The tests below therefore drive the component at the point
 * a real keystroke ends up - the model change PrimeNG emits - and assert
 * keyboard *reachability* separately. Actual masked typing is covered by the
 * keyboard pass required at review.
 */

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Quantity">
      <app-number-input formControlName="qty" />
    </app-form-field>
  </form>`;

async function setup(options?: { initial?: Qty | null; quantityScale?: number; noPort?: boolean }) {
  const form = new FormGroup({ qty: new FormControl<Qty | null>(options?.initial ?? null) });
  const providers = [
    {
      provide: PRECISION_POLICY,
      useValue: { currencyScale: 2, quantityScale: options?.quantityScale ?? 3 },
    },
    ...(options?.noPort ? [] : [{ provide: DECIMAL_PORT, useValue: fakeDecimalPort }]),
  ];
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, NumberInputComponent],
    componentProperties: { form },
    providers,
  });
  const control = view.fixture.debugElement.query(By.directive(NumberInputComponent))
    .componentInstance as NumberInputComponent;
  return { form, view, control, input: screen.getByRole<HTMLInputElement>('spinbutton') };
}

describe('app-number-input', () => {
  it('is labelled by app-form-field', async () => {
    const { input } = await setup();

    expect(screen.getByLabelText('Quantity')).toBe(input);
  });

  it('holds its value as a Qty, never as a number', async () => {
    const { form } = await setup({ initial: qty('12.5', 3) });

    const value = form.value.qty;
    expect(typeof value).not.toBe('number');
    expect(asText(value ?? null, 3)).toBe('12.5');
  });

  it('rejects a raw number at compile time', () => {
    const control = new FormControl<Qty | null>(null);

    // @ts-expect-error - a Qty is not a number; that is the whole point of the
    // branded type. If this line ever stops erroring, M2-C10's guarantee has
    // been lost and float arithmetic is back in the document model.
    control.setValue(12.5);
  });

  it('takes its decimal places from the injected policy, never a literal', async () => {
    const { control } = await setup({ quantityScale: 4 });

    expect(control.scale()).toBe(4);
  });

  it('lets one field override the scale without introducing a literal in a component', async () => {
    const form = new FormGroup({ qty: new FormControl<Qty | null>(null) });
    const view = await render(
      `<form [formGroup]="form">
         <app-form-field label="Quantity">
           <app-number-input formControlName="qty" [decimalPlaces]="6" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, NumberInputComponent],
        componentProperties: { form },
        providers: [
          { provide: PRECISION_POLICY, useValue: { currencyScale: 2, quantityScale: 3 } },
          { provide: DECIMAL_PORT, useValue: fakeDecimalPort },
        ],
      },
    );
    const control = view.fixture.debugElement.query(By.directive(NumberInputComponent))
      .componentInstance as NumberInputComponent;

    expect(control.scale()).toBe(6);
  });

  it('never produces a float artefact when 0.1 is followed by 0.2', async () => {
    const { form, control } = await setup({ quantityScale: 2 });

    control.onModelChange(0.1);
    expect(asText(form.value.qty ?? null, 2)).toBe('0.1');

    control.onModelChange(0.2);

    expect(asText(form.value.qty ?? null, 2)).toBe('0.2');
    expect(typeof form.value.qty).not.toBe('number');
  });

  it('clears to null rather than to zero', async () => {
    const { form, control } = await setup({ initial: qty('5', 3) });

    control.onModelChange(null);

    expect(form.value.qty).toBeNull();
  });

  it('marks the control touched when a value is committed', async () => {
    const { form, control } = await setup();

    control.onModelChange(3);

    expect(form.controls.qty.touched).toBe(true);
  });

  it('says so rather than lying when the decimal module is not provided', async () => {
    const { input } = await setup({ noPort: true });

    expect(input.disabled).toBe(true);
    expect(screen.getByText(/decimal module \(M2-C10\)/)).toBeDefined();
  });

  it('is reachable with a single Tab', async () => {
    const { input } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
  });

  it('is right-aligned with tabular figures, so a column of numbers can be read', async () => {
    const { input } = await setup();

    expect(input.className).toContain('app-control--numeric');
  });
});
