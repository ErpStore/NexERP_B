import { ChangeDetectionStrategy, Component, forwardRef, inject, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DatePicker } from 'primeng/datepicker';

import { BaseFormControl } from './base-control';
import { DATE_FORMAT } from './types';

/**
 * A single date.
 *
 * **Typed entry is always available and the calendar is never the only path.**
 * An operator entering fifty documents a shift types `2026-08-23` faster than
 * anyone can click a calendar, so `readonlyInput` stays off; the calendar
 * opens with `ArrowDown` for the cases where picking is genuinely easier.
 *
 * No financial-year helper lives here. That mirrors `FinancialYearHelper.cs`
 * and belongs to the header FY selector (M2-C03).
 *
 * Deviation from the task's implementation step 9, recorded rather than
 * hidden: `date-fns` is **not** used. It is not installed, adding it changes
 * `package.json` and `package-lock.json`, and `p-datepicker` already parses
 * and formats the one format this control needs. If a later task needs real
 * date arithmetic, that is the moment to make the dependency decision.
 */
@Component({
  selector: 'app-date-picker',
  templateUrl: './date-picker.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePicker, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DatePickerComponent), multi: true },
  ],
})
export class DatePickerComponent extends BaseFormControl<Date> {
  private readonly defaultFormat = inject(DATE_FORMAT);

  readonly dateFormat = input<string | undefined>(undefined);
  readonly minDate = input<Date | undefined>(undefined);
  readonly maxDate = input<Date | undefined>(undefined);

  protected override controlElementSelector = 'input';

  format(): string {
    return this.dateFormat() ?? this.defaultFormat;
  }

  onModelChange(next: Date | null): void {
    this.emitValue(next);
    this.markTouched();
  }
}
