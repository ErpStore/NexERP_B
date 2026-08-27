import { ChangeDetectionStrategy, Component, effect, forwardRef, input } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { SelectComponent } from '../../form/select.component';
import type { SelectOption } from '../../form/types';
import { LineItemCellBase } from './base-cell';

/**
 * A closed choice from a caller-supplied list - the column's `options`.
 * Never fetches. Internal `FormControl`, not `NgModel` - see
 * `text-cell.component.ts`'s class comment.
 */
@Component({
  selector: 'app-line-item-select-cell',
  template: `<app-select
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
    [options]="options()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SelectComponent, ReactiveFormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SelectCellComponent), multi: true },
  ],
})
export class SelectCellComponent<TValue = unknown> extends LineItemCellBase<TValue> {
  readonly options = input.required<readonly SelectOption<TValue>[]>();

  readonly inner = new FormControl<TValue | null>(null);

  constructor() {
    super();
    effect(() => this.inner.setValue(this.value(), { emitEvent: false }));
    this.inner.valueChanges.subscribe((next) => {
      this.emitValue(next);
      this.commit();
    });
  }
}
