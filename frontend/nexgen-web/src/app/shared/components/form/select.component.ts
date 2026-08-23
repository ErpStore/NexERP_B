import { ChangeDetectionStrategy, Component, computed, forwardRef, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';

import { BaseFormControl } from './base-control';
import type { SelectOption } from './types';

/**
 * Single choice from a list the caller already has.
 *
 * It never fetches. If the list has to be searched on the server, that is
 * `app-combobox`.
 *
 * The empty state is explicit: an operator seeing a blank dropdown cannot
 * tell "nothing configured" from "still loading" from "broken".
 */
@Component({
  selector: 'app-select',
  templateUrl: './select.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Select, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SelectComponent), multi: true },
  ],
})
export class SelectComponent<TValue = unknown> extends BaseFormControl<TValue> {
  readonly options = input.required<readonly SelectOption<TValue>[]>();
  readonly loading = input(false);
  /** A message from the caller's failed load. Rendered in the dropdown, not swallowed. */
  readonly errorMessage = input<string | undefined>(undefined);
  readonly showClear = input(true);
  readonly emptyMessage = input('No options');

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

  protected override controlElementSelector = '[role="combobox"], input, button';

  onModelChange(value: TValue | null): void {
    this.emitValue(value);
    this.markTouched();
  }
}
