import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

/**
 * `no-data` - nothing has ever been created here.
 * `filtered` - things exist, these filters exclude them.
 *
 * They are different situations and they need different words and different
 * actions (KB-051 State patterns). Offering "New sales order" to someone whose
 * date filter is simply wrong wastes their time; offering "Clear filters" on a
 * brand-new tenant is nonsense.
 */
export type EmptyStateVariant = 'no-data' | 'filtered';

@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.component.html',
  styleUrl: './state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly variant = input<EmptyStateVariant>('no-data');
  /** "No sales orders yet" - name the thing, not "No data". */
  readonly title = input.required<string>();
  readonly description = input<string | undefined>(undefined);
  /** The create action's label. Only meaningful for `no-data`. */
  readonly actionLabel = input<string | undefined>(undefined);
  readonly action = output<void>();
  readonly clearFilters = output<void>();

  readonly isFiltered = computed(() => this.variant() === 'filtered');
  readonly headingId = computed(() => `app-empty-state-${this.variant()}`);
}
