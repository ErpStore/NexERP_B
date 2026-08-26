import { computed, signal, type Signal } from '@angular/core';

import type { DataGridRowId, DataGridRowIdFn } from '../data-grid';

/**
 * The selection held by `RecordPickerDialog`, and the reason it is a class of
 * its own rather than an array in the component.
 *
 * **Insertion order is the contract.** `DetailsModal.razor` returns the ticked
 * rows ordered by the sequence in which the user ticked them - `SelectionOrder`
 * is assigned on every tick (`:186-195`, `:205-213`) and the confirm path sorts
 * by it (`:150-154`). Operators use that to control the line order of the
 * document they are building, so a naive rewrite that returns rows in grid
 * order produces a delivery challan with its lines in the wrong sequence and
 * nobody notices until a customer does. A `Map` preserves insertion order by
 * specification, which is exactly the guarantee needed, and
 * `selection-order.spec.ts` asserts it rather than trusting it.
 *
 * **It lives above the grid, not in the rows.** Selection is keyed by
 * `getRowId`, so it survives paging and searching: the row objects on page 1
 * are gone by page 3, but their ids are not.
 *
 * No business rule lives here. Whether a record *may* be selected is the
 * caller's decision, taken from server data (`disabledRowIds`).
 */
export class RecordSelection<TRow> {
  readonly #entries = signal<ReadonlyMap<DataGridRowId, TRow>>(new Map<DataGridRowId, TRow>());

  /**
   * Ids whose `initialSelection` has already been honoured once. Not a signal:
   * it is bookkeeping, not state anything renders. Without it, un-ticking a
   * pre-selected row would be undone the next time its page loaded.
   */
  readonly #adopted = new Set<DataGridRowId>();

  /**
   * Reads the current row-id function. A getter rather than a value because the
   * component's `getRowId` is a signal input and may legitimately change with
   * the resource being picked from.
   */
  readonly #idOf: () => DataGridRowIdFn<TRow>;

  constructor(idOf: () => DataGridRowIdFn<TRow>) {
    this.#idOf = idOf;
  }

  /** The selected rows, **in the order they were selected**. */
  readonly selected: Signal<readonly TRow[]> = computed(() => [...this.#entries().values()]);

  /** The selected ids, in the same order. */
  readonly ids: Signal<readonly DataGridRowId[]> = computed(() => [...this.#entries().keys()]);

  readonly count: Signal<number> = computed(() => this.#entries().size);

  readonly isEmpty: Signal<boolean> = computed(() => this.#entries().size === 0);

  isSelected(row: TRow): boolean {
    return this.#entries().has(this.#idOf()(row));
  }

  has(id: DataGridRowId): boolean {
    return this.#entries().has(id);
  }

  /**
   * Add a row at the end of the selection order. A row already selected keeps
   * its original position - re-ticking through a select-all must not reorder
   * what the user chose first.
   */
  add(row: TRow): void {
    const id = this.#idOf()(row);
    const current = this.#entries();
    if (current.has(id)) {
      return;
    }
    const next = new Map(current);
    next.set(id, row);
    this.#entries.set(next);
  }

  remove(id: DataGridRowId): void {
    const current = this.#entries();
    if (!current.has(id)) {
      return;
    }
    const next = new Map(current);
    next.delete(id);
    this.#entries.set(next);
  }

  toggle(row: TRow): void {
    const id = this.#idOf()(row);
    if (this.#entries().has(id)) {
      this.remove(id);
    } else {
      this.add(row);
    }
  }

  /** Replace the whole selection with one row - single-select mode. */
  replaceWith(row: TRow): void {
    const next = new Map<DataGridRowId, TRow>();
    next.set(this.#idOf()(row), row);
    this.#entries.set(next);
  }

  /**
   * Select every row on the current page, skipping `excluded`.
   *
   * **Page-scoped, and the UI says so.** `DetailsModal.ToggleSelectAll` covers
   * the client-side *filtered* set (`:181-198`), which is the whole candidate
   * set it happens to hold. With server paging "all" is ambiguous, and silently
   * selecting 4,000 unseen rows is a data-integrity hazard, so "all" means this
   * page.
   */
  selectPage(rows: readonly TRow[], excluded: ReadonlySet<DataGridRowId> = new Set()): void {
    const idOf = this.#idOf();
    const next = new Map(this.#entries());
    for (const row of rows) {
      const id = idOf(row);
      if (!excluded.has(id) && !next.has(id)) {
        next.set(id, row);
      }
    }
    this.#entries.set(next);
  }

  /** Deselect every row on the current page, leaving selections made elsewhere. */
  clearPage(rows: readonly TRow[]): void {
    const idOf = this.#idOf();
    const next = new Map(this.#entries());
    for (const row of rows) {
      next.delete(idOf(row));
    }
    this.#entries.set(next);
  }

  clear(): void {
    this.#entries.set(new Map<DataGridRowId, TRow>());
  }

  /** Clear the selection *and* forget which pre-selections were honoured. */
  reset(): void {
    this.#adopted.clear();
    this.clear();
  }

  /**
   * Honour `initialSelection` for the rows that have just arrived.
   *
   * Pre-selection cannot be applied at open time, because the picker never
   * holds the candidate set - the rows an id refers to only exist once their
   * page has loaded. Each id is honoured at most once, so a user who un-ticks a
   * pre-selected row stays in control of it.
   *
   * Reproduces `DetailsModal.razor:130-134` in *capability* only: the `"Selected"`
   * key it reads is dead code in the live system - the producing services write
   * `["Selected"] = false` in 75 places and `true` in none (INV-054) - so this
   * input preserves nothing observable and is recorded as new capability aimed
   * at the already-pulled case.
   */
  adopt(rows: readonly TRow[], ids: ReadonlySet<DataGridRowId>): void {
    if (ids.size === 0) {
      return;
    }
    const idOf = this.#idOf();
    let next: Map<DataGridRowId, TRow> | null = null;
    for (const row of rows) {
      const id = idOf(row);
      if (!ids.has(id) || this.#adopted.has(id)) {
        continue;
      }
      this.#adopted.add(id);
      if (!this.#entries().has(id)) {
        next ??= new Map(this.#entries());
        next.set(id, row);
      }
    }
    if (next) {
      this.#entries.set(next);
    }
  }
}
