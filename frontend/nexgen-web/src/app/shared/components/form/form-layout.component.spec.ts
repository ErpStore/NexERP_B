import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { FormLayoutComponent } from './form-layout.component';
import { TextInputComponent } from './text-input.component';

const byLayout = By.directive(FormLayoutComponent);

const IMPORTS = [ReactiveFormsModule, FormLayoutComponent, FormFieldComponent, TextInputComponent];

function twoFieldForm() {
  return new FormGroup({
    code: new FormControl<string | null>(null),
    name: new FormControl<string | null>(null),
  });
}

/*
 * Note the shape: the SCREEN owns `<form [formGroup]>` and the layout sits
 * inside it. Angular resolves a projected `formControlName` through the
 * declaration injector tree, so a `FormGroupDirective` hidden inside
 * `app-form-layout` would be invisible to these fields and every one of them
 * would throw NG01050.
 */
const TWO_FIELDS = `
  <form [formGroup]="form">
    <app-form-layout [form]="form" [mode]="mode" [loading]="loading" [formErrors]="formErrors">
      <app-form-field label="Item code">
        <app-text-input formControlName="code" />
      </app-form-field>
      <app-form-field label="Item name">
        <app-text-input formControlName="name" />
      </app-form-field>
      <button formActions type="submit">Save</button>
    </app-form-layout>
  </form>`;

async function renderLayout(overrides: Record<string, unknown> = {}) {
  const form = twoFieldForm();
  const view = await render(TWO_FIELDS, {
    imports: IMPORTS,
    componentProperties: {
      form,
      mode: 'edit',
      loading: false,
      formErrors: [],
      ...overrides,
    },
  });
  const layout = view.fixture.debugElement.query(byLayout).componentInstance as FormLayoutComponent;
  return { form, view, layout };
}

describe('app-form-layout', () => {
  it('renders the projected fields against the typed FormGroup the screen owns', async () => {
    const { form } = await renderLayout();

    expect(screen.getByRole('textbox', { name: 'Item code' })).toBeDefined();
    expect(Object.keys(form.controls)).toEqual(['code', 'name']);
  });

  // The bands come from M2-C04-01's breakpoint tokens; no new breakpoint
  // value is introduced here.
  const bands: readonly (readonly [number, number])[] = [
    [1600, 3],
    [1440, 3],
    [1200, 2],
    [1024, 2],
    [900, 1],
    [500, 1],
  ];

  for (const [width, expected] of bands) {
    it(`uses ${expected} column(s) at ${width}px`, async () => {
      window.innerWidth = width;

      const { view } = await renderLayout();
      view.fixture.detectChanges();

      const grid = document.querySelector<HTMLElement>('.form-layout__grid');
      expect(grid?.style.gridTemplateColumns).toBe(`repeat(${expected}, minmax(0, 1fr))`);
    });
  }

  it('autofocuses the first field in create mode', async () => {
    await renderLayout({ mode: 'create' });

    expect(document.activeElement).toBe(screen.getByRole('textbox', { name: 'Item code' }));
  });

  it('does not steal the focus in edit mode', async () => {
    await renderLayout({ mode: 'edit' });

    expect(document.activeElement).not.toBe(screen.getByRole('textbox', { name: 'Item code' }));
  });

  it('renders a field-shaped skeleton while loading, never a spinner on a blank page', async () => {
    await renderLayout({ loading: true });

    const grid = document.querySelector('.form-layout__grid');
    expect(grid?.getAttribute('aria-busy')).toBe('true');
    expect(document.querySelectorAll('.form-layout__skeleton').length).toBeGreaterThan(0);
    expect(screen.queryByRole('textbox')).toBeNull();
  });

  it('shows form-level messages verbatim in an alert', async () => {
    await renderLayout({ formErrors: ['A currency rate for today has not been entered.'] });

    const alert = screen.getByRole('alert');
    expect(alert.textContent).toContain('A currency rate for today has not been entered.');
  });

  it('exposes dirty state for the M2-C03/M2-C08 CanDeactivate guard', async () => {
    const { form, layout } = await renderLayout();

    expect(layout.dirty()).toBe(false);

    await userEvent.type(screen.getByRole('textbox', { name: 'Item code' }), 'x');

    expect(form.dirty).toBe(true);
    expect(layout.dirty()).toBe(true);
  });

  it('projects the sticky footer actions into a keyboard-reachable slot', async () => {
    await renderLayout();

    const save = screen.getByRole('button', { name: 'Save' });
    expect(save.closest('.form-layout__actions')).not.toBeNull();

    save.focus();
    expect(document.activeElement).toBe(save);
  });

  it('emits formSubmit when the screen hands the submit over', async () => {
    const { layout } = await renderLayout();
    let submitted = 0;
    layout.formSubmit.subscribe(() => (submitted += 1));

    layout.onSubmit();

    expect(submitted).toBe(1);
  });

  it('marks every control touched on submit, so nothing hides an error', async () => {
    const { form, layout } = await renderLayout();

    layout.onSubmit();

    expect(form.controls.code.touched).toBe(true);
    expect(form.controls.name.touched).toBe(true);
  });
});
