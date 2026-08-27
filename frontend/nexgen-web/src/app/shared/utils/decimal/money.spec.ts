import { Decimal } from './decimal';
import {
  DEFAULT_ROUNDING_MODE,
  DecimalBoundaryError,
  InvalidDecimalError,
  SERVER_DEFAULT_DECIMAL_PLACES,
  STORAGE_SCALE,
  ZERO,
  ZERO_QTY,
  abs,
  add,
  cmp,
  div,
  eq,
  format,
  fromApi,
  gt,
  gte,
  isMoney,
  isQty,
  isZero,
  lt,
  lte,
  money,
  mul,
  neg,
  qty,
  qtyFromApi,
  round,
  sub,
  toApi,
} from './index';

describe('money() / qty() constructors', () => {
  it('accept strings, numbers and Decimals', () => {
    expect(format(money('12.34'))).toBe('12.34');
    expect(format(money(12.34))).toBe('12.34');
    expect(format(qty('3.5'), { places: 3 })).toBe('3.500');
  });

  it('reject NaN, Infinity and blank input loudly', () => {
    expect(() => money(Number.NaN)).toThrow(InvalidDecimalError);
    expect(() => money(Number.POSITIVE_INFINITY)).toThrow(InvalidDecimalError);
    expect(() => money('')).toThrow(InvalidDecimalError);
    expect(() => money('abc')).toThrow(InvalidDecimalError);
    expect(() => qty(Number.NaN)).toThrow(InvalidDecimalError);
  });

  it('exposes ZERO constants that are values, not "absent"', () => {
    expect(isZero(ZERO)).toBe(true);
    expect(isZero(ZERO_QTY)).toBe(true);
    expect(format(ZERO)).toBe('0.00');
  });

  it('guards recognise module-produced values', () => {
    expect(isMoney(money('1'))).toBe(true);
    expect(isMoney('1')).toBe(false);
    expect(isQty(qty('1'))).toBe(true);
    expect(isQty(null)).toBe(false);
  });
});

describe('float regressions - the reason this module exists', () => {
  it('add(money(0.1), money(0.2)) formats as exactly 0.30', () => {
    expect(format(add(money('0.1'), money('0.2')))).toBe('0.30');
  });

  it('does not reproduce the raw IEEE-754 result', () => {
    // The hazard, asserted so the test names it rather than implies it.
    expect(0.1 + 0.2).not.toBe(0.3);
    expect(eq(add(money('0.1'), money('0.2')), money('0.3'))).toBe(true);
    expect(eq(add(money('0.1'), money('0.2')), money(0.1 + 0.2))).toBe(false);
  });

  it('keeps 0.07 times 100 exact', () => {
    expect(format(mul(money('0.07'), 100))).toBe('7.00');
  });
});

describe('the wire boundary', () => {
  it('reads a JSON number and a JSON string alike', () => {
    expect(format(fromApi(1234.5, 'GrandTotal'))).toBe('1,234.50');
    expect(format(fromApi('1234.5', 'GrandTotal'))).toBe('1,234.50');
    expect(format(qtyFromApi(2, 'AddQty'), { places: 3 })).toBe('2.000');
  });

  it.each([
    ['null', null],
    ['undefined', undefined],
    ['NaN', Number.NaN],
    ['empty string', ''],
  ])('throws DecimalBoundaryError naming the field for %s', (_label, input) => {
    expect(() => fromApi(input, 'InvoiceAmount')).toThrow(DecimalBoundaryError);
    expect(() => fromApi(input, 'InvoiceAmount')).toThrow(/InvoiceAmount/);
  });

  it('never returns zero for a missing amount', () => {
    let thrown: unknown = null;
    try {
      fromApi(null, 'NetAmount');
    } catch (error) {
      thrown = error;
    }
    expect(thrown).toBeInstanceOf(DecimalBoundaryError);
    expect((thrown as DecimalBoundaryError).field).toBe('NetAmount');
  });

  it('rejects an unsupported wire type', () => {
    expect(() => fromApi({ amount: 1 }, 'Total')).toThrow(DecimalBoundaryError);
  });

  it('toApi returns a JSON number when it round-trips exactly', () => {
    expect(toApi(money('1234.56'))).toBe(1234.56);
    expect(toApi(ZERO)).toBe(0);
  });

  it('a 20-significant-digit amount survives fromApi then toApi unchanged', () => {
    const wire = '12345678901234567890';
    expect(toApi(fromApi(wire, 'HugeAmount'))).toBe(wire);
  });

  it('a 20-digit value would have been corrupted by a JS number', () => {
    expect(String(Number('12345678901234567890'))).not.toBe('12345678901234567890');
  });
});

