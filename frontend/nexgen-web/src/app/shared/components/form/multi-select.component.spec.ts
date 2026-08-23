import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';
import { MultiSelectComponent } from './multi-select.component';
import type { SelectOption } from './types';

const OPTIONS: readonly SelectOption<string>[] = [
  { value: 'draft', label: 'Draft' },
  { value: 'approved', label: 'Approved' },
  { value: 'closed', label: 'Closed' },
];

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Status">
      <app-multi-select
        formControlName="status"
        [options]="options"
        [loading]="loading"
        [errorMessage]="errorMessage"
      />
    </app-form-field>
  </form>`;

async function setup(overrides: Record<string, unknown> = {}) {
  const form = new FormGroup({ status: new FormControl<string[] | null>([]) });
  const { fixture } = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, MultiSelectComponent],
    componentProperties: {
      form,
      options: OPTIONS,
      loading: false,
      errorMessage: undefined,
      ...overrides,
    },
  });
  return { form, fixture, trigger: screen.getByRole('combobox') };
}

describe('app-multi-select', () => {
  beforeAll(installMatchMedia);

  it('is named by the field label through aria-labelledby', async () => {
    await setup();

    expect(screen.getByRole('combobox', { name: /Status/ })).toBeDefined();
  });

  it('opens with ArrowDown and selects several options with Enter', async () => {
    const { form, trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();
    await userEvent.keyboard('{ArrowDown}{Enter}');
    fixture.detectChanges();
    await userEvent.keyboard('{ArrowDown}{Enter}');
    fixture.detectChanges();

    // Two distinct options, chosen entirely from the keyboard. Which two the
    // panel highlights first is PrimeNG own business, not this control.
    expect(form.value.status).toHaveLength(2);
    expect(new Set(form.value.status ?? []).size).toBe(2);
    for (const value of form.value.status ?? []) {
      expect(OPTIONS.map((o) => o.value)).toContain(value);
    }
  });

  it('jumps with End and Home and steps with ArrowDown, choosing every option by keyboard', async () => {
    const { form, trigger, fixture } = await setup();

    // End -> last option, Home -> first, ArrowDown -> the next one after it.
    // Enter toggles the focused option and the panel stays open, so the three
    // keys are asserted one after another on the same open list.
    trigger.focus();
    await userEvent.keyboard('{End}{Enter}');
    fixture.detectChanges();

    expect(form.value.status).toEqual(['closed']);

    await userEvent.keyboard('{Home}{Enter}');
    fixture.detectChanges();

    expect(form.value.status).toContain('draft');

    await userEvent.keyboard('{ArrowDown}{Enter}');
    fixture.detectChanges();

    expect(new Set(form.value.status ?? [])).toEqual(new Set(['draft', 'approved', 'closed']));

    // ArrowUp walks back to the option Home reached and Enter clears it,
    // which is the only assertion that proves ArrowUp moved at all.
    await userEvent.keyboard('{ArrowUp}{Enter}');
    fixture.detectChanges();

    expect(form.value.status).not.toContain('draft');
  });

  it('removes the most recent chip with Backspace, and leaves filter text alone', async () => {
    const { form, trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Home}{Enter}{ArrowDown}{Enter}');
    fixture.detectChanges();

    expect(form.value.status).toEqual(['draft', 'approved']);

    await userEvent.keyboard('{Backspace}');
    fixture.detectChanges();

    expect(form.value.status).toEqual(['draft']);

    // Inside the filter box the key belongs to the text, not to the chips.
    // PrimeNG gives that box role="searchbox" (primeng 22.1.0 multiselect
    // template); the trigger itself is the combobox.
    const filter = screen.getByRole<HTMLInputElement>('searchbox', { hidden: true });
    await userEvent.type(filter, 'dr');
    await userEvent.keyboard('{Backspace}');
    fixture.detectChanges();

    expect(filter.value).toBe('d');
    expect(form.value.status).toEqual(['draft']);
  });

  it('lists its options with the option role and their labels', async () => {
    const { trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    // hidden: true - see the note in select.component.spec.ts; the PrimeNG
    // panel is display:none until an animation jsdom never runs.
    expect(screen.getByRole('option', { name: /Draft/, hidden: true })).toBeDefined();
  });

  it('renders an explicit empty row rather than a blank panel', async () => {
    const { trigger, fixture } = await setup({ options: [] });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByText('No options')).toBeDefined();
  });

  it('renders a loading row', async () => {
    const { trigger, fixture } = await setup({ loading: true });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByText('Loading options…')).toBeDefined();
  });

  it('renders an error row instead of pretending the list is empty', async () => {
    const { trigger, fixture } = await setup({ errorMessage: 'Could not load statuses.' });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByRole('alert', { hidden: true }).textContent).toContain(
      'Could not load statuses.',
    );
  });

  it('normalises a cleared selection to an empty array, never null', async () => {
    const { form, fixture } = await setup();
    const control = fixture.debugElement.query(
      (node) => node.componentInstance instanceof MultiSelectComponent,
    ).componentInstance as MultiSelectComponent<string>;

    control.onModelChange(null);

    expect(form.value.status).toEqual([]);
  });
});
