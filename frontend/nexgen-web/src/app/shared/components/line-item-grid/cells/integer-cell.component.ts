import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  forwardRef,
  input,
  signal,
} from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { format, parseUserInputAsQty, type Qty } from '../../../utils/decimal';
import { TextInputComponent } from '../../form/text-input.component';
import { LineItemCellBase } from './base-cell';

/**
 * A quantity cell (named `'integer'` in {@link LineItemCellEditorKind} for
 * the common case - a count of pieces - but not restricted to zero decimal
 * places; `decimalPlaces` still comes from the column, for a fractional
 * quantity like a weight). Same deviation and same reasoning as
 * `decimal-cell.component.ts`: parses and formats through
 * `shared/utils/decimal` directly, wrapping `app-text-input` rather than
 * `app-number-input`, because `DECIMAL_PORT` has no real implementation to
 * wrap yet - and, like `decimal-cell.component.ts` and
 * `text-cell.component.ts`, drives it through an internal `FormControl`
 * rather than `NgModel`.
 */
@Component({
  selector: 'app-line-item-integer-cell',
  template: `<app-text-input
    class="app-line-item-cell--numeric"
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
    [trim]="false"
    inputType="text"
    (focusout)="onBlur()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TextInputComponent, ReactiveFormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => IntegerCellComponent),
      multi: true,
    },
  ],
})
export class IntegerCellComponent extends LineItemCellBase<Qty> {
  readonly decimalPlaces = input.required<number>();

  readonly inner = new FormControl('', { nonNullable: true });
  readonly #draftOverride = signal<string | null>(null);

  readonly draft = computed(
    () => this.#draftOverride() ?? format(this.value(), { places: this.decimalPlaces() }),
  );

  constructor() {
    super();
    effect(() => this.inner.setValue(this.draft(), { emitEvent: false }));
    this.inner.valueChanges.subscribe((text) => this.onInput(text));
  }

  onInput(text: string): void {
    this.#draftOverride.set(text);
    const parsed = parseUserInputAsQty(text, { places: this.decimalPlaces() });
    if (parsed.kind === 'value') {
      this.emitValue(parsed.value);
    }
  }

  onBlur(): void {
    const text = this.#draftOverride();
    if (text !== null) {
      const parsed = parseUserInputAsQty(text, { places: this.decimalPlaces() });
      if (parsed.kind === 'value') {
        this.emitValue(parsed.value);
      } else if (parsed.kind === 'empty') {
        this.emitValue(null);
      }
      // 'error' / 'incomplete' at blur: snap back to the last valid value's
      // formatting, same as `decimal-cell.component.ts`.
    }
    this.#draftOverride.set(null);
    this.commit();
  }
}
