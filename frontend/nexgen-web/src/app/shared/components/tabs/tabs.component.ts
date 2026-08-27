import { ChangeDetectionStrategy, Component, ElementRef, inject, input, model } from '@angular/core';

export interface TabItem {
  readonly id: string;
  readonly label: string;
}

/**
 * M2-C03 — underline-style tabs, scrollable overflow, arrow-key navigable
 * (KB-051 §Component inventory (Navigation)).
 *
 * **Headless on purpose.** This renders only the `role="tablist"` header row. No feature
 * consumes tabs yet (the document editor, `M2-C08`, is the first), so this does not invent
 * a `tabpanel` content-projection shape ahead of a real consumer — the caller pairs
 * `activeId()` with its own `role="tabpanel"` elements, `id`-matched and
 * `aria-labelledby`-linked to the corresponding tab's generated id.
 */
@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.component.html',
  styleUrl: './tabs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TabsComponent {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly tabs = input.required<readonly TabItem[]>();
  readonly activeId = model.required<string>();
  /** Prefixes each tab's generated DOM id, so two `app-tabs` instances on one page never
   * collide. Callers building `aria-labelledby` on their panels read this same prefix. */
  readonly idPrefix = input('app-tabs');

  protected tabDomId(tab: TabItem): string {
    return `${this.idPrefix()}-tab-${tab.id}`;
  }

  protected panelDomId(tab: TabItem): string {
    return `${this.idPrefix()}-panel-${tab.id}`;
  }

  protected select(tab: TabItem): void {
    this.activeId.set(tab.id);
  }

  /** Roving tabindex: arrow keys move focus *and* selection together (the common tabs
   * pattern — unlike a listbox, a tab that has focus but is not selected is unusual). */
  protected onKeydown(event: KeyboardEvent, index: number): void {
    const items = this.tabs();
    if (items.length === 0) {
      return;
    }
    let nextIndex: number;
    switch (event.key) {
      case 'ArrowRight':
        nextIndex = (index + 1) % items.length;
        break;
      case 'ArrowLeft':
        nextIndex = (index - 1 + items.length) % items.length;
        break;
      case 'Home':
        nextIndex = 0;
        break;
      case 'End':
        nextIndex = items.length - 1;
        break;
      default:
        return;
    }
    event.preventDefault();
    const nextTab = items[nextIndex];
    if (!nextTab) {
      return;
    }
    this.activeId.set(nextTab.id);
    this.host.nativeElement.querySelector<HTMLElement>(`#${this.tabDomId(nextTab)}`)?.focus();
  }
}
