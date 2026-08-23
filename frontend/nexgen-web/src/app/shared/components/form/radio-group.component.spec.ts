import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { RadioGroupComponent } from './radio-group.component';
import type { SelectOption } from './types';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

const OPTIONS: readonly SelectOption<string>[] = [
  { value: 'Local', label: 'Local' },
  { value: 'InterState', label: 'Inter state' },
  { value: 'Export', label: 'Export' },
];

async function setup(required = false) {
  const form = new FormGroup({
    busiType: new FormControl<string | null>(null, required ? [REQUIRED] : []),
  });
  const { fixture } = await render(
    `<form [formGroup]="form">
       <app-form-field label="Business type">
         <app-radio-group formControlName="busiType" [options]="options" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, RadioGroupComponent],
      componentProperties: { form, options: OPTIONS },
    },
  );
  return { form, fixture, group: screen.getByRole('radiogroup') };
}

describe('app-radio-group', () => {
  it('names the group from the field label rather than an orphan label-for', async () => {
    // A group has no single focusable target, so the label becomes
    // aria-labelledby on the container instead of `for`.
    const { group } = await setup();

    expect(screen.getByRole('radiogroup', { name: 'Business type' })).toBe(group);
    const label = document.querySelector('label.form-field__label');
    expect(label?.getAttribute('for')).toBeNull();
  });

  it('renders one native radio per option, all sharing a name', async () => {
    await setup();

    const radios = screen.getAllByRole<HTMLInputElement>('radio');
    expect(radios).toHaveLength(3);
    expect(new Set(radios.map((r) => r.name)).size).toBe(1);
  });

  it('labels every option', async () => {
    await setup();

    expect(screen.getByLabelText('Inter state')).toBeDefined();
  });

  it('selects an option and writes it to the FormGroup', async () => {
    const { form } = await setup();

    await userEvent.click(screen.getByLabelText('Export'));

    expect(form.value.busiType).toBe('Export');
  });

  it('enters the group with a single Tab', async () => {
    await setup();

    await userEvent.tab();

    // Native radios supply the roving tab stop; the group costs one Tab, not
    // three. Arrow-key movement between them is the user agent's own
    // behaviour, which jsdom does not synthesise - it is covered by the
    // keyboard pass required at review.
    expect(screen.getAllByRole('radio')).toContain(document.activeElement);
  });

  it('carries aria-required and aria-invalid on the group when the field errors', async () => {
    const { form, fixture, group } = await setup(true);

    expect(group.getAttribute('aria-required')).toBe('true');

    form.controls.busiType.markAsTouched();
    fixture.detectChanges();

    expect(screen.getByRole('radiogroup').getAttribute('aria-invalid')).toBe('true');
  });
});
