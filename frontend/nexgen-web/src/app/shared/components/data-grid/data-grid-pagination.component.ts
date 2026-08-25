import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SelectComponent } from '../form/select.component';
import type { SelectOption } from '../form/types';
import { DATA_GRID_PAGE_SIZE_OPTIONS } from './data-grid.model';

/**
 * The pager beneath `DataGrid` (M2-C05-01).
 *
 * PrimeNG's own `p-paginator` is not used, and the reason is the same one that
 * puts `[lazy]="true"` on the table: the pager must be a **view** of query
 * state that lives elsewhere, not a second owner of the page number. It emits
 * intents; `DataGridQueryState` decides.
 *
 * The page-size control is `app-select` (M2-C04-02), not a second dropdown
 * vocabulary.
 */
@Component({
  selector: 'app-data-grid-pagination',
  templateUrl: './data-grid-pagination.component.html',
  styleUrl: './data-grid-pagination.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, SelectComponent],
})
export class DataGridPaginationComponent {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  /** The server's filtered, unpaged total. */
  readonly totalCount = input.required<number>();
  readonly pageSizeOptions = input<readonly number[]>(DATA_GRID_PAGE_SIZE_OPTIONS);
  readonly disabled = input(false);

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();

  readonly totalPages = computed(() =>
    this.pageSize() > 0 ? Math.max(1, Math.ceil(this.totalCount() / this.pageSize())) : 1,
  );
  readonly firstRow = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1,
  );
  readonly lastRow = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));
  readonly onFirstPage = computed(() => this.page() <= 1);
  readonly onLastPage = computed(() => this.page() >= this.totalPages());

  readonly sizeOptions = computed<readonly SelectOption<number>[]>(() =>
    this.pageSizeOptions().map((size) => ({ value: size, label: String(size) })),
  );

  /** "1-20 of 1,204" - the count is the server's, never the page length. */
  readonly rangeLabel = computed(() => {
    if (this.totalCount() === 0) {
      return 'No rows';
    }
    return `${this.firstRow().toLocaleString()}-${this.lastRow().toLocaleString()} of ${this.totalCount().toLocaleString()}`;
  });

  goTo(page: number): void {
    const target = Math.min(Math.max(1, page), this.totalPages());
    if (target !== this.page()) {
      this.pageChange.emit(target);
    }
  }

  onPageSizeChange(size: number | null): void {
    if (size !== null && size !== this.pageSize()) {
      this.pageSizeChange.emit(size);
    }
  }
}
