import { ChangeDetectionStrategy, Component, effect, forwardRef } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { DatePickerComponent } from '../../form/date-picker.component';
import { LineItemCellBase } from './base-cell';

/**
 * A single-date cell - a delivery or reference date on the line. Internal
 * `FormControl`, not `NgModel` - see `text-cell.component.ts`'s class
 * comment.
 */
@Component({
  selector: 'app-line-item-date-cell',
  template: `<app-date-picker
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePickerComponent, ReactiveFormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateCellComponent), multi: true },
  ],
})
export class DateCellComponent extends LineItemCellBase<Date> {
  readonly inner = new FormControl<Date | null>(null);

  constructor() {
    super();
    effect(() => this.inner.setValue(this.value(), { emitEvent: false }));
    this.inner.valueChanges.subscribe((next) => {
      this.emitValue(next);
      this.commit();
    });
  }
}
