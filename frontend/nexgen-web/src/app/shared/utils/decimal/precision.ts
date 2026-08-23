/**
 * The decimal-place policy. One place, so that no component ever hardcodes `2`.
 *
 * INV-032, sub-finding 2 (Confirmed): the *display* default comes from the
 * company setting `Companydetails.DecimalPlaces`, whose server-side default is
 * `2` -
 * `V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:208`
 * (`public int DecimalPlaces { get; set; } = 2;`).
 *
 * There is no per-item quantity precision and no per-price-list rate precision
 * anywhere in `V.SMART.Shared/Data` (negative result, INV-032). Storage scale is
 * expressed only as EF Core `[Precision(18, n)]` attributes, and it is NOT
 * uniformly 2 - see the constants below. The company setting governs display;
 * the attributes govern storage. They are different things and are recorded as
 * different constants here so nobody conflates them again.
 */

/** Server-side default of `Companydetails.DecimalPlaces` (Companydetails.cs:208). */
export const SERVER_DEFAULT_DECIMAL_PLACES = 2;

/**
 * Storage scales observed in the EF entities (INV-032, Confirmed). These are
 * the *maximum* meaningful precision a value can carry, not a display default.
 */
export const STORAGE_SCALE = {
  /** Amount / money columns, e.g. `Banks.cs:36,39` `[Precision(18, 2)]`. */
  amount: 2,
  /** Quantity columns, e.g. `StockAdd.cs:36,39` `[Precision(18, 3)]`. */
  quantity: 3,
  /** Unit price / rate columns, e.g. `StockAdd.cs:44` `[Precision(18, 4)]`. */
  rate: 4,
} as const;

let currentDefault: number = SERVER_DEFAULT_DECIMAL_PLACES;

/**
 * The decimal places to use when a caller does not state one.
 *
 * TODO(M3-3-01…14 - Masters: Admin & Settings): once a company-settings
 * endpoint exposes `Companydetails.DecimalPlaces`, an app initialiser calls
 * `setDefaultDecimalPlaces()` with the tenant's value. Until then this returns
 * the server-side default of 2. It is deliberately a single mutable policy
 * rather than a literal sprinkled through components.
 */
export function getDefaultDecimalPlaces(): number {
  return currentDefault;
}

/** Set the tenant's display precision, once, from server settings. */
export function setDefaultDecimalPlaces(places: number): void {
  if (!Number.isInteger(places) || places < 0 || places > 10) {
    throw new RangeError(`Decimal places must be an integer in 0..10, received ${String(places)}`);
  }
  currentDefault = places;
}

/** Restore the server-side default. Exists for tests and for tenant switching. */
export function resetDefaultDecimalPlaces(): void {
  currentDefault = SERVER_DEFAULT_DECIMAL_PLACES;
}
