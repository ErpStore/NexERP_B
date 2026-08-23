import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';
import { SelectComponent } from './select.component';
import type { SelectOption } from './types';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

const OPTIONS: readonly SelectOption<number>[] = [
  { value: 1, label: 'Indian Rupee' },
  { value: 2, label: 'US Dollar' },
];

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Currency">
      <app-select
        formControlName="currId"
        [options]="options"
        [loading]="loading"
        [errorMessage]="errorMessage"
      />
    </app-form-field>
  </form>`;

async function setup(overrides: Record<string, unknown> = {}, required = false) {
  const form = new FormGroup({
    currId: new FormControl<number | null>(null, required ? [REQUIRED] : []),
  });
  const { fixture } = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, SelectComponent],
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

describe('app-select', () => {
  beforeAll(installMatchMedia);

  it('is labelled by app-form-field', async () => {
    await setup();

    expect(screen.getByRole('combobox', { name: /Currency/ })).toBeDefined();
  });

  it('opens with ArrowDown and lists its options', async () => {
    const { trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    // `hidden: true` is a jsdom concession, not a lowered bar: PrimeNG opens
    // the panel through an animation wrapper that carries `display: none`
    // until a transition jsdom never runs, so the options are in the DOM with
    // their roles and names but computed as hidden. The roles and names are
    // still asserted, which is the part this control owns.
    expect(screen.getByRole('option', { name: 'Indian Rupee', hidden: true })).toBeDefined();
    expect(screen.getByRole('option', { name: 'US Dollar', hidden: true })).toBeDefined();
  });

  it('selects with Enter and writes the option value to the FormGroup', async () => {
    const { form, trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();
    await userEvent.keyboard('{ArrowDown}{Enter}');
    fixture.detectChanges();

    expect(form.value.currId).toBe(1);
  });

  it('jumps to the last option with End and to the first with Home', async () => {
    const { form, trigger, fixture } = await setup();

    // End and Home open the panel and move the focused option (PrimeNG
    // Select.onEndKey / onHomeKey); Enter commits the focused one, so the
    // whole first-to-last jump is provable without a mouse.
    trigger.focus();
    await userEvent.keyboard('{End}');
    fixture.detectChanges();
    await userEvent.keyboard('{Enter}');
    fixture.detectChanges();

    expect(form.value.currId).toBe(OPTIONS[OPTIONS.length - 1]!.value);

    screen.getByRole('combobox').focus();
    await userEvent.keyboard('{Home}');
    fixture.detectChanges();
    await userEvent.keyboard('{Enter}');
    fixture.detectChanges();

    expect(form.value.currId).toBe(OPTIONS[0]!.value);
  });

  it('leaves the value untouched when the user escapes out of the panel', async () => {
    const { form, trigger, fixture } = await setup();

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();
    await userEvent.keyboard('{Escape}');
    fixture.detectChanges();

    // Escape restores the previous value - here, nothing selected. The panel
    // actually disappearing is animation-driven and jsdom runs no
    // transitions, so that half is covered by the keyboard pass at review.
    expect(form.value.currId).toBeNull();
    expect(form.controls.currId.pristine).toBe(true);
  });

  it('renders an explicit empty row rather than a silently blank dropdown', async () => {
    const { trigger, fixture } = await setup({ options: [] });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByText('No options')).toBeDefined();
  });

  it('renders a loading row in the dropdown, not a blocking overlay', async () => {
    const { trigger, fixture } = await setup({ loading: true });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByText('Loading options…')).toBeDefined();
  });

  it('renders the error state rather than swallowing it into an empty list', async () => {
    const { trigger, fixture } = await setup({ errorMessage: 'Could not load currencies.' });

    trigger.focus();
    await userEvent.keyboard('{ArrowDown}');
    fixture.detectChanges();

    expect(screen.getByRole('alert', { hidden: true }).textContent).toContain(
      'Could not load currencies.',
    );
  });

  it('carries the required and invalid ARIA state from app-form-field', async () => {
    const { form, fixture, trigger } = await setup({}, true);

    expect(trigger.getAttribute('aria-required')).toBe('true');

    form.controls.currId.markAsTouched();
    fixture.detectChanges();

    expect(screen.getByRole('combobox').getAttribute('aria-invalid')).toBe('true');
    expect(screen.getByText('This field is required.')).toBeDefined();
  });
});
