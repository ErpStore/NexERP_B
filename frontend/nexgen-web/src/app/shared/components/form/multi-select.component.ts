import { ChangeDetectionStrategy, Component, computed, forwardRef, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MultiSelect } from 'primeng/multiselect';

import { BaseFormControl } from './base-control';
import type { SelectOption } from './types';

/**
 * Several choices from a list the caller already has - typically a filter.
 *
 * Chips show what is selected, and `Backspace` removes the most recent one.
 * PrimeNG 22.1.0's `MultiSelect.onKeyDown` has no `Backspace` case of its own
 * (unlike `AutoComplete`), so this control adds it - guarded so that a
 * `Backspace` inside the filter box still edits the filter text.
 *
 * The same explicit empty / loading / error triad as `app-select`.
 */
@Component({
  selector: 'app-multi-select',
  templateUrl: './multi-select.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MultiSelect, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultiSelectComponent),
      multi: true,
    },
  ],
})
export class MultiSelectComponent<TValue = unknown> extends BaseFormControl<TValue[]> {
  readonly options = input.required<readonly SelectOption<TValue>[]>();
  readonly loading = input(false);
  readonly errorMessage = input<string | undefined>(undefined);
  readonly emptyMessage = input('No options');
  readonly filter = input(true);

  /** PrimeNG mutates the array it is handed, so it gets a copy, not our input. */
  readonly visibleOptions = computed(() =>
    this.loading() || this.errorMessage() ? [] : [...this.options()],
  );

  constructor() {
    super();
    // PrimeNG renders the combobox on a span, which a native label cannot
    // name; the field label is wired with aria-labelledby instead.
    this.field?.useGroupLabel();
  }

  protected override usesAriaLabelledBy(): boolean {
    return true;
  }

  protected override controlElementSelector = '[role="listbox"], [role="combobox"], input, button';

  /**
   * `Backspace` removes the most recent chip. It is ignored while the filter
   * box holds text, because there the key belongs to the text.
   */
  onKeyDown(event: KeyboardEvent): void {
    if (event.key !== 'Backspace') {
      return;
    }
    // PrimeNG's trigger is itself a hidden input whose `value` is the current
    // selection, so "is this an input" is not the test. The filter box is the
    // only text entry here, and it is not the combobox.
    const target = event.target;
    const inFilterText =
      target instanceof HTMLInputElement &&
      target.getAttribute('role') !== 'combobox' &&
      target.value.length > 0;
    if (inFilterText) {
      return;
    }
    const current = this.value() ?? [];
    if (current.length === 0) {
      return;
    }
    event.preventDefault();
    this.emitValue(current.slice(0, -1));
    this.markTouched();
  }

  onModelChange(value: TValue[] | null): void {
    this.emitValue(value ?? []);
    this.markTouched();
  }
}
