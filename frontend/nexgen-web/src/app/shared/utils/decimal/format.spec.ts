import {
  ABSENT_DISPLAY,
  SERVER_DEFAULT_DECIMAL_PLACES,
  format,
  formatAriaLabel,
  getDefaultDecimalPlaces,
  money,
  qty,
  resetDefaultDecimalPlaces,
  setDefaultDecimalPlaces,
} from './index';

describe('format()', () => {
  afterEach(() => {
    resetDefaultDecimalPlaces();
  });

  it('uses two decimal places by default, from the precision policy', () => {
    expect(SERVER_DEFAULT_DECIMAL_PLACES).toBe(2);
    expect(getDefaultDecimalPlaces()).toBe(2);
    expect(format(money('1'), { locale: 'en-US' })).toBe('1.00');
  });

  it('follows the policy when the tenant setting changes', () => {
    setDefaultDecimalPlaces(3);
    expect(format(money('1'), { locale: 'en-US' })).toBe('1.000');
    resetDefaultDecimalPlaces();
    expect(format(money('1'), { locale: 'en-US' })).toBe('1.00');
  });

  it('rejects a nonsense policy value', () => {
    expect(() => {
      setDefaultDecimalPlaces(-1);
    }).toThrow(RangeError);
  });

  it('lets the places argument override the default', () => {
    expect(format(qty('2.5'), { places: 3, locale: 'en-US' })).toBe('2.500');
    expect(format(money('2.5'), { places: 0, locale: 'en-US' })).toBe('3');
  });

  it('renders zero as 0.00 and an absent value as an em dash', () => {
    expect(format(money('0'), { locale: 'en-US' })).toBe('0.00');
    expect(format(null)).toBe(ABSENT_DISPLAY);
    expect(format(undefined)).toBe(ABSENT_DISPLAY);
    expect(ABSENT_DISPLAY).toBe('—');
    expect(format(undefined, { absent: 'n/a' })).toBe('n/a');
  });

  it('renders a negative value with an explicit minus sign', () => {
    expect(format(money('-1234.5'), { locale: 'en-US' })).toBe('-1,234.50');
    expect(formatAriaLabel(money('-1234.5'), { locale: 'en-US' })).toBe('minus 1,234.50');
    expect(formatAriaLabel(money('1234.5'), { locale: 'en-US' })).toBe('1,234.50');
  });

  it('groups thousands per the locale', () => {
    expect(format(money('1234567.891'), { locale: 'en-US' })).toBe('1,234,567.89');
    expect(format(money('1234567.891'), { locale: 'de-DE' })).toBe('1.234.567,89');
    expect(format(money('1234567.891'), { locale: 'en-IN' })).toBe('12,34,567.89');
  });

  it('formats currency when a code is given', () => {
    expect(format(money('1234.5'), { locale: 'en-US', currency: 'USD' })).toBe('$1,234.50');
  });

  it('rounds for display in the server mode, without mutating the value', () => {
    const value = money('2.345');
    expect(format(value, { places: 2, locale: 'en-US' })).toBe('2.35');
    expect(format(value, { places: 2, locale: 'en-US', mode: 'down' })).toBe('2.34');
    expect(format(value, { places: 3, locale: 'en-US' })).toBe('2.345');
  });

  it('does not lose digits a JS number would have lost', () => {
    expect(format(money('12345678901234567890'), { places: 0, locale: 'en-US' })).toBe(
      '12,345,678,901,234,567,890',
    );
  });
});
