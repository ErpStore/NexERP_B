import { ChangeDetectionStrategy, Component, forwardRef } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ToggleSwitch } from 'primeng/toggleswitch';

import { BaseFormControl } from './base-control';

/**
 * A boolean with an **immediate effect** - a filter, a live preference, a
 * display mode.
 *
 * **Do not use it for a form field that is saved on submit.** That is
 * `app-checkbox`. A switch signals "this has already happened"; a checkbox
 * signals "this will happen when you save". Mixing them across 140 screens
 * makes it impossible for an operator to know whether their change stuck.
 */
@Component({
  selector: 'app-switch',
  templateUrl: './switch.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ToggleSwitch, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SwitchComponent), multi: true },
  ],
})
export class SwitchComponent extends BaseFormControl<boolean> {
  onModelChange(checked: boolean): void {
    this.emitValue(checked);
    this.markTouched();
  }
}
