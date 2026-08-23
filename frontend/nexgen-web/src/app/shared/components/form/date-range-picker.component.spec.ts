import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { DateRangePickerComponent } from './date-range-picker.component';
import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Period">
      <app-date-range-picker formControlName="period" />
    </app-form-field>
  </form>`;

async function setup(initial: Date[] | null = null) {
  const form = new FormGroup({ period: new FormControl<Date[] | null>(initial) });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, DateRangePickerComponent],
    componentProperties: { form },
  });
  return { form, view, input: screen.getByRole<HTMLInputElement>('combobox') };
}

describe('app-date-range-picker', () => {
  beforeAll(installMatchMedia);

  it('is labelled by app-form-field', async () => {
    const { input } = await setup();

    expect(screen.getByLabelText('Period')).toBe(input);
  });

  it('accepts typed input for both ends of the range', async () => {
    const { input } = await setup();

    expect(input.readOnly).toBe(false);
  });

  it('renders a written range as two dates', async () => {
    const { view, input } = await setup([new Date(2026, 3, 1), new Date(2027, 2, 31)]);
    view.fixture.detectChanges();

    expect(input.value).toContain('2026-04-01');
    expect(input.value).toContain('2027-03-31');
  });

  it('keeps the half-selected state visible rather than hiding it behind a normalised object', async () => {
    const { form, view } = await setup();
    const control = view.fixture.debugElement.query(
      (node) => node.componentInstance instanceof DateRangePickerComponent,
    ).componentInstance as DateRangePickerComponent;

    control.onModelChange([new Date(2026, 3, 1)]);

    expect(form.value.period).toHaveLength(1);
  });

  it('is reachable with a single Tab', async () => {
    const { input } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
  });
});
