import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

// eslint-disable-next-line @typescript-eslint/unbound-method -- a static ValidatorFn, not a method.
const REQUIRED = Validators.required;

import { FormFieldComponent } from './form-field.component';
import { TextInputComponent } from './text-input.component';

async function setup(initial: string | null = null, required = false) {
  const form = new FormGroup({
    code: new FormControl<string | null>(initial, required ? [REQUIRED] : []),
  });
  const { fixture } = await render(
    `<form [formGroup]="form">
       <app-form-field label="Item code">
         <app-text-input formControlName="code" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent],
      componentProperties: { form },
    },
  );
  return { form, fixture, input: screen.getByRole<HTMLInputElement>('textbox') };
}

describe('app-text-input', () => {
  it('participates in a typed FormGroup', async () => {
    const { form, input } = await setup();

    await userEvent.type(input, 'ITM-001');

    expect(form.value.code).toBe('ITM-001');
    expect(form.controls.code.dirty).toBe(true);
  });

  it('writes an existing value into the field', async () => {
    const { input } = await setup('ITM-002');

    expect(input.value).toBe('ITM-002');
  });

  it('shows a validator error through app-form-field, not its own markup', async () => {
    const { input } = await setup(null, true);

    await userEvent.click(input);
    await userEvent.tab();

    expect(screen.getByText('This field is required.')).toBeDefined();
  });

  it('does not trim while the user is typing - the caret must not move', async () => {
    const { form, input } = await setup();

    // A trailing space is legitimate mid-word; trimming it on every keystroke
    // would make "AB CD" impossible to type.
    await userEvent.type(input, 'AB ');

    expect(form.value.code).toBe('AB ');
    expect(input.selectionStart).toBe(3);
  });

  it('trims on commit, reproducing TrimmedInputText.razor:21-31', async () => {
    const { form, input } = await setup();

    await userEvent.type(input, '  ITM-003  ');
    await userEvent.tab();

    expect(form.value.code).toBe('ITM-003');
  });

  it('leaves an all-whitespace value untrimmed, exactly as the Blazor guard does', async () => {
    // TrimmedInputText.razor:23 - `if (!string.IsNullOrWhiteSpace(CurrentValue))`.
    // Reproduced on purpose; see Q-71.
    const { form, input } = await setup();

    await userEvent.type(input, '   ');
    await userEvent.tab();

    expect(form.value.code).toBe('   ');
  });

  it('can be told not to trim', async () => {
    const form = new FormGroup({ code: new FormControl<string | null>(null) });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Item code">
           <app-text-input formControlName="code" [trim]="false" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent],
        componentProperties: { form },
      },
    );

    await userEvent.type(screen.getByRole<HTMLInputElement>('textbox'), ' x ');
    await userEvent.tab();

    expect(form.value.code).toBe(' x ');
  });

  it('is reachable and operable from the keyboard alone', async () => {
    const { input } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
  });

  it('keeps readonly distinct from disabled: still focusable, still copyable', async () => {
    const form = new FormGroup({ code: new FormControl<string | null>('ITM-004') });
    await render(
      `<form [formGroup]="form">
         <app-form-field label="Item code">
           <app-text-input formControlName="code" [readonly]="true" />
         </app-form-field>
       </form>`,
      {
        imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent],
        componentProperties: { form },
      },
    );

    const input = screen.getByRole<HTMLInputElement>('textbox');
    expect(input.readOnly).toBe(true);
    expect(input.disabled).toBe(false);
    input.focus();
    expect(document.activeElement).toBe(input);
  });

  it('reflects the form control disabled state on the native input', async () => {
    const { form, fixture, input } = await setup('x');

    form.controls.code.disable();
    fixture.detectChanges();

    expect(input.disabled).toBe(true);
  });
});
