import {
  Decimal,
  DecimalBoundaryError,
  InvalidDecimalError,
  brand,
  toDecimal,
  type DecimalInput,
  type Money,
  type Numeric,
  type Qty,
} from './decimal';

/* ------------------------------------------------------------------ *
 * Constructors
 * ------------------------------------------------------------------ */

/** The only way to make a {@link Money}. Throws on NaN, Infinity, blank. */
export function money(input: DecimalInput): Money {
  return brand<Money>(toDecimal(input));
}

/** The only way to make a {@link Qty}. Throws on NaN, Infinity, blank. */
export function qty(input: DecimalInput): Qty {
  return brand<Qty>(toDecimal(input));
}

/** Zero money. Note this is a *value*, never a stand-in for "absent". */
export const ZERO: Money = money(0);

/** Zero quantity. */
export const ZERO_QTY: Qty = qty(0);

/**
 * Runtime guard. The brand is erased at runtime, so this proves only that the
 * value was produced by this module - it cannot distinguish Money from Qty.
 * That distinction is a compile-time one and is deliberately not faked here.
 */
export function isMoney(value: unknown): value is Money {
  return value instanceof Decimal;
}

/** See {@link isMoney} for the runtime/compile-time caveat. */
export function isQty(value: unknown): value is Qty {
  return value instanceof Decimal;
}

/* ------------------------------------------------------------------ *
 * The wire boundary (INV-032)
 * ------------------------------------------------------------------ */

/**
 * The single crossing point for a server `decimal`.
 *
 * INV-032, sub-finding 1: `V.SMART.Api` registers no `AddJsonOptions`, no
 * custom `JsonConverter` and no Newtonsoft serialiser, so ASP.NET Core's
 * default `System.Text.Json` applies and a C# `decimal` arrives as a **JSON
 * number**. A JSON string is accepted here as well, because a string is the
 * representation this module recommends the API move to (raised as a finding
 * for M2-B10 / M2-A06 - it is NOT implemented server-side by this task).
 *
 * Throws {@link DecimalBoundaryError} - naming `field` - for `null`,
 * `undefined`, `NaN`, `''` and anything else that is not a finite decimal.
 * It never substitutes zero.
 */
export function fromApi(value: unknown, field: string): Money {
  return brand<Money>(decodeWire(value, field));
}

/** {@link fromApi} for quantities. Same contract, same errors. */
export function qtyFromApi(value: unknown, field: string): Qty {
  return brand<Qty>(decodeWire(value, field));
}

function decodeWire(value: unknown, field: string): Decimal {
  if (value === null || value === undefined) {
    throw new DecimalBoundaryError(field, value, 'null or undefined');
  }
  if (typeof value !== 'number' && typeof value !== 'string' && !(value instanceof Decimal)) {
    throw new DecimalBoundaryError(field, value, `unsupported wire type ${typeof value}`);
  }
  try {
    return toDecimal(value);
  } catch (error) {
    const reason = error instanceof InvalidDecimalError ? error.message : 'not a decimal';
    throw new DecimalBoundaryError(field, value, reason);
  }
}

/**
 * The reverse crossing. Returns a JSON **number** when the value round-trips
 * through IEEE-754 without losing a digit - which is what the API's
 * `System.Text.Json` reads today - and the exact decimal **string** when it
 * would not. Silently shipping a lossy number was the alternative and is worse:
 * the caller would never learn that a 20-digit amount had been truncated.
 *
 * The string branch is only reachable for values beyond ~15 significant digits,
 * which `decimal(18, n)` storage can hold. That the current API would reject
 * such a string is precisely the finding recorded for M2-B10 / M2-A06.
 */
export function toApi(value: Numeric): string | number {
  const asNumber = value.toNumber();
  if (Number.isFinite(asNumber) && new Decimal(asNumber).eq(value)) {
    return asNumber;
  }
  return value.toFixed();
}

/* ------------------------------------------------------------------ *
 * Arithmetic - explicit, total, and NOT ERP calculation
 *
 * There is no `calculateLineTotal`, no `applyTax`, no `computeGrandTotal` and
 * there never will be: BR-CALC-001 has exactly one implementation and it is
 * `CalculationService.UpdateTotalsAsync` on the server.
 * ------------------------------------------------------------------ */

export function add<T extends Numeric>(a: T, b: T): T {
  return brand<T>(a.plus(b));
}

export function sub<T extends Numeric>(a: T, b: T): T {
  return brand<T>(a.minus(b));
}

export function mul<T extends Numeric>(a: T, factor: Numeric | number | string): T {
  return brand<T>(a.times(toDecimal(factor)));
}

export function div<T extends Numeric>(a: T, divisor: Numeric | number | string): T {
  const d = toDecimal(divisor);
  if (d.isZero()) {
    throw new InvalidDecimalError(divisor, 'division by zero');
  }
  return brand<T>(a.dividedBy(d));
}

export function neg<T extends Numeric>(a: T): T {
  return brand<T>(a.negated());
}

export function abs<T extends Numeric>(a: T): T {
  return brand<T>(a.absoluteValue());
}

/* ------------------------------------------------------------------ *
 * Comparison - `===` on a wrapped value is meaningless, never use it
 * ------------------------------------------------------------------ */

export function eq(a: Numeric, b: Numeric): boolean {
  return a.equals(b);
}

export function lt(a: Numeric, b: Numeric): boolean {
  return a.lessThan(b);
}

export function lte(a: Numeric, b: Numeric): boolean {
  return a.lessThanOrEqualTo(b);
}

export function gt(a: Numeric, b: Numeric): boolean {
  return a.greaterThan(b);
}

export function gte(a: Numeric, b: Numeric): boolean {
  return a.greaterThanOrEqualTo(b);
}

export function cmp(a: Numeric, b: Numeric): -1 | 0 | 1 {
  return a.comparedTo(b) as -1 | 0 | 1;
}

export function isZero(a: Numeric): boolean {
  return a.isZero();
}

/* ------------------------------------------------------------------ *
 * Rounding - always explicit, never implicit
 * ------------------------------------------------------------------ */

/**
 * The rounding modes this module admits. `half-up` is the default because it is
 * what the server does (INV-032 sub-finding 3, `CalculationService.cs:103`).
 */
export type RoundingMode = 'half-up' | 'half-even' | 'up' | 'down' | 'ceil' | 'floor';

/** Server-matching default. Changing it is a business decision, not a tidy-up. */
export const DEFAULT_ROUNDING_MODE: RoundingMode = 'half-up';

const ROUNDING: Record<RoundingMode, Decimal.Rounding> = {
  'half-up': Decimal.ROUND_HALF_UP,
  'half-even': Decimal.ROUND_HALF_EVEN,
  up: Decimal.ROUND_UP,
  down: Decimal.ROUND_DOWN,
  ceil: Decimal.ROUND_CEIL,
  floor: Decimal.ROUND_FLOOR,
};

export function round<T extends Numeric>(
  value: T,
  places: number,
  mode: RoundingMode = DEFAULT_ROUNDING_MODE,
): T {
  if (!Number.isInteger(places) || places < 0 || places > 20) {
    throw new InvalidDecimalError(places, 'decimal places must be an integer in 0..20');
  }
  return brand<T>(value.toDecimalPlaces(places, ROUNDING[mode]));
}
