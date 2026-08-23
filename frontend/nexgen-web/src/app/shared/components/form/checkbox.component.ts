import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Checkbox } from 'primeng/checkbox';

import { BaseFormControl } from './base-control';

/**
 * A boolean **saved on submit**.
 *
 * The rule that keeps 140 screens consistent: a value that is part of the
 * form and persists when the user presses Save is a **checkbox**. A toggle
 * that takes effect the moment it is flipped is `app-switch`. Used
 * interchangeably, the two teach the user nothing about whether their change
 * has been saved.
 */
@Component({
  selector: 'app-checkbox',
  templateUrl: './checkbox.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Checkbox, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxComponent), multi: true },
  ],
})
export class CheckboxComponent extends BaseFormControl<boolean> {
  /** Text beside the box. The field label above still comes from `app-form-field`. */
  readonly optionLabel = input<string | undefined>(undefined);

  onModelChange(checked: boolean): void {
    this.emitValue(checked);
    this.markTouched();
  }
}
