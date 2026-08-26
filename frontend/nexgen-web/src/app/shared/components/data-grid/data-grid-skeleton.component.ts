import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { SkeletonComponent } from '../feedback/skeleton.component';
import type { DataGridColumn } from './data-grid.model';

/** The cap on skeleton rows, whatever the page size (M2-C05-03, requirement 6). */
export const DATA_GRID_MAX_SKELETON_ROWS = 12;

/**
 * First load: `min(pageSize, 12)` placeholder rows whose cells carry the
 * **resolved** column widths, so nothing moves when the real rows arrive.
 *
 * Never a spinner on a blank page - KB-051 State patterns names that failure
 * outright, and it is what the Blazor list does today
 * (`ProcessingOverlay.razor`, a full blocking overlay for a list refresh).
 *
 * Accessibility: the bars are `aria-hidden` (`app-skeleton` does that itself),
 * and the region announces **once**. Thirty announced placeholders is noise, not
 * information.
 */
@Component({
  selector: 'app-data-grid-skeleton',
  templateUrl: './data-grid-skeleton.component.html',
  // Layout shared with `app-data-grid-states`; the visuals belong to the primitives.
  styleUrl: './data-grid-states.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SkeletonComponent],
})
export class DataGridSkeletonComponent<TRow> {
  readonly columns = input.required<readonly DataGridColumn<TRow>[]>();
  /** Resolved widths, keyed by `field` - the grid's `columnWidths()` after any user resize. */
  readonly columnWidths = input<Readonly<Record<string, string>>>({});
  readonly pageSize = input(20);
  readonly rowHeightPx = input(36);
  /** A leading select-all cell shifts every data column one to the right. */
  readonly leadingCells = input(0);
  readonly label = input('Loading results');

  readonly rowCount = computed(() =>
    Math.max(1, Math.min(this.pageSize(), DATA_GRID_MAX_SKELETON_ROWS)),
  );
  readonly rowIndexes = computed(() => Array.from({ length: this.rowCount() }, (_, i) => i));
  readonly leadingIndexes = computed(() =>
    Array.from({ length: this.leadingCells() }, (_, i) => i),
  );

  /** The width the real cell will have: a user resize first, then the column's own. */
  width(column: DataGridColumn<TRow>): string {
    return this.columnWidths()[column.field] ?? column.width ?? '100%';
  }
}
