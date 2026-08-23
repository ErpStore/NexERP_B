import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { AmountOrPercentInputComponent } from './amount-or-percent-input.component';
import { CheckboxComponent } from './checkbox.component';
import { ComboboxComponent } from './combobox.component';
import { DatePickerComponent } from './date-picker.component';
import { DateRangePickerComponent } from './date-range-picker.component';
import { fakeDecimalPort } from './fake-decimal-port';
import { FileUploadComponent } from './file-upload.component';
import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';
import { MultiSelectComponent } from './multi-select.component';
import { RadioGroupComponent } from './radio-group.component';
import { SelectComponent } from './select.component';
import { SwitchComponent } from './switch.component';
import { DECIMAL_PORT, type AmountOrPercent, type SelectOption } from './types';

/**
 * `readonly` is **not** `disabled`. A readonly control keeps its value on
 * screen, focusable, in the tab order and copyable; only the editing
 * affordance goes away. A disabled control is inert and leaves the tab order.
 *
 * This spec exists because the distinction is invisible to an otherwise green
 * suite: nine controls once routed `readonly` into PrimeNG's `[disabled]`,
 * which silently removed the value from the tab order - `primeng-select.mjs`
 * computes `tabindex = !$disabled() ? tabindex() : -1`. Every control with a
 * readonly state is asserted here so the defect cannot return unobserved.
 */

const OPTIONS: readonly SelectOption<number>[] = [
  { value: 1, label: 'Indian Rupee' },
  { value: 2, label: 'US Dollar' },
];

function pdf(name: string): File {
  return new File(['x'], name, { type: 'application/pdf' });
}

