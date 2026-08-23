import { ChangeDetectionStrategy, Component, forwardRef, inject, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DatePicker } from 'primeng/datepicker';

import { BaseFormControl } from './base-control';
import { DATE_FORMAT } from './types';

/**
 * A from/to date range - the shape every report filter in the ERP uses.
 *
 * The value is PrimeNG's own range shape: `[from, to]`, with `to` absent
 * while the user is mid-selection. It is not normalised into an object here,
 * because a report screen has to handle the half-selected state anyway and a
 * second shape would just hide it.
 *
 * Typed entry stays available, for the same reason as `app-date-picker`.
 */
@Component({
  selector: 'app-date-range-picker',
  templateUrl: './date-range-picker.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePicker, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateRangePickerComponent),
      multi: true,
    },
  ],
})
export class DateRangePickerComponent extends BaseFormControl<Date[]> {
  private readonly defaultFormat = inject(DATE_FORMAT);

  readonly dateFormat = input<string | undefined>(undefined);
  readonly minDate = input<Date | undefined>(undefined);
  readonly maxDate = input<Date | undefined>(undefined);

  protected override controlElementSelector = 'input';

  format(): string {
    return this.dateFormat() ?? this.defaultFormat;
  }

  onModelChange(next: Date[] | null): void {
    this.emitValue(next);
    this.markTouched();
  }
}
