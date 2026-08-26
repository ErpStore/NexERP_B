import { describe, expect, it } from 'vitest';

import { RecordSelection } from './record-selection';
import { makePickerRows, pickerRowId, type PickerRow } from './test-fixtures';

/**
 * The selection holder on its own - no TestBed, no DOM. Everything the dialog
 * promises about ordering, page scope and pre-selection is a property of this
 * class, and proving it here is cheaper and sharper than proving it through a
 * rendered grid.
 */
function make(): { selection: RecordSelection<PickerRow>; rows: PickerRow[] } {
  return { selection: new RecordSelection<PickerRow>(() => pickerRowId), rows: makePickerRows(5) };
}

describe('RecordSelection', () => {
  it('returns rows in the order they were added, not in row order', () => {
    const { selection, rows } = make();

    selection.add(rows[4]!);
    selection.add(rows[1]!);
    selection.add(rows[3]!);

    expect(selection.ids()).toEqual([5, 2, 4]);
  });

  it('leaves an already-selected row where the user first put it', () => {
    const { selection, rows } = make();

    selection.add(rows[4]!);
    selection.add(rows[1]!);
    selection.selectPage(rows);

    // 5 and 2 keep their positions; the rest follow in page order.
    expect(selection.ids()).toEqual([5, 2, 1, 3, 4]);
  });

  it('toggles a row off and back on to the end of the order', () => {
    const { selection, rows } = make();

    selection.add(rows[0]!);
    selection.add(rows[1]!);
    selection.toggle(rows[0]!);
    selection.toggle(rows[0]!);

    expect(selection.ids()).toEqual([2, 1]);
  });

  it('skips excluded ids in selectPage - a disabled row is never selected', () => {
    const { selection, rows } = make();

    selection.selectPage(rows, new Set([2, 4]));

    expect(selection.ids()).toEqual([1, 3, 5]);
  });

  it('clearPage removes only the rows on that page', () => {
    const { selection } = make();
    const pageOne = makePickerRows(3);
    const pageTwo = makePickerRows(3, 10);

    selection.selectPage(pageOne);
    selection.selectPage(pageTwo);
    selection.clearPage(pageTwo);

    expect(selection.ids()).toEqual([1, 2, 3]);
  });

  it('counts and reports emptiness', () => {
    const { selection, rows } = make();

    expect(selection.isEmpty()).toBe(true);
    selection.add(rows[0]!);
    expect(selection.count()).toBe(1);
    expect(selection.isEmpty()).toBe(false);
    selection.clear();
    expect(selection.isEmpty()).toBe(true);
  });

  it('replaceWith keeps exactly one row - single-select mode', () => {
    const { selection, rows } = make();

    selection.add(rows[0]!);
    selection.add(rows[1]!);
    selection.replaceWith(rows[3]!);

    expect(selection.ids()).toEqual([4]);
  });

  it('adopts a pre-selected id once, and does not re-adopt it after the user removes it', () => {
    const { selection, rows } = make();
    const initial = new Set([2]);

    selection.adopt(rows, initial);
    expect(selection.ids()).toEqual([2]);

    selection.remove(2);
    selection.adopt(rows, initial);

    expect(selection.ids()).toEqual([]);
  });

  it('reset forgets both the selection and which pre-selections were honoured', () => {
    const { selection, rows } = make();
    const initial = new Set([3]);

    selection.adopt(rows, initial);
    selection.reset();
    selection.adopt(rows, initial);

    expect(selection.ids()).toEqual([3]);
  });
});
