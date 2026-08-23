import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import axe, { type Result } from 'axe-core';
import { afterEach, beforeAll, describe, expect, it } from 'vitest';

import { AmountOrPercentInputComponent } from './amount-or-percent-input.component';
import { CheckboxComponent } from './checkbox.component';
import { ComboboxComponent } from './combobox.component';
import { CurrencyInputComponent } from './currency-input.component';
import { DatePickerComponent } from './date-picker.component';
import { DateRangePickerComponent } from './date-range-picker.component';
import { fakeDecimalPort } from './fake-decimal-port';
import { FileUploadComponent } from './file-upload.component';
import { FormFieldComponent } from './form-field.component';
import { FormLayoutComponent } from './form-layout.component';
import { FormSectionComponent } from './form-section.component';
import { installMatchMedia } from './jsdom-overlay-support';
import { MultiSelectComponent } from './multi-select.component';
import { NumberInputComponent } from './number-input.component';
import { RadioGroupComponent } from './radio-group.component';
import { SelectComponent } from './select.component';
import { SwitchComponent } from './switch.component';
import { TextInputComponent } from './text-input.component';
import { TextareaComponent } from './textarea.component';
import {
  DECIMAL_PORT,
  type AmountOrPercent,
  type Money,
  type Qty,
  type SelectOption,
} from './types';

/**
 * Runtime accessibility scan over **every** control in the form layer, in both
 * themes. The `angular-eslint` template-a11y rules run in addition to this,
 * not instead of it: they read the template, axe reads the rendered DOM.
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass is still M5-09.
 */

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

/** A full-form axe pass in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;

const OPTIONS: readonly SelectOption<number>[] = [
  { value: 1, label: 'Indian Rupee' },
  { value: 2, label: 'US Dollar' },
];

const TEMPLATE = `
  <form [formGroup]="form">
  <app-form-layout [form]="form" mode="create">
    <app-form-section title="Every control" description="One of each, composed as a screen would.">
      <app-form-field label="Document number" hint="Trailing spaces are trimmed on commit.">
        <app-text-input formControlName="docNo" />
      </app-form-field>

      <app-form-field label="Remarks">
        <app-textarea formControlName="remarks" />
      </app-form-field>

      <app-form-field label="Quantity">
        <app-number-input formControlName="qty" />
      </app-form-field>

      <app-form-field label="Rate">
        <app-currency-input formControlName="rate" />
      </app-form-field>

      <app-form-field label="Discount">
        <app-amount-or-percent-input formControlName="discount" />
      </app-form-field>

      <app-form-field label="Currency">
        <app-select formControlName="currId" [options]="options" />
      </app-form-field>

      <app-form-field label="Tags">
        <app-multi-select formControlName="tags" [options]="options" />
      </app-form-field>

      <app-form-field label="Customer">
        <app-combobox formControlName="customer" [loader]="loader" />
      </app-form-field>

      <app-form-field label="Document date">
        <app-date-picker formControlName="docDate" />
      </app-form-field>

      <app-form-field label="Period">
        <app-date-range-picker formControlName="period" />
      </app-form-field>

      <app-form-field label="Active">
        <app-checkbox formControlName="active" />
      </app-form-field>

      <app-form-field label="Business type">
        <app-radio-group formControlName="busiType" [options]="options" />
      </app-form-field>

      <app-form-field label="Notifications">
        <app-switch formControlName="notify" />
      </app-form-field>

      <app-form-field label="Attachments">
        <app-file-upload formControlName="files" [accept]="'.pdf'" />
      </app-form-field>
    </app-form-section>
  </app-form-layout>
  </form>`;

function buildForm() {
  return new FormGroup({
    docNo: new FormControl<string | null>(null, [REQUIRED]),
    remarks: new FormControl<string | null>(null),
    qty: new FormControl<Qty | null>(null),
    rate: new FormControl<Money | null>(null),
    discount: new FormControl<AmountOrPercent | null>(null),
    currId: new FormControl<number | null>(null, [REQUIRED]),
    tags: new FormControl<number[] | null>(null),
    customer: new FormControl<number | null>(null),
    docDate: new FormControl<Date | null>(null),
    period: new FormControl<(Date | null)[] | null>(null),
    active: new FormControl<boolean | null>(false),
    busiType: new FormControl<number | null>(null),
    notify: new FormControl<boolean | null>(false),
    files: new FormControl<File[] | null>([]),
  });
}

async function renderEveryControl(theme: 'light' | 'dark') {
  document.documentElement.setAttribute('data-theme', theme);
  const form = buildForm();
  const view = await render(TEMPLATE, {
    imports: [
      ReactiveFormsModule,
      FormLayoutComponent,
      FormSectionComponent,
      FormFieldComponent,
      TextInputComponent,
      TextareaComponent,
      NumberInputComponent,
      CurrencyInputComponent,
      AmountOrPercentInputComponent,
      SelectComponent,
      MultiSelectComponent,
      ComboboxComponent,
      DatePickerComponent,
      DateRangePickerComponent,
      CheckboxComponent,
      RadioGroupComponent,
      SwitchComponent,
      FileUploadComponent,
    ],
    componentProperties: {
      form,
      options: OPTIONS,
      loader: () => Promise.resolve(OPTIONS),
    },
    providers: [{ provide: DECIMAL_PORT, useValue: fakeDecimalPort }],
  });
  return { form, view };
}

async function criticalViolations(container: HTMLElement): Promise<Result[]> {
  const results = await axe.run(container, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('form layer accessibility', () => {
  beforeAll(installMatchMedia);

  afterEach(() => {
    document.documentElement.removeAttribute('data-theme');
  });

  it(
    'has zero critical axe violations across every control in the light theme',
    async () => {
      const { view } = await renderEveryControl('light');

      expect(await criticalViolations(view.container)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'has zero critical axe violations across every control in the dark theme',
    async () => {
      const { view } = await renderEveryControl('dark');

      expect(await criticalViolations(view.container)).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'still has zero critical violations once every field is showing an error',
    async () => {
      const { form, view } = await renderEveryControl('light');

      form.markAllAsTouched();
      form.get('docNo')?.markAsDirty();
      form.get('currId')?.markAsDirty();
      view.fixture.detectChanges();

      // The error text is rendered, so the scan is not passing on an empty form.
      expect(screen.getAllByText('This field is required.').length).toBeGreaterThan(0);
      expect(await criticalViolations(view.container)).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});
