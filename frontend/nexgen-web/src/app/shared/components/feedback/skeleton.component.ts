import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Skeleton } from 'primeng/skeleton';

/**
 * One shape-matched placeholder, over `p-skeleton`.
 *
 * KB-051 State patterns: first load is a skeleton **matching the final
 * layout** - never a spinner on a blank page. A spinner tells the operator
 * only that something is happening; a skeleton tells them what is coming and
 * stops the page jumping when it arrives.
 *
 * Individual bars are `aria-hidden`: the announcement belongs once to the
 * region (`app-skeleton-table`, `app-skeleton-form`, or the caller's own
 * `aria-busy`), not thirty times to the bars.
 */
@Component({
  selector: 'app-skeleton',
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Skeleton],
})
export class SkeletonComponent {
  readonly width = input('100%');
  readonly height = input('1rem');
  readonly shape = input<'rectangle' | 'circle'>('rectangle');
  readonly radius = input<string | undefined>(undefined);
}
