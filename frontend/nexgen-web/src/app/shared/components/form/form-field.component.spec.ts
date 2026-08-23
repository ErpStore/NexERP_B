import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

import { FormFieldComponent } from './form-field.component';
import { TextInputComponent } from './text-input.component';

interface Harness {
  readonly form: FormGroup<{ name: FormControl<string | null> }>;
  readonly input: HTMLInputElement;
}

async function renderField(options?: { hint?: string; required?: boolean }): Promise<Harness> {
  const form = new FormGroup({
    name: new FormControl<string | null>(null, options?.required ? [REQUIRED] : []),
  });
  await render(
    `<form [formGroup]="form">
       <app-form-field label="Currency name" [hint]="hint">
         <app-text-input formControlName="name" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent],
      componentProperties: { form, hint: options?.hint },
    },
  );
  return { form, input: screen.getByRole('textbox') };
}

describe('app-form-field - the single validation-display mechanism', () => {
  it('associates the label with the projected control programmatically', async () => {
    const { input } = await renderField();

    // getByLabelText resolves the label -> control association the same way
    // an assistive technology does.
    expect(screen.getByLabelText('Currency name')).toBe(input);
  });

  it('puts the hint in aria-describedby', async () => {
    const { input } = await renderField({ hint: 'As printed on the invoice' });

    const describedBy = input.getAttribute('aria-describedby');
    expect(describedBy).not.toBeNull();
    const hint = document.getElementById(describedBy!.split(' ')[0]!);
    expect(hint?.textContent?.trim()).toBe('As printed on the invoice');
  });

  it('renders * and aria-required when the control has Validators.required', async () => {
    const { input } = await renderField({ required: true });

    expect(screen.getByText('*')).toBeDefined();
    expect(input.getAttribute('aria-required')).toBe('true');
  });

  it('renders neither * nor aria-required when the control is optional', async () => {
    const { input } = await renderField();

    expect(screen.queryByText('*')).toBeNull();
    expect(input.getAttribute('aria-required')).toBeNull();
  });

  it('shows the error only once the user has touched the field, and toggles aria-invalid', async () => {
    const { input } = await renderField({ required: true });

    expect(input.getAttribute('aria-invalid')).toBeNull();
    expect(screen.queryByText('This field is required.')).toBeNull();

    await userEvent.click(input);
    await userEvent.tab();

    expect(screen.getByText('This field is required.')).toBeDefined();
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });

  it('adds both hint and error to aria-describedby when both are showing', async () => {
    const { input } = await renderField({ required: true, hint: 'As printed on the invoice' });

    await userEvent.click(input);
    await userEvent.tab();

    const ids = (input.getAttribute('aria-describedby') ?? '').split(' ').filter(Boolean);
    expect(ids).toHaveLength(2);
    const texts = ids.map((id) => document.getElementById(id)?.textContent?.trim());
    expect(texts).toContain('As printed on the invoice');
    expect(texts?.some((t) => t?.includes('This field is required.'))).toBe(true);
  });

  it('announces the error in a polite live region that exists before the error does', async () => {
    const { input } = await renderField({ required: true });

    const region = document.querySelector('[aria-live="polite"]');
    expect(region).not.toBeNull();
    expect(region?.textContent?.trim()).toBe('');

    await userEvent.click(input);
    await userEvent.tab();

    expect(region?.textContent).toContain('This field is required.');
  });

  it('does not encode the error in colour alone - there is an icon and words', async () => {
    const { input, form } = await renderField({ required: true });
    await userEvent.click(input);
    await userEvent.tab();
    expect(form.controls.name.touched).toBe(true);

    const region = document.querySelector('[aria-live="polite"]');
    expect(region?.querySelector('[aria-hidden="true"]')).not.toBeNull();
    expect(region?.textContent).toContain('required');
  });
});
