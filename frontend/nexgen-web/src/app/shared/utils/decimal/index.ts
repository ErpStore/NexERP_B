/**
 * The money / quantity module (M2-C10). Everything outside this folder imports
 * from here and never from the files behind it - `decimal.js` itself is
 * imported in exactly one place, `./decimal`.
 *
 * This module formats, parses, compares and (rarely) combines numbers. It does
 * not calculate: BR-CALC-001 (document totals, tax, discount, freight, TCS,
 * round-off) and BR-STK-001 (FIFO stock allocation) live on the server and stay
 * there. See `README.md` in this folder.
 */
export type { Money, Qty, Numeric, DecimalInput } from './decimal';
export { DecimalBoundaryError, InvalidDecimalError } from './decimal';

export {
  money,
  qty,
  ZERO,
  ZERO_QTY,
  isMoney,
  isQty,
  fromApi,
  qtyFromApi,
  toApi,
  add,
  sub,
  mul,
  div,
  neg,
  abs,
  eq,
  lt,
  lte,
  gt,
  gte,
  cmp,
  isZero,
  round,
  DEFAULT_ROUNDING_MODE,
} from './money';
export type { RoundingMode } from './money';

export { format, formatAriaLabel, ABSENT_DISPLAY } from './format';
export type { FormatOptions } from './format';

export { parseUserInput, parseUserInputAsQty } from './parse';
export type { ParseResult, ParseFailure, ParseOptions } from './parse';

export {
  SERVER_DEFAULT_DECIMAL_PLACES,
  STORAGE_SCALE,
  getDefaultDecimalPlaces,
  setDefaultDecimalPlaces,
  resetDefaultDecimalPlaces,
} from './precision';
