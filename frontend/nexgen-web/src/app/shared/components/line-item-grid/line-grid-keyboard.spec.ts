import { describe, expect, it } from 'vitest';

import {
  caretExhausted,
  decideLineGridKey,
  firstEditableColumn,
  isLineGridShortcut,
  lastEditableColumn,
  nextEditableCell,
  type LineGridBounds,
} from './line-grid-keyboard';

/** 3 rows x 4 cols, column 1 (`itemName`-equivalent) readonly in every row. */
function bounds(overrides: Partial<LineGridBounds> = {}): LineGridBounds {
  return {
    rowCount: 3,
    colCount: 4,
    viewportRows: 10,
    isEditable: (_row, col) => col !== 1,
    ...overrides,
  };
}

describe('isLineGridShortcut', () => {
  it('claims Enter, Escape, Delete, Ctrl+D and Alt+ArrowUp/Down', () => {
    expect(
      isLineGridShortcut({ key: 'Enter', ctrlKey: false, metaKey: false, altKey: false }),
    ).toBe(true);
    expect(
      isLineGridShortcut({ key: 'Escape', ctrlKey: false, metaKey: false, altKey: false }),
    ).toBe(true);
    expect(
      isLineGridShortcut({ key: 'Delete', ctrlKey: false, metaKey: false, altKey: false }),
    ).toBe(true);
    expect(isLineGridShortcut({ key: 'd', ctrlKey: true, metaKey: false, altKey: false })).toBe(
      true,
    );
    expect(isLineGridShortcut({ key: 'd', ctrlKey: false, metaKey: true, altKey: false })).toBe(
      true,
    );
    expect(
      isLineGridShortcut({ key: 'ArrowUp', ctrlKey: false, metaKey: false, altKey: true }),
    ).toBe(true);
    expect(
      isLineGridShortcut({ key: 'ArrowDown', ctrlKey: false, metaKey: false, altKey: true }),
    ).toBe(true);
  });

  it('does not claim a plain letter, a plain arrow, or Tab', () => {
    expect(isLineGridShortcut({ key: 'd', ctrlKey: false, metaKey: false, altKey: false })).toBe(
      false,
    );
    expect(
      isLineGridShortcut({ key: 'ArrowUp', ctrlKey: false, metaKey: false, altKey: false }),
    ).toBe(false);
    expect(isLineGridShortcut({ key: 'Tab', ctrlKey: false, metaKey: false, altKey: false })).toBe(
      false,
    );
  });
});

describe('decideLineGridKey', () => {
  it('maps every shortcut to its action', () => {
    expect(
      decideLineGridKey({ key: 'Enter', ctrlKey: false, metaKey: false, altKey: false }),
    ).toEqual({ kind: 'commit-and-add-row' });
    expect(
      decideLineGridKey({ key: 'Escape', ctrlKey: false, metaKey: false, altKey: false }),
    ).toEqual({ kind: 'revert-row' });
    expect(
      decideLineGridKey({ key: 'Delete', ctrlKey: false, metaKey: false, altKey: false }),
    ).toEqual({ kind: 'delete-row' });
    expect(decideLineGridKey({ key: 'd', ctrlKey: true, metaKey: false, altKey: false })).toEqual({
      kind: 'duplicate-row',
    });
    expect(
      decideLineGridKey({ key: 'ArrowUp', ctrlKey: false, metaKey: false, altKey: true }),
    ).toEqual({ kind: 'move-row', delta: -1 });
    expect(
      decideLineGridKey({ key: 'ArrowDown', ctrlKey: false, metaKey: false, altKey: true }),
    ).toEqual({ kind: 'move-row', delta: 1 });
  });

  it('is `none` for a key it does not claim', () => {
    expect(decideLineGridKey({ key: 'a', ctrlKey: false, metaKey: false, altKey: false })).toEqual({
      kind: 'none',
    });
  });
});

describe('firstEditableColumn / lastEditableColumn', () => {
  it('skips the readonly column in both directions', () => {
    expect(firstEditableColumn(bounds(), 0)).toBe(0);
    expect(firstEditableColumn(bounds(), 0, 1)).toBe(2);
    expect(lastEditableColumn(bounds(), 0)).toBe(3);
    expect(lastEditableColumn(bounds(), 0, 1)).toBe(0);
  });

  it('returns -1 when the row has no editable column at all', () => {
    const allReadOnly = bounds({ isEditable: () => false });
    expect(firstEditableColumn(allReadOnly, 0)).toBe(-1);
    expect(lastEditableColumn(allReadOnly, 0)).toBe(-1);
  });
});

describe('nextEditableCell', () => {
  it('moves forward across editable cells, skipping the readonly one, wrapping to the next row', () => {
    expect(nextEditableCell({ row: 0, col: 0 }, bounds(), false)).toEqual({ row: 0, col: 2 });
    expect(nextEditableCell({ row: 0, col: 3 }, bounds(), false)).toEqual({ row: 1, col: 0 });
  });

  it('moves backward symmetrically', () => {
    expect(nextEditableCell({ row: 1, col: 0 }, bounds(), true)).toEqual({ row: 0, col: 3 });
    expect(nextEditableCell({ row: 0, col: 3 }, bounds(), true)).toEqual({ row: 0, col: 2 });
  });

  it('returns null past the last (or before the first) editable cell of the whole grid', () => {
    expect(nextEditableCell({ row: 2, col: 3 }, bounds(), false)).toBeNull();
    expect(nextEditableCell({ row: 0, col: 0 }, bounds(), true)).toBeNull();
  });
});

describe('caretExhausted', () => {
  it('is exhausted for a null control (no input focused - nothing to edit)', () => {
    expect(caretExhausted(null, 'left')).toBe(true);
    expect(caretExhausted(null, 'right')).toBe(true);
  });

  it('is exhausted at the start (left) or end (right) with no selection', () => {
    expect(caretExhausted({ value: 'abc', selectionStart: 0, selectionEnd: 0 }, 'left')).toBe(true);
    expect(caretExhausted({ value: 'abc', selectionStart: 3, selectionEnd: 3 }, 'right')).toBe(
      true,
    );
  });

  it('is not exhausted mid-string, or while text is selected', () => {
    expect(caretExhausted({ value: 'abc', selectionStart: 1, selectionEnd: 1 }, 'left')).toBe(
      false,
    );
    expect(caretExhausted({ value: 'abc', selectionStart: 0, selectionEnd: 3 }, 'left')).toBe(
      false,
    );
  });
});
