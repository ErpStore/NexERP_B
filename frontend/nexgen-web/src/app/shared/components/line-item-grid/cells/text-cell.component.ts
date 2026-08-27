import { ChangeDetectionStrategy, Component, effect, forwardRef } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { TextInputComponent } from '../../form/text-input.component';
import { LineItemCellBase } from './base-cell';

/**
 * A free-text cell - remarks, a reference number, anything shape-only.
 *
 * This component is itself the `ControlValueAccessor` the row's `FormGroup`
 * binds to (via `[formControl]` in `line-item-row.component`); `app-text-input`
 * inside it is driven through a **private, internal** `FormControl` -
 * `ReactiveFormsModule` only, deliberately not `NgModel`. `NgModel`'s own
 * change-detection notification does not compose as precisely with `OnPush`
 * under this workspace's zoneless configuration: `line-item-grid.render-performance.spec.ts`
 * caught it directly - typing in one row re-rendered all 200 until this was
 * the fix. `[formControl]` has no such gap, and `LineItemGrid` is exactly
 * the component 200 of these sit on one screen for.
 */
@Component({
  selector: 'app-line-item-text-cell',
  template: `<app-text-input
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
    (focusout)="commit()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TextInputComponent, ReactiveFormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TextCellComponent), multi: true },
  ],
})
export class TextCellComponent extends LineItemCellBase<string> {
  protected readonly inner = new FormControl('', { nonNullable: true });

  constructor() {
    super();
    effect(() => this.inner.setValue(this.value() ?? '', { emitEvent: false }));
    this.inner.valueChanges.subscribe((next) => this.emitValue(next));
  }
}
