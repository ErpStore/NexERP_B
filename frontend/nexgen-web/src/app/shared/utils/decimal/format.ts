import type { Numeric } from './decimal';
import { getDefaultDecimalPlaces } from './precision';
import { DEFAULT_ROUNDING_MODE, round, type RoundingMode } from './money';

/**
 * What an *absent* value renders as. An absent amount is never `0.00`:
 * "we do not have a value" and "the value is zero" are different facts and the
 * UI must not conflate them (KB-051).
 */
export const ABSENT_DISPLAY = '—';

export interface FormatOptions {
  /** Defaults to the precision policy (see `precision.ts`), i.e. 2 today. */
  places?: number;
  /** BCP-47 tag. Defaults to the runtime locale. */
  locale?: string;
  /** ISO-4217 code. When given, `Intl` currency style is used. */
  currency?: string;
  /** Rounding applied for display only. Defaults to the server's mode. */
  mode?: RoundingMode;
  /** Rendered for `null`/`undefined`. Defaults to an em dash. */
  absent?: string;
}

/**
 * Display formatting. Presentation only - it never changes a stored value.
 *
 * The value is rounded by `decimal.js` first (exactly, in the server's mode) and
 * the resulting fixed-point *string* is handed to `Intl.NumberFormat`, so the
 * number never passes through an IEEE-754 double on the way to the screen.
 *
 * Render the result with `font-variant-numeric: tabular-nums`, right-aligned in
 * tables (KB-051 §Typography). Negative values carry an explicit minus sign;
 * parentheses-only negatives are not acceptable.
 */
export function format(value: Numeric | null | undefined, options: FormatOptions = {}): string {
  const absent = options.absent ?? ABSENT_DISPLAY;
  if (value === null || value === undefined) {
    return absent;
  }
  const places = options.places ?? getDefaultDecimalPlaces();
  const mode = options.mode ?? DEFAULT_ROUNDING_MODE;
  const fixed = round(value, places, mode).toFixed(places);

  const formatter = new Intl.NumberFormat(options.locale, {
    minimumFractionDigits: places,
    maximumFractionDigits: places,
    ...(options.currency === undefined
      ? {}
      : { style: 'currency' as const, currency: options.currency }),
  });
  return formatDecimalString(formatter, fixed);
}

/**
 * The screen-reader label for a formatted value. A leading minus sign is a
 * single glyph that many screen readers skip, so it is spoken explicitly.
 */
export function formatAriaLabel(
  value: Numeric | null | undefined,
  options: FormatOptions = {},
): string {
  const text = format(value, options);
  return text.startsWith('-') ? `minus ${text.slice(1)}` : text;
}

/**
 * ECMA-402 (Intl NumberFormat V3) accepts a decimal *string* and formats it
 * without coercing through an IEEE-754 double - which is the whole point of
 * doing it this way. TypeScript's ES2022 `lib` still types `format` as
 * `(number | bigint)`, so the overload is reached through one narrow assertion,
 * kept here rather than repeated at every call site.
 */
function formatDecimalString(formatter: Intl.NumberFormat, value: string): string {
  return (formatter as unknown as { format(input: string): string }).format(value);
}