describe('arithmetic', () => {
  it('add, sub, mul, div, neg, abs', () => {
    expect(format(add(money('1.05'), money('2.10')))).toBe('3.15');
    expect(format(sub(money('1.05'), money('2.10')))).toBe('-1.05');
    expect(format(mul(money('1.05'), '3'))).toBe('3.15');
    expect(format(div(money('10'), 4))).toBe('2.50');
    expect(format(neg(money('1.05')))).toBe('-1.05');
    expect(format(abs(money('-1.05')))).toBe('1.05');
  });

  it('div throws rather than yielding Infinity', () => {
    expect(() => div(money('1'), 0)).toThrow(InvalidDecimalError);
  });
});

describe('comparison', () => {
  it('compares numerically, not by reference', () => {
    const a = money('1.10');
    const b = money('1.1');
    expect(a === b).toBe(false);
    expect(eq(a, b)).toBe(true);
    expect(cmp(a, b)).toBe(0);
  });

  it('lt, lte, gt, gte', () => {
    expect(lt(money('1'), money('2'))).toBe(true);
    expect(lte(money('2'), money('2'))).toBe(true);
    expect(gt(money('3'), money('2'))).toBe(true);
    expect(gte(money('2'), money('2'))).toBe(true);
    expect(cmp(money('3'), money('2'))).toBe(1);
    expect(cmp(money('1'), money('2'))).toBe(-1);
  });

  it('isZero distinguishes zero from absent', () => {
    expect(isZero(money('0.000'))).toBe(true);
    expect(isZero(money('0.01'))).toBe(false);
  });
});

describe('rounding', () => {
  it('defaults to the server mode - half away from zero (CalculationService.cs:103)', () => {
    expect(format(round(money('2.5'), 0), { places: 0 })).toBe('3');
    expect(format(round(money('-2.5'), 0), { places: 0 })).toBe('-3');
    expect(format(round(money('1.005'), 2))).toBe('1.01');
  });

  it('honours an explicit mode', () => {
    expect(format(round(money('2.5'), 0, 'half-even'), { places: 0 })).toBe('2');
    expect(format(round(money('2.4'), 0, 'ceil'), { places: 0 })).toBe('3');
    expect(format(round(money('2.6'), 0, 'floor'), { places: 0 })).toBe('2');
    expect(format(round(money('2.6'), 0, 'down'), { places: 0 })).toBe('2');
    expect(format(round(money('2.1'), 0, 'up'), { places: 0 })).toBe('3');
  });

  it('rejects nonsense places', () => {
    expect(() => round(money('1'), -1)).toThrow(InvalidDecimalError);
    expect(() => round(money('1'), 1.5)).toThrow(InvalidDecimalError);
  });

  it('defaults the mode to the one INV-032 recorded', () => {
    expect(DEFAULT_ROUNDING_MODE).toBe('half-up');
    expect(format(round(money('2.5'), 0, DEFAULT_ROUNDING_MODE), { places: 0 })).toBe('3');
  });

  it('records the storage scales INV-032 measured, distinct from the display default', () => {
    expect(SERVER_DEFAULT_DECIMAL_PLACES).toBe(2);
    expect(STORAGE_SCALE).toEqual({ amount: 2, quantity: 3, rate: 4 });
  });

  it('configures decimal.js once, with the server rounding mode', () => {
    expect(Decimal.rounding).toBe(Decimal.ROUND_HALF_UP);
    expect(Decimal.precision).toBe(34);
  });
});

describe('branding', () => {
  it('rejects a raw number where Money is expected', () => {
    // @ts-expect-error a number is not Money - this is the whole point of the brand
    add(money('1'), 2);
  });

  it('rejects a bare Decimal where Money is expected', () => {
    // @ts-expect-error a bare Decimal carries no brand
    add(money('1'), new Decimal('2'));
  });

  it('does not let Qty stand in for Money', () => {
    // @ts-expect-error Money and Qty are distinct brands
    add(money('1'), qty('2'));
  });
});
