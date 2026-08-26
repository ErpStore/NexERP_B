import { format, parseUserInput, parseUserInputAsQty, type ParseResult } from './index';
import type { Money } from './decimal';

function valueOf(result: ParseResult<Money>): string {
  expect(result.kind).toBe('value');
  return result.kind === 'value' ? format(result.value, { places: 4, locale: 'en-US' }) : '';
}

describe('parseUserInput()', () => {
  it('returns "empty" for blank input - never a silent zero', () => {
    expect(parseUserInput('').kind).toBe('empty');
    expect(parseUserInput('   ').kind).toBe('empty');
  });

  it('returns "incomplete" for a legal prefix mid-typing', () => {
    expect(parseUserInput('-').kind).toBe('incomplete');
    expect(parseUserInput('.').kind).toBe('incomplete');
    expect(parseUserInput('1.').kind).toBe('incomplete');
    expect(parseUserInput('-.').kind).toBe('incomplete');
  });

  it('parses a plain value', () => {
    expect(valueOf(parseUserInput('12.34'))).toBe('12.3400');
    expect(valueOf(parseUserInput('-12.34'))).toBe('-12.3400');
    expect(valueOf(parseUserInput('0'))).toBe('0.0000');
  });

  it('honours locale group and decimal separators', () => {
    expect(valueOf(parseUserInput('1,234.56', { locale: 'en-US' }))).toBe('1,234.5600');
    expect(valueOf(parseUserInput('1.234,56', { locale: 'de-DE' }))).toBe('1,234.5600');
    expect(valueOf(parseUserInput('12,34,567.89', { locale: 'en-IN' }))).toBe('1,234,567.8900');
  });

  it('rejects more decimals than the field allows rather than rounding silently', () => {
    const result = parseUserInput('1.999', { places: 2 });
    expect(result).toEqual({ kind: 'error', reason: 'too-many-decimals', raw: '1.999' });
    expect(valueOf(parseUserInput('1.999', { places: 3 }))).toBe('1.9990');
  });

  it('rejects text and duplicated signs', () => {
    expect(parseUserInput('abc')).toEqual({ kind: 'error', reason: 'not-a-number', raw: 'abc' });
    expect(parseUserInput('1.2.3').kind).toBe('error');
    expect(parseUserInput('--5')).toEqual({
      kind: 'error',
      reason: 'too-many-signs',
      raw: '--5',
    });
  });

  it('keeps a 40-digit input exact', () => {
    const digits = '1234567890123456789012345678901234567890';
    const result = parseUserInput(digits, { places: 2 });
    expect(result.kind).toBe('value');
    if (result.kind === 'value') {
      expect(format(result.value, { places: 0, locale: 'en-US' }).replaceAll(',', '')).toBe(digits);
    }
  });

  it('never throws, whatever it is given', () => {
    const inputs = ['', ' ', '-', '.', '1.', 'abc', '1e5', 'NaN', 'Infinity', '1/2', '£4'];
    for (const input of inputs) {
      expect(() => parseUserInput(input)).not.toThrow();
    }
  });

  it('parses quantities into the Qty brand', () => {
    const result = parseUserInputAsQty('2.500', { places: 3 });
    expect(result.kind).toBe('value');
    if (result.kind === 'value') {
      expect(format(result.value, { places: 3, locale: 'en-US' })).toBe('2.500');
    }
  });
});
