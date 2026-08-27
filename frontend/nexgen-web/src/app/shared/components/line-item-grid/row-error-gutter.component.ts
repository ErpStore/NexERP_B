import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/**
 * One row's validation badge - icon plus text, never colour alone (KB-051
 * principle 3), reachable by keyboard and announced. `messages` comes from
 * the caller's `rowErrors` input, keyed by row id - this component renders
 * what it is given; it never decides whether a row is valid (see
 * `line-item-grid.model.ts`'s `LineItemRowError`).
 */
@Component({
  selector: 'app-row-error-gutter',
  template: `
    @if (messages().length > 0) {
      <span
        class="app-row-error-gutter__badge"
        role="img"
        [id]="describedById()"
        [attr.aria-label]="
          'Row has ' + messages().length + ' error' + (messages().length === 1 ? '' : 's')
        "
      >
        <span class="app-row-error-gutter__icon" aria-hidden="true">⚠</span>
      </span>
      <span class="app-row-error-gutter__text" role="status" aria-live="polite">{{
        summary()
      }}</span>
    } @else {
      <span class="app-row-error-gutter__ok" aria-hidden="true"></span>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RowErrorGutterComponent {
  readonly rowId = input.required<string>();
  readonly messages = input<readonly string[]>([]);

  /** `aria-describedby` target on the row's first editable cell. */
  readonly describedById = computed(() => `row-error-${this.rowId()}`);

  /** The gutter shows the first message inline; the rest are in the tooltip/detail the caller may add via `aria-describedby`. */
  readonly summary = computed(() => this.messages()[0] ?? '');
}
