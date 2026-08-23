import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { RadioButton } from 'primeng/radiobutton';

import { BaseFormControl } from './base-control';
import type { SelectOption } from './types';

let nextRadioGroupId = 0;

/**
 * One of a small, always-visible set - three to five options. More than that
 * and it should be `app-select`, because a radio group costs vertical space
 * that a dense document header does not have.
 *
 * Rendered as native `<input type="radio">` sharing one `name`, so the
 * browser supplies the roving tab stop and arrow-key selection rather than a
 * hand-rolled key handler that will drift.
 */
@Component({
  selector: 'app-radio-group',
  templateUrl: './radio-group.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RadioButton, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RadioGroupComponent), multi: true },
  ],
})
export class RadioGroupComponent<TValue = unknown> extends BaseFormControl<TValue> {
  readonly options = input.required<readonly SelectOption<TValue>[]>();

  readonly groupName = `app-radio-group-${++nextRadioGroupId}`;

  constructor() {
    super();
    // A `<label for>` has no single target in a group; the field label
    // becomes `aria-labelledby` on the radiogroup container instead.
    this.field?.useGroupLabel();
  }

  protected override accessibilityTarget(): HTMLElement | null {
    return this.host.nativeElement.querySelector<HTMLElement>('[role="radiogroup"]');
  }

  optionId(index: number): string {
    return `${this.groupName}-${index}`;
  }

  /**
   * A radio has no native `readonly`. Disabling the buttons would be wrong -
   * PrimeNG drops a disabled radio out of the tab order, so the chosen value
   * would stop being reachable, focusable or copyable, which is exactly the
   * distinction the design system draws. Cancelling the click instead keeps
   * every option focusable and announced (`aria-readonly` on the group says
   * why) while the user agent undoes the pre-click check.
   */
  onOptionClick(event: Event): void {
    if (this.readonly()) {
      event.preventDefault();
    }
  }

  onModelChange(value: TValue): void {
    if (this.readonly()) {
      return;
    }
    this.emitValue(value);
    this.markTouched();
  }
}
