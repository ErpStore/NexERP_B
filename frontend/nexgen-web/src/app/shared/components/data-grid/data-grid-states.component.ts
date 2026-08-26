import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { EmptyStateComponent } from '../feedback/empty-state.component';
import { DataGridErrorComponent } from './data-grid-error.component';
import { DataGridSkeletonComponent } from './data-grid-skeleton.component';
import type { DataGridColumn } from './data-grid.model';

/** Which of the five states the grid is in. */
export type DataGridStateKind = 'loading' | 'error' | 'empty' | 'filtered-empty';

/**
 * The non-happy-path states of `DataGrid`, in one place (M2-C05-03).
 *
 * It **composes** M2-C04-03's feedback primitives and adds no visuals of its
 * own: `app-empty-state`, `app-error-state`, `app-inline-alert`,
 * `app-permission-denied-state` and `app-skeleton` are that task's deliverables,
 * and a second `EmptyState` living in this directory is the duplication the
 * acceptance criteria forbid.
 *
 * **"No data yet" and "filtered to nothing" are decided, not guessed.** They are
 * different situations - one wants "New currency", the other wants "Clear
 * filters" - and the grid distinguishes them by {@link hasActiveFilters}, which
 * `DataGridQueryState.hasActiveFilters` derives from the **committed** filter
 * set. The Blazor list conflates both into one spanning `"No data found."` row
 * (`V.SMART/V.SMART.Shared/Components/DetailsModal.razor:75-82`); that conflation
 * is replaced deliberately.
 *
 * The refetch state is not here: it keeps the previous rows on screen and is a
 * progress bar above the table, so it never reaches an empty-body slot.
 */
@Component({
  selector: 'app-data-grid-states',
  templateUrl: './data-grid-states.component.html',
  styleUrl: './data-grid-states.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [EmptyStateComponent, DataGridErrorComponent, DataGridSkeletonComponent],
})
export class DataGridStatesComponent<TRow> {
  readonly columns = input.required<readonly DataGridColumn<TRow>[]>();
  readonly columnWidths = input<Readonly<Record<string, string>>>({});
  readonly pageSize = input(20);
  readonly rowHeightPx = input(36);
  readonly leadingCells = input(0);

  /** First load - nothing on screen yet. */
  readonly loading = input(false);
  /** The server's `ProblemDetails`, untouched. */
  readonly error = input<unknown>(null);
  /** Committed filters, not the draft. Chooses between the two empty variants. */
  readonly hasActiveFilters = input(false);

  /** "No currencies yet" - name the thing. A caller that says "No data" wasted the state. */
  readonly emptyTitle = input('Nothing here yet');
  readonly emptyDescription = input<string | undefined>(undefined);
  /** The create action's label. Omit it and no primary action is offered. */
  readonly emptyActionLabel = input<string | undefined>(undefined);
  readonly filteredTitle = input('No results for these filters');
  readonly filteredDescription = input<string | undefined>(
    'Every row is excluded by the filters currently applied.',
  );

  readonly emptyAction = output<void>();
  readonly clearFilters = output<void>();
  readonly retry = output<void>();

  readonly kind = computed<DataGridStateKind>(() => {
    if (this.loading()) {
      return 'loading';
    }
    if (this.error() !== null && this.error() !== undefined) {
      return 'error';
    }
    return this.hasActiveFilters() ? 'filtered-empty' : 'empty';
  });

  readonly title = computed(() =>
    this.kind() === 'filtered-empty' ? this.filteredTitle() : this.emptyTitle(),
  );
  readonly description = computed(() =>
    this.kind() === 'filtered-empty' ? this.filteredDescription() : this.emptyDescription(),
  );
}
