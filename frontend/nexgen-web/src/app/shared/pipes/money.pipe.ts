import { Pipe, type PipeTransform } from '@angular/core';

import { format, type FormatOptions, type Numeric } from '../utils/decimal';

/**
 * Display a `Money` or `Qty` in a template.
 *
 * Angular's own `DecimalPipe` and `CurrencyPipe` must **not** be used on a
 * monetary value: both coerce their input to `number`, which is the IEEE-754
 * hazard M2-C10 exists to remove. This pipe delegates to `format()`, which
 * rounds with `decimal.js` and hands a fixed-point string to `Intl`.
 *
 * Pure, because `format()` is pure and `Money` values are immutable.
 *
 * ```html
 * <td class="numeric">{{ line.amount | money }}</td>
 * <td class="numeric">{{ line.quantity | money: { places: 3 } }}</td>
 * ```
 *
 * Apply `font-variant-numeric: tabular-nums` and right-align numeric columns
 * (KB-051 §Typography). An absent value renders as an em dash, never `0.00`.
 */
@Pipe({ name: 'money', pure: true })
export class MoneyPipe implements PipeTransform {
  transform(value: Numeric | null | undefined, options: FormatOptions | number = {}): string {
    return format(value, typeof options === 'number' ? { places: options } : options);
  }
}
