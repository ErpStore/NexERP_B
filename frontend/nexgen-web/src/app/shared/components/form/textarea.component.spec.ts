import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { TextareaComponent } from './textarea.component';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

async function setup(required = false) {
  const form = new FormGroup({
    remarks: new FormControl<string | null>(null, required ? [REQUIRED] : []),
  });
  const { fixture } = await render(
    `<form [formGroup]="form">
       <app-form-field label="Remarks">
         <app-textarea formControlName="remarks" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, TextareaComponent],
      componentProperties: { form },
    },
  );
  return { form, fixture, textarea: screen.getByRole<HTMLTextAreaElement>('textbox') };
}

describe('app-textarea', () => {
  it('is labelled by app-form-field and participates in the FormGroup', async () => {
    const { form, textarea } = await setup();

    expect(screen.getByLabelText('Remarks')).toBe(textarea);

    await userEvent.type(textarea, 'Deliver to gate 3');

    expect(form.value.remarks).toBe('Deliver to gate 3');
  });

  it('shows a validator error through app-form-field', async () => {
    const { textarea } = await setup(true);

    await userEvent.click(textarea);
    await userEvent.tab();

    expect(screen.getByText('This field is required.')).toBeDefined();
    expect(textarea.getAttribute('aria-invalid')).toBe('true');
  });

  it('trims on commit but keeps interior newlines - an address is two lines of data', async () => {
    const { form, textarea } = await setup();

    await userEvent.type(textarea, '  Line one{Enter}Line two  ');
    await userEvent.tab();

    expect(form.value.remarks).toBe('Line one\nLine two');
  });

  it('is reachable and operable from the keyboard alone', async () => {
    const { textarea } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(textarea);
  });
});
