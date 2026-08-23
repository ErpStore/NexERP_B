import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { SkeletonComponent } from './skeleton.component';

/**
 * The first-load placeholder for a grid: a header strip and `rows` x `columns`
 * cells, sized like the real thing.
 *
 * A preset exists so that ~140 list screens do not each invent one - and so
 * that none of them falls back to a spinner on a blank page, which is the
 * failure KB-051 State patterns names outright.
 */
@Component({
  selector: 'app-skeleton-table',
  templateUrl: './skeleton-table.component.html',
  styleUrl: './skeleton-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SkeletonComponent],
})
export class SkeletonTableComponent {
  readonly rows = input(8);
  readonly columns = input(5);
  readonly showHeader = input(true);
  /** Announced once, politely. "Loading sales orders" beats "Loading". */
  readonly label = input('Loading rows');

  readonly rowIndexes = computed(() => Array.from({ length: this.rows() }, (_, i) => i));
  readonly columnIndexes = computed(() => Array.from({ length: this.columns() }, (_, i) => i));
}
