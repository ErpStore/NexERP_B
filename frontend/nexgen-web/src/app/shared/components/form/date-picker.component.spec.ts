import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { DatePickerComponent } from './date-picker.component';
import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Invoice date">
      <app-date-picker formControlName="docDate" />
    </app-form-field>
  </form>`;

async function setup(initial: Date | null = null, required = false) {
  const form = new FormGroup({
    docDate: new FormControl<Date | null>(initial, required ? [REQUIRED] : []),
  });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, DatePickerComponent],
    componentProperties: { form },
  });
  return { form, view, input: screen.getByRole<HTMLInputElement>('combobox') };
}

describe('app-date-picker', () => {
  beforeAll(installMatchMedia);

  it('is labelled by app-form-field', async () => {
    const { input } = await setup();

    expect(screen.getByLabelText('Invoice date')).toBe(input);
  });

  it('accepts typed input - the calendar is never the only entry path', async () => {
    const { form, input } = await setup();

    expect(input.readOnly).toBe(false);

    await userEvent.type(input, '2026-08-23');
    await userEvent.tab();

    expect(form.value.docDate).toBeInstanceOf(Date);
    expect((form.value.docDate as Date).getFullYear()).toBe(2026);
    expect((form.value.docDate as Date).getMonth()).toBe(7);
    expect((form.value.docDate as Date).getDate()).toBe(23);
  });

  it('renders a written value in the ISO default format', async () => {
    const { view, input } = await setup(new Date(2026, 7, 23));
    view.fixture.detectChanges();

    expect(input.value).toBe('2026-08-23');
  });

  it('offers the calendar as an extra path, not the only one', async () => {
    await setup();

    // A separate, keyboard-reachable trigger exists beside the text field.
    expect(screen.getByRole('button')).toBeDefined();
  });

  it('opens the calendar from the keyboard alone', async () => {
    const { view } = await setup(new Date(2026, 7, 23));

    await userEvent.tab();
    view.fixture.detectChanges();

    // Tabbing in is enough - PrimeNG's showOnFocus opens the panel, so a
    // keyboard user never needs the trigger button.
    expect(document.querySelector('.p-datepicker-panel')).not.toBeNull();
    expect(document.querySelector('span[data-date="2026-7-23"]')).not.toBeNull();
  });

  /*
   * The calendar GRID keys - ArrowLeft/Right/Up/Down by day, PageUp/PageDown
   * by month, Home/End within the month, Esc to close - are deliberately not
   * asserted here, and the reason is measurable rather than a matter of
   * effort. PrimeNG 22.1.0 reads the legacy 'event.which' /'event.keyCode'
   * in DatePicker.onDateCellKeydown and DatePicker.onInputKeydown, while
   * @testing-library/user-event v14 dispatches keydown with which === 0 and
   * keyCode === 0 under jsdom (probed 2026-08-23). Every one of those
   * handlers therefore falls through no matter which key is sent, so a test
   * written against them would assert the harness, not the control. They are
   * carried by the keyboard pass required at review, and both this file and
   * form/README.md say so rather than implying coverage that does not exist.
   */

  it('is reachable with a single Tab', async () => {
    const { input } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
  });

  it('shows a validator error through app-form-field', async () => {
    const { input } = await setup(null, true);

    await userEvent.click(input);
    await userEvent.tab();

    expect(screen.getByText('This field is required.')).toBeDefined();
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });
});
