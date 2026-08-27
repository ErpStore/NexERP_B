import { FormControl, FormGroup } from '@angular/forms';

import { money, qty, type Money, type Qty } from '../../utils/decimal';
import type { SelectOption } from '../form/types';
import type { LineItemRowFactory } from './line-item-form';
import type { LineItemColumn } from './line-item-grid.model';

/**
 * **Test fixtures only.** Nothing here is bundled into the application.
 *
 * A deliberately small line shape - an item picker, a quantity, a rate, a
 * remark, a cancel flag - covering one of each cell editor kind so the same
 * fixture set exercises every spec in this directory.
 */

export interface TestLine {
  /** The row's picker column holds the whole `SelectOption`, matching `ComboboxComponent`'s own CVA contract - not a bare id. */
  item: SelectOption<number> | null;
  itemName: string;
  qty: Qty | null;
  rate: Money | null;
  remarks: string;
  cancelled: boolean;
}

export const createTestRow: LineItemRowFactory<TestLine> = (initial) =>
  new FormGroup({
    item: new FormControl<SelectOption<number> | null>(initial.item ?? null),
    itemName: new FormControl<string>(initial.itemName ?? '', { nonNullable: true }),
    qty: new FormControl<Qty | null>(initial.qty ?? null),
    rate: new FormControl<Money | null>(initial.rate ?? null),
    remarks: new FormControl<string>(initial.remarks ?? '', { nonNullable: true }),
    cancelled: new FormControl<boolean>(initial.cancelled ?? false, { nonNullable: true }),
  });

export const TEST_COLUMNS: readonly LineItemColumn<TestLine>[] = [
  {
    field: 'item',
    title: 'Item',
    editor: 'typeahead',
    typeaheadLoader: (query) =>
      Promise.resolve(
        TEST_ITEM_OPTIONS.filter((o) => o.label.toLowerCase().includes(query.toLowerCase())),
      ),
    onEditCommitted: () => 'item-selected',
  },
  { field: 'itemName', title: 'Item name', editor: 'readonly' },
  {
    field: 'qty',
    title: 'Qty',
    editor: 'integer',
    numeric: true,
    decimalPlaces: 2,
    onEditCommitted: () => 'quantity-changed',
  },
  {
    field: 'rate',
    title: 'Rate',
    editor: 'decimal',
    numeric: true,
    decimalPlaces: 2,
    onEditCommitted: () => 'rate-changed',
  },
  { field: 'remarks', title: 'Remarks', editor: 'text' },
  {
    field: 'cancelled',
    title: 'Cancelled',
    editor: 'checkbox',
    onEditCommitted: () => 'row-cancel-toggled',
  },
];

export const TEST_ITEM_OPTIONS: readonly SelectOption<number>[] = [
  { value: 1, label: 'Steel Rod 10mm' },
  { value: 2, label: 'Steel Rod 12mm' },
  { value: 3, label: 'Cement Bag 50kg' },
];

export function testMoney(value: string): Money {
  return money(value);
}

export function testQty(value: string): Qty {
  return qty(value);
}
