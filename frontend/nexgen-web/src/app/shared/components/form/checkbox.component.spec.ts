import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { CheckboxComponent } from './checkbox.component';
import { FormFieldComponent } from './form-field.component';

async function setup(initial: boolean | null = null) {
  const form = new FormGroup({ active: new FormControl<boolean | null>(initial) });
  const { fixture } = await render(
    `<form [formGroup]="form">
       <app-form-field label="Active">
         <app-checkbox formControlName="active" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, CheckboxComponent],
      componentProperties: { form },
    },
  );
  return { form, fixture, box: screen.getByRole('checkbox') };
}

describe('app-checkbox', () => {
  it('is labelled by app-form-field', async () => {
    const { box } = await setup();

    expect(screen.getByLabelText('Active')).toBe(box);
  });

  it('toggles with Space and writes to the FormGroup', async () => {
    const { form, box } = await setup(false);

    box.focus();
    await userEvent.keyboard(' ');

    expect(form.value.active).toBe(true);
  });

  it('toggles back on a second activation', async () => {
    const { form, box, fixture } = await setup(false);

    await userEvent.click(box);
    fixture.detectChanges();
    await userEvent.click(box);

    expect(form.value.active).toBe(false);
  });

  it('reflects a written value', async () => {
    const { box, fixture } = await setup(true);
    fixture.detectChanges();

    expect((box as HTMLInputElement).checked).toBe(true);
  });

  it('is one Tab stop', async () => {
    const { box } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(box);
  });

  it('documents that it is the on-submit control, not the immediate-effect one', () => {
    // The rule has to live where a developer will read it, or the 140 screens
    // will use switch and checkbox interchangeably and the user will never
    // know whether a change has been saved.
    const source = readFileSync(
      resolve(process.cwd(), 'src/app/shared/components/form/checkbox.component.ts'),
      'utf8',
    );
    expect(source).toContain('app-switch');
    expect(source).toContain('saved on submit');
  });
});
