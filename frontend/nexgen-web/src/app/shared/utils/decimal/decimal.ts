import Decimal from 'decimal.js';

/**
 * The ONLY file in the application that imports `decimal.js`.
 *
 * ESLint (`no-restricted-imports`, see `eslint.config.js`) and
 * `no-float-money.spec.ts` both enforce that; if you are reading this because
 * you wanted a `Decimal` somewhere else, the answer is to add a function here
 * and export it from `index.ts`.
 *
 * Rounding mode (INV-032, sub-finding 3, Confirmed): the server rounds with
 * `MidpointRounding.AwayFromZero` -
 * `V.SMART/V.SMART.Shared/Services/CalculationService.cs:103`
 * (`Math.Round(preRoundGrandTotal, 0, MidpointRounding.AwayFromZero)`), which is
 * `decimal.js`'s `ROUND_HALF_UP` ("rounds towards nearest neighbour; if
 * equidistant, away from zero"). This mirrors the server so that a *display*
 * rounding never disagrees with the authoritative one - it does NOT license the
 * client to compute a round-off. BR-CALC-001 stays server-side.
 *
 * `precision: 34` is IEEE-754 decimal128's significand, comfortably wider than
 * SQL Server's `decimal(18, n)` storage (KB-012), so no arithmetic performed for
 * a preview can lose a digit the server would have kept.
 */
Decimal.set({
  precision: 34,
  rounding: Decimal.ROUND_HALF_UP,
  toExpNeg: -9,
  toExpPos: 21,
});

export { Decimal };

declare const MONEY_BRAND: unique symbol;
declare const QTY_BRAND: unique symbol;

/**
 * A monetary value. Branded so that a bare `number` or a bare `Decimal` is a
 * *compile error* where `Money` is expected - without the brand every rule in
 * this module would be advisory only.
 */
export type Money = Decimal & { readonly [MONEY_BRAND]: 'Money' };

/** A quantity. Distinct from {@link Money}: the two never mix silently. */
export type Qty = Decimal & { readonly [QTY_BRAND]: 'Qty' };

/** Either branded kind. Used by the arithmetic and comparison helpers. */
export type Numeric = Money | Qty;

/** What a constructor will accept. Deliberately excludes `null`/`undefined`. */
export type DecimalInput = string | number | Decimal;

/**
 * Thrown by the constructors for input that is not a finite decimal.
 * Never returns zero instead - a missing amount displayed as `0.00` is exactly
 * the class of bug this module exists to prevent.
 */
export class InvalidDecimalError extends Error {
  constructor(
    readonly received: unknown,
    reason: string,
  ) {
    super(`Invalid decimal input (${reason}): ${String(received)}`);
    this.name = 'InvalidDecimalError';
  }
}

/**
 * Thrown at the HTTP boundary by `fromApi`, naming the offending field so the
 * message that reaches `GlobalErrorHandler` says which one it was.
 */
export class DecimalBoundaryError extends Error {
  constructor(
    readonly field: string,
    readonly received: unknown,
    reason: string,
  ) {
    super(`Field "${field}" is not a valid decimal (${reason}): ${String(received)}`);
    this.name = 'DecimalBoundaryError';
  }
}

/** Internal: validate and construct. Not exported from `index.ts`. */
export function toDecimal(input: DecimalInput): Decimal {
  if (typeof input === 'number' && !Number.isFinite(input)) {
    throw new InvalidDecimalError(input, 'not a finite number');
  }
  if (typeof input === 'string' && input.trim() === '') {
    throw new InvalidDecimalError(input, 'empty string');
  }
  let value: Decimal;
  try {
    value = new Decimal(input);
  } catch {
    throw new InvalidDecimalError(input, 'not parseable as a decimal');
  }
  if (!value.isFinite()) {
    throw new InvalidDecimalError(input, 'NaN or Infinity');
  }
  return value;
}

/** Internal: apply the compile-time brand. Runtime identity, no allocation. */
export function brand<T extends Numeric>(value: Decimal): T {
  return value as T;
}
