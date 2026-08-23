import { ChangeDetectionStrategy, Component, computed, forwardRef } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputNumber } from 'primeng/inputnumber';

import { NumericFormControl } from './numeric-base';
import type { Money } from './types';

/**
 * An amount of money. Its value is a {@link Money} from M2-C10, never a
 * `number`.
 *
 * The number of decimal places comes from the injected precision policy,
 * which a screen populates from the tenant's company settings - Confirmed
 * `V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:208`,
 * `DecimalPlaces` defaulting to 2. No component hardcodes it.
 */
@Component({
  selector: 'app-currency-input',
  templateUrl: './currency-input.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [InputNumber, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CurrencyInputComponent),
      multi: true,
    },
  ],
})
export class CurrencyInputComponent extends NumericFormControl<Money> {
  readonly scale = computed(() => this.decimalPlaces() ?? this.precision.currencyScale);

  protected override controlElementSelector = 'input';

  onModelChange(next: number | null): void {
    this.commitNumber(next);
  }
}
