import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputText } from 'primeng/inputtext';

import { BaseFormControl } from './base-control';

/**
 * Single-line text. Replaces `TrimmedInputText.razor` for SPA screens.
 *
 * **Trim on commit, not while typing.** Confirmed at
 * `V.SMART/V.SMART.Shared/Components/TrimmedInputText.razor:9` (`@onblur`) and
 * `:21-31` (`TrimInput`). Trimming mid-keystroke would fight the user and
 * move the caret; trailing whitespace in a document number or an item code is
 * a real data-quality problem, so it is cleaned when the field is left.
 *
 * One quirk is reproduced deliberately rather than silently improved: the
 * Blazor guard is `if (!string.IsNullOrWhiteSpace(CurrentValue))`
 * (`TrimmedInputText.razor:23`), so an **all-whitespace** value is left
 * exactly as typed and is *not* collapsed to empty. Changing that would
 * change what 140 screens submit. Recorded as Q-73 in
 * `docs/kb/open-questions.md`.
 */
@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [InputText],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TextInputComponent), multi: true },
  ],
})
export class TextInputComponent extends BaseFormControl<string> {
  /** Trim on commit. On by default - see the class comment for why. */
  readonly trim = input(true);
  readonly maxlength = input<number | undefined>(undefined);
  readonly inputType = input<'text' | 'email' | 'tel' | 'url' | 'password'>('text');

  onInput(event: Event): void {
    this.emitValue((event.target as HTMLInputElement).value);
  }

  onBlur(): void {
    const current = this.value();
    if (this.trim() && current !== null && current.trim().length > 0) {
      const trimmed = current.trim();
      if (trimmed !== current) {
        this.emitValue(trimmed);
      }
    }
    this.markTouched();
  }
}
