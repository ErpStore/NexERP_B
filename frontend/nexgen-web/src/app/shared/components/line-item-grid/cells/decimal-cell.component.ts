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

import { format, parseUserInput, type Money } from '../../../utils/decimal';
import { TextInputComponent } from '../../form/text-input.component';
import { LineItemCellBase } from './base-cell';

/**
 * A money cell. Parses and formats **only** through M2-C10's real decimal
 * module (`shared/utils/decimal`) - never through `app-currency-input`.
 *
 * That is a deliberate deviation from instruction 8's literal "wrap an
 * M2-C04-02 control", recorded rather than silently taken: `app-currency-input`
 * (M2-C04-02) is typed against `DECIMAL_PORT`
 * (`shared/components/form/types.ts`), and that port has **no real
 * implementation anywhere in the app** - `types.ts` still carries its own
 * `TODO(M2-C10): provide the real implementation`, and the only `DecimalPort`
 * that exists is `fake-decimal-port.ts`, which is test-only. Wiring the real
 * port there is a cross-cutting fix to a file this task does not own the
 * scope to change (every numeric control app-wide, not just this grid) - see
 * this file's `README.md` note and the task's Close-out for the finding.
 * `app-text-input` (also M2-C04-02, and unaffected by the gap) is wrapped
 * instead, with this cell doing the decimal-safe parse/format itself, in the
 * one file M2-C10 already licenses to import `shared/utils/decimal` from
 * outside its own directory.
 *
 * `app-text-input` is driven through a **private, internal** `FormControl` -
 * `ReactiveFormsModule` only, not `NgModel`. See `text-cell.component.ts`'s
 * class comment for why: `NgModel`'s notification path does not compose
 * precisely enough with `OnPush` under this workspace's zoneless
 * configuration, and `line-item-grid.render-performance.spec.ts` /
 * `line-item-grid.decimal-safety.spec.ts` both caught the symptom directly.
 */
@Component({
  selector: 'app-line-item-decimal-cell',
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
      useExisting: forwardRef(() => DecimalCellComponent),
      multi: true,
    },
  ],
})
export class DecimalCellComponent extends LineItemCellBase<Money> {
  /** Never a literal at the call site - the column config supplies it from server settings. */
  readonly decimalPlaces = input.required<number>();

  readonly inner = new FormControl('', { nonNullable: true });
  readonly #draftOverride = signal<string | null>(null);

  /** What the input shows: the operator's own typing while it is in flight, the formatted value once it is not. */
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
    const parsed = parseUserInput(text, { places: this.decimalPlaces() });
    if (parsed.kind === 'value') {
      this.emitValue(parsed.value);
    }
    // 'empty' / 'incomplete' / 'error': the FormControl keeps its last valid
    // value - never a guessed zero - while the draft still shows exactly
    // what was typed, per `parseUserInput`'s own contract.
  }

  onBlur(): void {
    const text = this.#draftOverride();
    if (text !== null) {
      const parsed = parseUserInput(text, { places: this.decimalPlaces() });
      if (parsed.kind === 'value') {
        this.emitValue(parsed.value);
      } else if (parsed.kind !== 'empty') {
        // An error or a still-incomplete entry at blur time cannot become
        // the committed value - the cell snaps back to display formatting
        // of whatever was last genuinely valid, rather than keeping
        // unparsed text on screen indefinitely.
      } else {
        this.emitValue(null);
      }
    }
    this.#draftOverride.set(null);
    this.commit();
  }
}
