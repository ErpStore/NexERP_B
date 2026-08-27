import { describe, expect, it } from 'vitest';

import { fuzzyFilter, fuzzyScore } from './fuzzy-match';

describe('fuzzyScore', () => {
  it('matches a subsequence, case-insensitively', () => {
    expect(fuzzyScore('crm', 'Customer Master')).not.toBeNull();
    expect(fuzzyScore('CUR', 'currency master')).not.toBeNull();
  });

  it('returns null when the characters are not all present in order', () => {
    expect(fuzzyScore('xyz', 'Currency Master')).toBeNull();
    expect(fuzzyScore('rc', 'Customer Requisition')).toBeNull(); // 'c' after 'r' — wrong order
  });

  it('scores a contiguous, earlier match lower (better) than a scattered one', () => {
    const contiguous = fuzzyScore('cur', 'Currency Master');
    const scattered = fuzzyScore('cur', 'Customer Requisition Report');
    expect(contiguous).not.toBeNull();
    expect(scattered).not.toBeNull();
    expect(contiguous as number).toBeLessThan(scattered as number);
  });

  it('an empty query matches everything with score 0', () => {
    expect(fuzzyScore('', 'anything')).toBe(0);
  });
});

describe('fuzzyFilter', () => {
  it('filters out non-matches and sorts the rest best-first', () => {
    const items = ['Currency Master', 'Customer Requisition Report', 'Vendor Master'];
    const result = fuzzyFilter('cur', items, (s) => s);

    expect(result).toEqual(['Currency Master', 'Customer Requisition Report']);
  });
});
