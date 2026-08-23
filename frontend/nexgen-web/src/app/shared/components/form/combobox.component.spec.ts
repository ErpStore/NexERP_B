import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it, vi } from 'vitest';

import { ComboboxComponent } from './combobox.component';
import { FormFieldComponent } from './form-field.component';
import { installMatchMedia } from './jsdom-overlay-support';
import type { SelectOption } from './types';

const CUSTOMERS: readonly SelectOption<number>[] = [
  { value: 11, label: 'Acme Engineering' },
  { value: 12, label: 'Acme Tooling' },
];

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Customer">
      <app-combobox formControlName="customer" [loader]="loader" [debounceMs]="debounceMs" />
    </app-form-field>
  </form>`;

async function setup(
  loader: (query: string) => Promise<readonly SelectOption<number>[]>,
  debounceMs = 300,
) {
  const form = new FormGroup({
    customer: new FormControl<SelectOption<number> | null>(null),
  });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, ComboboxComponent],
    componentProperties: { form, loader, debounceMs },
  });
  const combobox = view.fixture.debugElement.query(By.directive(ComboboxComponent))
    .componentInstance as ComboboxComponent<number>;
  return { form, view, combobox, input: screen.getByRole<HTMLInputElement>('combobox') };
}

/** Type into the field without waiting for the debounce. */
async function type(input: HTMLInputElement, text: string): Promise<void> {
  await userEvent.type(input, text);
}

describe('app-combobox', () => {
  beforeAll(installMatchMedia);

  it('is labelled by app-form-field and is a real text entry', async () => {
    const { input } = await setup(() => Promise.resolve(CUSTOMERS));

    expect(screen.getByLabelText('Customer')).toBe(input);
    expect(input.tagName).toBe('INPUT');
  });

  it('debounces: many keystrokes produce one call to the loader', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    const loader = vi.fn(() => Promise.resolve(CUSTOMERS));
    const { input } = await setup(loader, 300);

    await type(input, 'Acme');
    expect(loader).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(300);

    expect(loader).toHaveBeenCalledTimes(1);
    expect(loader).toHaveBeenCalledWith('Acme');
    vi.useRealTimers();
  });

  it('shows a loading row while the query is in flight', async () => {
    let release: (value: readonly SelectOption<number>[]) => void = () => undefined;
    const loader = () =>
      new Promise<readonly SelectOption<number>[]>((resolve) => {
        release = resolve;
      });
    const { view, combobox, input } = await setup(loader, 0);

    await type(input, 'Ac');
    await vi.waitFor(() => expect(combobox.loading()).toBe(true));
    view.fixture.detectChanges();

    expect(screen.getByText('Searching…')).toBeDefined();

    release(CUSTOMERS);
  });

  it('holds the previous list while refetching', async () => {
    let release: (value: readonly SelectOption<number>[]) => void = () => undefined;
    let calls = 0;
    const loader = () => {
      calls += 1;
      if (calls === 1) {
        return Promise.resolve(CUSTOMERS);
      }
      return new Promise<readonly SelectOption<number>[]>((resolve) => {
        release = resolve;
      });
    };
    const { combobox, input } = await setup(loader, 0);

    await type(input, 'Ac');
    await vi.waitFor(() => expect(combobox.suggestions()).toHaveLength(2));

    await type(input, 'm');
    await vi.waitFor(() => expect(combobox.loading()).toBe(true));

    // The old results are still on screen; a typeahead that blanks its list
    // on every keystroke is unusable at speed.
    expect(combobox.suggestions()).toHaveLength(2);

    release([]);
  });

  it('shows an explicit no-results row naming the query', async () => {
    const { view, combobox, input } = await setup(() => Promise.resolve([]), 0);

    await type(input, 'zzz');
    await vi.waitFor(() => expect(combobox.searched()).toBe(true));
    view.fixture.detectChanges();

    expect(screen.getByText(/No results for/)).toBeDefined();
    expect(screen.getByText(/zzz/)).toBeDefined();
  });

  it('shows an error row with a retry, and retrying calls the loader again', async () => {
    let attempts = 0;
    const loader = () => {
      attempts += 1;
      return attempts === 1
        ? Promise.reject(new Error('Search service unavailable.'))
        : Promise.resolve(CUSTOMERS);
    };
    const { view, combobox, input } = await setup(loader, 0);

    await type(input, 'Ac');
    await vi.waitFor(() => expect(combobox.errorMessage()).toBe('Search service unavailable.'));
    view.fixture.detectChanges();

    expect(screen.getByText('Search service unavailable.')).toBeDefined();

    await userEvent.click(screen.getByRole('button', { name: 'Retry', hidden: true }));
    await vi.waitFor(() => expect(combobox.suggestions()).toHaveLength(2));

    expect(combobox.errorMessage()).toBeNull();
  });

  it('does not call the loader below the minimum query length', async () => {
    const loader = vi.fn(() => Promise.resolve(CUSTOMERS));
    const { input } = await setup(loader, 0);

    await userEvent.clear(input);
    await new Promise((resolve) => setTimeout(resolve, 10));

    expect(loader).not.toHaveBeenCalled();
  });

  it('writes the selected option to the FormGroup and marks it touched', async () => {
    const { form, combobox } = await setup(() => Promise.resolve(CUSTOMERS));

    combobox.onSelect(CUSTOMERS[0]!);

    expect(form.value.customer).toEqual({ value: 11, label: 'Acme Engineering' });
    expect(form.controls.customer.touched).toBe(true);
  });

  it('clears back to null', async () => {
    const { form, combobox } = await setup(() => Promise.resolve(CUSTOMERS));

    combobox.onSelect(CUSTOMERS[0]!);
    combobox.onClear();

    expect(form.value.customer).toBeNull();
  });

  it('reaches the field with a single Tab', async () => {
    const { input } = await setup(() => Promise.resolve(CUSTOMERS));

    await userEvent.tab();

    expect(document.activeElement).toBe(input);
  });

  it('tracks the highlighted option with aria-activedescendant', async () => {
    const { view, combobox, input } = await setup(() => Promise.resolve(CUSTOMERS), 0);

    await type(input, 'Ac');
    await vi.waitFor(() => expect(combobox.suggestions()).toHaveLength(2));
    view.fixture.detectChanges();
    await userEvent.keyboard('{ArrowDown}');
    view.fixture.detectChanges();

    expect(input.getAttribute('aria-activedescendant')).not.toBeNull();
  });
});
