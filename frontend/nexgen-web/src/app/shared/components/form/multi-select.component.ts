import { ChangeDetectionStrategy, Component, computed, forwardRef, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MultiSelect } from 'primeng/multiselect';

import { BaseFormControl } from './base-control';
import type { SelectOption } from './types';

/**
 * Several choices from a list the caller already has - typically a filter.
 *
 * `Backspace` removes the last chip; the same explicit empty / loading /
 * error triad as `app-select`.
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

  onModelChange(value: TValue[] | null): void {
    this.emitValue(value ?? []);
    this.markTouched();
  }
}
