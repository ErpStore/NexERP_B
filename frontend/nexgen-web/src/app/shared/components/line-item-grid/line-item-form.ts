import type { AbstractControl, FormArray, FormControl, FormGroup } from '@angular/forms';

import { makeRowId, type LineItemRowId } from './line-item-grid.model';

/**
 * `FormArray` operations for `LineItemGrid` (M2-C07).
 *
 * **This module never clones the caller's `FormArray`.** It is handed the
 * caller's own array and manipulates it in place - append, duplicate,
 * remove, move, revert - so the caller's `[lines]` binding and this module's
 * view of the data are always the same object, never two copies drifting
 * apart. That is what the task's *Target Result* means by "the grid edits
 * the caller's form; it does not own a second copy of the data."
 */

/**
 * Every field is a plain `FormControl`, never a nested `FormGroup`/`FormArray` -
 * a document line is a flat record of scalar values (item, quantity, rate, a
 * handful of flags), and `[formControl]` in `line-item-row.component.html`
 * needs exactly `FormControl`, not the wider `AbstractControl` a mapped type
 * would otherwise infer.
 */
export type LineItemFormGroup<TLine> = FormGroup<{
  [K in keyof TLine]: FormControl<TLine[K]>;
}>;

export type LineItemFormArrayModel<TLine> = FormArray<LineItemFormGroup<TLine>>;

/**
 * Builds one row's `FormGroup` from an initial (possibly partial) value.
 * Supplied once per document type by the caller - this module has no
 * opinion about what fields a line carries, only about the array they sit
 * in.
 */
export type LineItemRowFactory<TLine> = (initial: Partial<TLine>) => LineItemFormGroup<TLine>;

/**
 * Row identity, keyed by `FormGroup` object identity rather than array
 * index. A `WeakMap` is what lets an id "survive reordering" without adding
 * a field to the caller's row type: moving a group in the array never
 * creates a new object, so the id travels with it for free, and an
 * unreferenced row's id is reclaimed with the group itself.
 */
const rowIds = new WeakMap<AbstractControl, LineItemRowId>();

/**
 * A row's last known-good values - set once when the row is created, and
 * again every time a `respond(patch)` successfully applies. `Esc` and a
 * `respond(null)` refusal both roll back to this, **never to a guess**: if
 * nothing has ever been committed (a still-blank new row), reverting is a
 * no-op rather than inventing a prior state.
 */
const lastCommitted = new WeakMap<AbstractControl, unknown>();

/**
 * Wraps one caller-owned `FormArray` with the row-lifecycle operations
 * `LineItemGrid` needs. One instance per grid; its internal sequence counter
 * is per-instance and starts at zero, so ids are deterministic under test
 * rather than derived from a clock (`row-1`, `row-2`, ... never a UUID or a
 * timestamp - readable in a failing assertion, and stable across runs).
 */
export class LineItemForm<TLine> {
  #sequence = 0;

  constructor(
    private readonly array: LineItemFormArrayModel<TLine>,
    private readonly createRow: LineItemRowFactory<TLine>,
  ) {
    // Rows the caller seeded directly into the `FormArray` (an existing
    // document being edited, not one added through this class) have never
    // been through `append()`/`duplicate()`, so without this they would
    // have no revert baseline at all - `Esc` on a freshly-loaded document's
    // first row would silently do nothing until some other commit happened
    // to that row first.
    for (const group of this.array.controls) {
      this.commit(this.idOf(group));
    }
  }

  /** Every row's `FormGroup`, in array order. */
  rows(): readonly LineItemFormGroup<TLine>[] {
    return this.array.controls;
  }

  /** Every row's id, in the same order as {@link rows}. */
  rowIds(): readonly LineItemRowId[] {
    return this.rows().map((group) => this.idOf(group));
  }

  /** The stable id for a row's `FormGroup`, assigning one on first use. */
  idOf(group: LineItemFormGroup<TLine>): LineItemRowId {
    let id = rowIds.get(group);
    if (id === undefined) {
      id = makeRowId(`row-${++this.#sequence}`);
      rowIds.set(group, id);
    }
    return id;
  }

