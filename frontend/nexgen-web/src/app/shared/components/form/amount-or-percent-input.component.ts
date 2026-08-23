import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  inject,
  input,
} from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputNumber } from 'primeng/inputnumber';
import { SelectButton } from 'primeng/selectbutton';

import { BaseFormControl } from './base-control';
import {
  DECIMAL_PORT,
  PRECISION_POLICY,
  type AmountOrPercent,
  type DecimalPort,
  type Money,
} from './types';

/**
 * The recurring discount, packing, insurance and TCS control: a number plus a
 * mode flag.
 *
 * Confirmed on the document model at
 * `V.SMART/V.SMART.Shared/Data/OutSourcing/Debit_Note/DebitNote.cs:95`
 * (DiscAmtOrPer), `:109` (PackingAmtOrPer), `:117` (InsuranceAmtOrPer),
 * `:146` (TCSAmtOrPer), all bool defaulting to true, and repeated on every
 * transactional document.
 *
 * Polarity, Confirmed at
 * `V.SMART/V.SMART.Shared/Services/CalculationService.cs:29,38,41`:
 * true means the number is a fixed AMOUNT, false means it is a PERCENT.
 * `isAmount` is that flag unnegated.
 *
 * ## It computes nothing
 *
 * Switching the mode does not convert the number. Applying the flag is
 * BR-CALC-001, `CalculationService.UpdateTotalsAsync`, server-side. This file
 * contains no arithmetic operator at all, and
 * `amount-or-percent-input.component.spec.ts` asserts that mechanically so
 * the guarantee cannot rot.
 *
 * Note for whoever wires this to an endpoint: the server stores the two modes
 * in SEPARATE properties (DiscountAmount vs DiscountPercent, PackingCharges
 * vs PackingPercent), not as one value plus a flag. The projection from this
 * control onto that pair is an API contract question, recorded as Q-74 in
 * `docs/kb/open-questions.md`. Getting it backwards silently misprices
 * documents.
 *
 * Trap, so nobody reads it as evidence of the opposite: the e-invoice
 * boundary inverts the flag on the wire (EinvoiceDatabaseService.cs:1570 and
 * EWayDatabaseService.cs:1047 write `MainDiscAmtOrPer = d.DiscAmtOrPer == true
 * ? false : true`). That is the external payload convention, not the domain
 * semantics.
 */
@Component({
  selector: 'app-amount-or-percent-input',
  templateUrl: './amount-or-percent-input.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [InputNumber, SelectButton, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AmountOrPercentInputComponent),
      multi: true,
    },
  ],
})
export class AmountOrPercentInputComponent extends BaseFormControl<AmountOrPercent> {
  private readonly decimal = inject<DecimalPort | null>(DECIMAL_PORT, { optional: true });
  private readonly precision = inject(PRECISION_POLICY);

  readonly decimalPlaces = input<number | undefined>(undefined);
  readonly amountLabel = input('Amount');
  readonly percentLabel = input('Percent');

  protected override controlElementSelector = 'input';

  readonly modeOptions = computed(() => [
    { label: this.amountLabel(), value: true },
    { label: this.percentLabel(), value: false },
  ]);

  /** Defaults to true, matching the server default on every ...AmtOrPer column. */
  readonly isAmount = computed(() => this.value()?.isAmount ?? true);
  readonly modeLabel = computed(() => (this.isAmount() ? this.amountLabel() : this.percentLabel()));
  readonly amount = computed(() => this.value()?.value ?? null);
  readonly scale = computed(() => this.decimalPlaces() ?? this.precision.currencyScale);
  readonly decimalUnavailable = computed(() => this.decimal === null);
  readonly displayNumber = computed(() => this.decimal?.toNumber(this.amount()) ?? null);

  onNumberChange(next: number | null): void {
    const port = this.decimal;
    if (!port) {
      return;
    }
    this.emitValue({
      value: port.fromNumber<Money>(next, this.scale()),
      isAmount: this.isAmount(),
    });
    this.markTouched();
  }

  /**
   * Flip the mode. The number is carried across untouched: an amount of 100
   * becomes a percent of 100, not 1. Converting here would be applying the
   * flag, which is the server side of BR-CALC-001.
   */
  onModeChange(isAmount: boolean): void {
    this.emitValue({ value: this.amount(), isAmount });
    this.markTouched();
  }
}
