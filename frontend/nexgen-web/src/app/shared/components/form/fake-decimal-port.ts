import Decimal from 'decimal.js';

import type { DecimalPort, Money, Qty, ScaledDecimal } from './types';

/**
 * **Test fixture only.** Not exported from `index.ts`, not provided anywhere
 * in the application.
 *
 * M2-C10 owns the real decimal module. This exists so the numeric controls
 * can be tested before it lands, and it is deliberately thin: if it grew a
 * rounding policy or a currency rule it would become a second implementation
 * of the thing M2-C10 exists to centralise.
 */
interface Boxed {
  readonly decimal: Decimal;
}

function box(decimal: Decimal): ScaledDecimal {
  return { decimal } as unknown as ScaledDecimal;
}

function unbox(value: ScaledDecimal): Decimal {
  return (value as unknown as Boxed).decimal;
}

export const fakeDecimalPort: DecimalPort = {
  parseUserInput<T extends ScaledDecimal = ScaledDecimal>(text: string, scale: number): T | null {
    const trimmed = text.trim();
    if (trimmed === '') {
      return null;
    }
    return box(new Decimal(trimmed).toDecimalPlaces(scale)) as T;
  },
  format(value: ScaledDecimal | null, scale: number): string {
    return value === null ? '' : unbox(value).toDecimalPlaces(scale).toString();
  },
  fromNumber<T extends ScaledDecimal = ScaledDecimal>(
    value: number | null,
    scale: number,
  ): T | null {
    return value === null ? null : (box(new Decimal(value).toDecimalPlaces(scale)) as T);
  },
  toNumber(value: ScaledDecimal | null): number | null {
    return value === null ? null : unbox(value).toNumber();
  },
  equals(a: ScaledDecimal | null, b: ScaledDecimal | null): boolean {
    if (a === null || b === null) {
      return a === b;
    }
    return unbox(a).equals(unbox(b));
  },
};

/** Build a `Money` in a test without going through a control. */
export function money(text: string, scale = 2): Money {
  const parsed = fakeDecimalPort.parseUserInput<Money>(text, scale);
  if (parsed === null) {
    throw new Error('Not a money value: ' + text);
  }
  return parsed;
}

/** Build a `Qty` in a test without going through a control. */
export function qty(text: string, scale = 2): Qty {
  const parsed = fakeDecimalPort.parseUserInput<Qty>(text, scale);
  if (parsed === null) {
    throw new Error('Not a quantity: ' + text);
  }
  return parsed;
}

/** The rendered text of a decimal, for assertions. */
export function asText(value: ScaledDecimal | null, scale = 2): string {
  return fakeDecimalPort.format(value, scale);
}