describe('readonly is distinct from disabled', () => {
  beforeAll(installMatchMedia);

  it('app-select stays focusable and does not open', async () => {
    const form = new FormGroup({ v: new FormControl<number | null>(1) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Currency">
           <app-select formControlName="v" [options]="options" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, SelectComponent],
        componentProperties: { form, options: OPTIONS },
      },
    );

    const trigger = screen.getByRole('combobox');
    expect(trigger.getAttribute('tabindex')).not.toBe('-1');
    trigger.focus();
    expect(document.activeElement).toBe(trigger);

    await userEvent.keyboard('{ArrowDown}');

    expect(screen.queryByRole('listbox', { hidden: true })).toBeNull();
    expect(form.value.v).toBe(1);
  });

  it('a disabled app-select, by contrast, leaves the tab order', async () => {
    const form = new FormGroup({ v: new FormControl<number | null>(1) });
    const view = await render(
      `<form [formGroup]="form">
         <app-form-field label="Currency">
           <app-select formControlName="v" [options]="options" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, SelectComponent],
        componentProperties: { form, options: OPTIONS },
      },
    );

    form.controls.v.disable();
    view.fixture.detectChanges();

    expect(screen.getByRole('combobox').getAttribute('tabindex')).toBe('-1');
  });

  it('app-multi-select stays focusable and does not open', async () => {
    const form = new FormGroup({ v: new FormControl<number[] | null>([1]) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Tags">
           <app-multi-select formControlName="v" [options]="options" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, MultiSelectComponent],
        componentProperties: { form, options: OPTIONS },
      },
    );

    const trigger = screen.getByRole('combobox');
    expect(trigger.getAttribute('tabindex')).not.toBe('-1');
    trigger.focus();
    expect(document.activeElement).toBe(trigger);

    await userEvent.keyboard('{ArrowDown}');
    await userEvent.keyboard('{Enter}');

    expect(screen.queryByRole('listbox', { hidden: true })).toBeNull();
    expect(form.value.v).toEqual([1]);
  });

  it('app-combobox renders a readonly input, not a disabled one', async () => {
    const form = new FormGroup({
      v: new FormControl<SelectOption<number> | null>({ value: 1, label: 'Indian Rupee' }),
    });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Customer">
           <app-combobox formControlName="v" [loader]="loader" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, ComboboxComponent],
        componentProperties: { form, loader: () => Promise.resolve(OPTIONS) },
      },
    );

    const input = screen.getByRole<HTMLInputElement>('combobox');
    expect(input.readOnly).toBe(true);
    expect(input.disabled).toBe(false);
    input.focus();
    expect(document.activeElement).toBe(input);

    await userEvent.keyboard('zzz{ArrowDown}{Enter}');

    expect(form.value.v).toEqual({ value: 1, label: 'Indian Rupee' });
  });

  it('app-date-picker keeps its typed value focusable and offers no calendar', async () => {
    const form = new FormGroup({ v: new FormControl<Date | null>(new Date(2026, 7, 23)) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Invoice date">
           <app-date-picker formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, DatePickerComponent],
        componentProperties: { form },
      },
    );

    const input = screen.getByRole<HTMLInputElement>('combobox');
    expect(input.readOnly).toBe(true);
    expect(input.disabled).toBe(false);
    expect(input.value).not.toBe('');
    input.focus();
    expect(document.activeElement).toBe(input);
    // showIcon and showOnFocus are both off when readonly, so there is no way
    // in. date-picker.component.spec.ts asserts the trigger button otherwise.
    expect(screen.queryByRole('button')).toBeNull();

    const before = form.value.v;
    await userEvent.keyboard('{ArrowDown}{Enter}');

    expect(form.value.v).toBe(before);
  });

  it('app-date-range-picker keeps its typed value focusable', async () => {
    const form = new FormGroup({
      v: new FormControl<(Date | null)[] | null>([new Date(2026, 7, 1), new Date(2026, 7, 23)]),
    });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Period">
           <app-date-range-picker formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, DateRangePickerComponent],
        componentProperties: { form },
      },
    );

    const input = screen.getByRole<HTMLInputElement>('combobox');
    expect(input.readOnly).toBe(true);
    expect(input.disabled).toBe(false);
    input.focus();
    expect(document.activeElement).toBe(input);
  });

  it('app-checkbox stays focusable and Space does not change the value', async () => {
    const form = new FormGroup({ v: new FormControl<boolean | null>(true) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Active">
           <app-checkbox formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, CheckboxComponent],
        componentProperties: { form },
      },
    );

    const box = screen.getByRole<HTMLInputElement>('checkbox');
    expect(box.disabled).toBe(false);
    box.focus();
    expect(document.activeElement).toBe(box);

    await userEvent.keyboard('{ }');

    expect(form.value.v).toBe(true);
  });

  it('app-switch stays focusable and Space does not change the value', async () => {
    const form = new FormGroup({ v: new FormControl<boolean | null>(false) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Notifications">
           <app-switch formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, SwitchComponent],
        componentProperties: { form },
      },
    );

    const control = screen.getByRole<HTMLInputElement>('switch');
    expect(control.disabled).toBe(false);
    control.focus();
    expect(document.activeElement).toBe(control);

    await userEvent.keyboard('{ }');

    expect(form.value.v).toBe(false);
  });

  it('app-radio-group keeps every option focusable and announces aria-readonly', async () => {
    const form = new FormGroup({ v: new FormControl<number | null>(1) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Business type">
           <app-radio-group formControlName="v" [options]="options" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, RadioGroupComponent],
        componentProperties: { form, options: OPTIONS },
      },
    );

    expect(screen.getByRole('radiogroup').getAttribute('aria-readonly')).toBe('true');

    const chosen = screen.getByLabelText<HTMLInputElement>('Indian Rupee');
    const other = screen.getByLabelText<HTMLInputElement>('US Dollar');
    expect(chosen.disabled).toBe(false);
    expect(other.disabled).toBe(false);
    chosen.focus();
    expect(document.activeElement).toBe(chosen);

    await userEvent.click(other);

    expect(form.value.v).toBe(1);
  });

  it('app-file-upload shows the attachments and no way to change them', async () => {
    const form = new FormGroup({ v: new FormControl<File[] | null>([pdf('po.pdf')]) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Attachments">
           <app-file-upload formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, FileUploadComponent],
        componentProperties: { form },
      },
    );

    expect(screen.getByText('po.pdf')).toBeDefined();
    expect(screen.queryByRole('button')).toBeNull();
  });

  it('a disabled app-file-upload, by contrast, still renders its chooser', async () => {
    const form = new FormGroup({ v: new FormControl<File[] | null>([pdf('po.pdf')]) });
    const view = await render(
      `<form [formGroup]="form">
         <app-form-field label="Attachments">
           <app-file-upload formControlName="v" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, FileUploadComponent],
        componentProperties: { form },
      },
    );

    form.controls.v.disable();
    view.fixture.detectChanges();

    expect(screen.getByRole('button', { name: /Choose/ })).toBeDefined();
  });

  it('app-amount-or-percent-input renders its mode as text, not as buttons', async () => {
    const form = new FormGroup({
      v: new FormControl<AmountOrPercent | null>({ value: null, isAmount: false }),
    });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Discount">
           <app-amount-or-percent-input formControlName="v" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, AmountOrPercentInputComponent],
        componentProperties: { form },
        providers: [{ provide: DECIMAL_PORT, useValue: fakeDecimalPort }],
      },
    );

    expect(screen.queryByRole('button', { name: 'Amount' })).toBeNull();
    expect(screen.getByText('Percent')).toBeDefined();
    expect(screen.getByRole<HTMLInputElement>('spinbutton').readOnly).toBe(true);
  });
});
