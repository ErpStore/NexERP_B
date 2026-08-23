import { Directive, computed, inject, input } from '@angular/core';

import { BaseFormControl } from './base-control';
import { DECIMAL_PORT, PRECISION_POLICY, type DecimalPort, type ScaledDecimal } from './types';

/**
 * What the three numeric controls share.
 *
 * **They do no arithmetic and no parsing.** Every conversion goes through the
 * injected {@link DecimalPort}, which M2-C10 owns. A `parseFloat` here is
 * precisely the defect M2-C10 exists to prevent, and it would stay invisible
 * until an invoice failed to reconcile.
 *
 * TODO(M2-C10): the port has no implementation yet. Until one is provided the
 * controls render disabled with a visible explanation rather than silently
 * accepting input they cannot represent. See `form/README.md`.
 */
@Directive()
export abstract class NumericFormControl<
  TValue extends ScaledDecimal,
> extends BaseFormControl<TValue> {
  protected readonly decimal = inject<DecimalPort | null>(DECIMAL_PORT, { optional: true });
  protected readonly precision = inject(PRECISION_POLICY);

  /** Override the tenant's scale for one field. Still not a literal in a component. */
  readonly decimalPlaces = input<number | undefined>(undefined);

  /** True when M2-C10 has not been provided; the control says so instead of lying. */
  readonly decimalUnavailable = computed(() => this.decimal === null);

  /** The scale this control renders at. Defaults come from the injected policy. */
  abstract readonly scale: () => number;

  /** The `number` `p-inputnumber` needs. The conversion belongs to the port, not here. */
  readonly displayNumber = computed(() => this.decimal?.toNumber(this.value()) ?? null);

  protected commitNumber(next: number | null): void {
    const port = this.decimal;
    if (!port) {
      return;
    }
    this.emitValue(port.fromNumber<TValue>(next, this.scale()));
    this.markTouched();
  }
}
