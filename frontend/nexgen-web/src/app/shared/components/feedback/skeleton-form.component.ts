import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { SkeletonComponent } from './skeleton.component';

/**
 * The first-load placeholder for a document header or an editor: `fields`
 * label-and-control pairs laid out in `columns`, matching the shape
 * `app-form-layout` will render once the data arrives.
 */
@Component({
  selector: 'app-skeleton-form',
  templateUrl: './skeleton-form.component.html',
  styleUrl: './skeleton-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SkeletonComponent],
})
export class SkeletonFormComponent {
  readonly fields = input(6);
  readonly columns = input(2);
  readonly label = input('Loading form');

  readonly fieldIndexes = computed(() => Array.from({ length: this.fields() }, (_, i) => i));
}
