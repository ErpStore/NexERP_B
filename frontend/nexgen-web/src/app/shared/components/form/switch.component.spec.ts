import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { SwitchComponent } from './switch.component';

async function setup(initial: boolean | null = false) {
  const form = new FormGroup({ showClosed: new FormControl<boolean | null>(initial) });
  const { fixture } = await render(
    `<form [formGroup]="form">
       <app-form-field label="Show closed">
         <app-switch formControlName="showClosed" />
       </app-form-field>
     </form>`,
    {
      imports: [ReactiveFormsModule, FormFieldComponent, SwitchComponent],
      componentProperties: { form },
    },
  );
  return { form, fixture, control: screen.getByRole('switch') };
}

describe('app-switch', () => {
  it('exposes the switch role and is labelled by app-form-field', async () => {
    const { control } = await setup();

    expect(screen.getByLabelText('Show closed')).toBe(control);
  });

  it('toggles with Space', async () => {
    const { form, control } = await setup(false);

    control.focus();
    await userEvent.keyboard(' ');

    expect(form.value.showClosed).toBe(true);
  });

  it('is one Tab stop', async () => {
    const { control } = await setup();

    await userEvent.tab();

    expect(document.activeElement).toBe(control);
  });

  it('documents the immediate-effect rule that separates it from app-checkbox', () => {
    const source = readFileSync(
      resolve(process.cwd(), 'src/app/shared/components/form/switch.component.ts'),
      'utf8',
    );
    expect(source).toContain('immediate effect');
    expect(source).toContain('app-checkbox');
  });
});
