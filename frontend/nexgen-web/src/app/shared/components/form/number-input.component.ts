import { ChangeDetectionStrategy, Component, computed, forwardRef } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputNumber } from 'primeng/inputnumber';

import { NumericFormControl } from './numeric-base';
import type { Qty } from './types';

/**
 * A quantity. Its value is a {@link Qty} from M2-C10 - **never** a `number`,
 * so no screen can quietly do float arithmetic on it.
 *
 * Right-aligned with tabular figures, because a column of quantities is read
 * by comparing digits in the same position.
 */
@Component({
  selector: 'app-number-input',
  templateUrl: './number-input.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [InputNumber, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NumberInputComponent),
      multi: true,
    },
  ],
})
export class NumberInputComponent extends NumericFormControl<Qty> {
  readonly scale = computed(() => this.decimalPlaces() ?? this.precision.quantityScale);

  protected override controlElementSelector = 'input';

  onModelChange(next: number | null): void {
    this.commitNumber(next);
  }
}
