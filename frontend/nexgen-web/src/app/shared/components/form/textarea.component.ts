import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Textarea } from 'primeng/textarea';

import { BaseFormControl } from './base-control';

/**
 * Multi-line text - remarks, terms, addresses.
 *
 * Trims on commit for the same reason `app-text-input` does, but keeps
 * interior newlines: a two-line delivery address is data, trailing blank
 * lines are not.
 */
@Component({
  selector: 'app-textarea',
  templateUrl: './textarea.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Textarea],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TextareaComponent), multi: true },
  ],
})
export class TextareaComponent extends BaseFormControl<string> {
  readonly rows = input(3);
  readonly maxlength = input<number | undefined>(undefined);
  readonly trim = input(true);

  onInput(event: Event): void {
    this.emitValue((event.target as HTMLTextAreaElement).value);
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
