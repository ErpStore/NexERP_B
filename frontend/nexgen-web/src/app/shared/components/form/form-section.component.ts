import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

let nextSectionId = 0;

/**
 * A titled group of fields, optionally collapsible.
 *
 * The heading is a real heading element, so it stays in the accessibility
 * tree and a screen-reader user can jump between the sections of a long
 * document header rather than arrowing through every field.
 */
@Component({
  selector: 'app-form-section',
  templateUrl: './form-section.component.html',
  styleUrl: './form-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormSectionComponent {
  private readonly uid = `app-form-section-${++nextSectionId}`;

  readonly title = input.required<string>();
  readonly description = input<string | undefined>(undefined);
  readonly collapsible = input(false);
  readonly collapsed = input(false);
  /** Heading level, so a section nests correctly under the page's own heading. */
  readonly headingLevel = input<2 | 3 | 4>(3);

  private readonly userCollapsed = signal<boolean | null>(null);

  readonly bodyId = `${this.uid}-body`;
  readonly headingId = `${this.uid}-heading`;
  readonly descriptionId = `${this.uid}-description`;

  readonly isCollapsed = computed(() => this.userCollapsed() ?? this.collapsed());

  toggle(): void {
    this.userCollapsed.set(!this.isCollapsed());
  }
}
