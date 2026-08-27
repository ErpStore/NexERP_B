import { ChangeDetectionStrategy, Component, effect, forwardRef } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { CheckboxComponent } from '../../form/checkbox.component';
import { LineItemCellBase } from './base-cell';

/**
 * A boolean cell - e.g. a per-line cancel flag. Commits immediately on
 * toggle; there is no blur to wait for. Internal `FormControl`, not
 * `NgModel` - see `text-cell.component.ts`'s class comment.
 */
@Component({
  selector: 'app-line-item-checkbox-cell',
  template: `<app-checkbox
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CheckboxComponent, ReactiveFormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CheckboxCellComponent),
      multi: true,
    },
  ],
})
export class CheckboxCellComponent extends LineItemCellBase<boolean> {
  readonly inner = new FormControl(false, { nonNullable: true });

  constructor() {
    super();
    effect(() => this.inner.setValue(this.value() ?? false, { emitEvent: false }));
    this.inner.valueChanges.subscribe((checked) => {
      this.emitValue(checked);
      this.commit();
    });
  }
}