  /** The row's current array position, or `-1` if the id no longer exists. */
  indexOf(id: LineItemRowId): number {
    return this.rows().findIndex((group) => this.idOf(group) === id);
  }

  /** The row's `FormGroup`, or `null` if the id no longer exists. */
  groupOf(id: LineItemRowId): LineItemFormGroup<TLine> | null {
    return this.rows().find((group) => this.idOf(group) === id) ?? null;
  }

  /** Appends a new row at the end, commits its initial values as its baseline, and returns its id. */
  append(initial: Partial<TLine> = {}): LineItemRowId {
    const group = this.createRow(initial);
    this.array.push(group);
    const id = this.idOf(group);
    this.commit(id);
    return id;
  }

  /**
   * Copies a row's **current, possibly-uncommitted** values into a new row
   * directly beneath it. The source row is read, never mutated - `Ctrl+D`
   * must not disturb the row the operator is looking at.
   */
  duplicate(id: LineItemRowId): LineItemRowId | null {
    const index = this.indexOf(id);
    if (index === -1) {
      return null;
    }
    const source = this.rows()[index]!;
    const group = this.createRow(source.getRawValue() as Partial<TLine>);
    this.array.insert(index + 1, group);
    const newId = this.idOf(group);
    this.commit(newId);
    return newId;
  }

  /** Removes a row. Returns `false` if the id no longer exists - already removed by a concurrent edit. */
  remove(id: LineItemRowId): boolean {
    const index = this.indexOf(id);
    if (index === -1) {
      return false;
    }
    this.array.removeAt(index);
    return true;
  }

  /**
   * Moves a row by `delta` positions (`-1`/`+1` for `Alt+ArrowUp`/`Down`),
   * clamped to the array's bounds. Returns the row's resulting index - equal
   * to its starting index when the move would leave the array, so the
   * caller can tell "moved" from "already at the edge" without a second
   * lookup.
   */
  move(id: LineItemRowId, delta: number): number {
    const from = this.indexOf(id);
    if (from === -1) {
      return -1;
    }
    const to = Math.min(Math.max(from + delta, 0), this.rows().length - 1);
    if (to === from) {
      return from;
    }
    const controls = this.array.controls;
    const [group] = controls.splice(from, 1);
    controls.splice(to, 0, group!);
    // A direct splice on `controls` bypasses FormArray's own bookkeeping;
    // this is what brings validity/value state back in sync with it,
    // without emitting a value the caller did not ask for.
    this.array.updateValueAndValidity({ emitEvent: false });
    return to;
  }

  /**
   * Marks a row's **current** values as its new last-known-good state.
   * Called by the grid after a `respond(patch)` successfully applies - never
   * called for a row still awaiting one, so a mid-flight edit cannot become
   * the revert target for itself.
   */
  commit(id: LineItemRowId): void {
    const group = this.groupOf(id);
    if (!group) {
      return;
    }
    lastCommitted.set(group, group.getRawValue());
  }

  /**
   * Rolls a row back to its last {@link commit} - `Esc`'s entire
   * implementation. A no-op when nothing has been committed yet, rather than
   * inventing an empty state to revert to.
   */
  revert(id: LineItemRowId): void {
    const group = this.groupOf(id);
    if (!group) {
      return;
    }
    const snapshot = lastCommitted.get(group);
    if (snapshot !== undefined) {
      group.reset(snapshot as never, { emitEvent: true });
    }
  }

  /**
   * Applies a caller's `respond(patch)` to one row: a patch merges and
   * becomes the new baseline; `null` reverts to the last baseline instead of
   * being merged as if it were `{}`. This is the only path a domain value
   * reaches a row (see `LineItemDomainEvent` in `line-item-grid.model.ts`) -
   * the grid never computes a replacement itself.
   */
  applyPatch(id: LineItemRowId, patch: Partial<TLine> | null): void {
    if (patch === null) {
      this.revert(id);
      return;
    }
    const group = this.groupOf(id);
    if (!group) {
      return;
    }
    group.patchValue(patch as never, { emitEvent: true });
    this.commit(id);
  }
}
