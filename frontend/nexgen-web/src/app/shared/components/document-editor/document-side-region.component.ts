import { NgTemplateOutlet } from '@angular/common';
import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  TemplateRef,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';

import { BREAKPOINTS } from '@/app/core/theme/breakpoints';

import { TabsComponent, type TabItem } from '../tabs/tabs.component';
import type { DocumentSideRegion } from './document-editor.model';

/**
 * Attachments, terms, remarks, audit trail - whatever the config declares.
 *
 * Tabs at 1024 px and above, an accordion below it (KB-051 §Responsive
 * behaviour). **Only the open region is instantiated**, which is what stops a
 * document with four regions paying for four of them on load.
 *
 * The component supplies the container, the accessible names and the keyboard
 * model. It supplies no content: each region's body is a caller template
 * named by `appDocumentRegion`. The survey is the reason - none of the five
 * sampled screens has a side region today (attachments are the inline
 * `CorrespondenceStatus` badge at `MfgPOUpsert.razor:256`, terms a card in the
 * form body at `PurchPOUpsert.razor:1440-1470`, remarks a header textarea at
 * `MfgPOUpsert.razor:620-624`, and no priced document has an audit trail), so
 * inventing region content here would be inventing behaviour.
 */
@Component({
  selector: 'app-document-side-region',
  templateUrl: './document-side-region.component.html',
  styleUrl: './document-side-region.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet, TabsComponent],
})
export class DocumentSideRegionComponent {
  readonly #document = inject(DOCUMENT);

  readonly regions = input.required<readonly DocumentSideRegion[]>();
  readonly templates = input<Readonly<Record<string, TemplateRef<unknown>>>>({});

  readonly #activeId = signal<string | null>(null);
  readonly #openAccordionIds = signal<ReadonlySet<string>>(new Set());
  readonly #viewportWidth = signal(this.#document.defaultView?.innerWidth ?? BREAKPOINTS.lg);

  readonly asAccordion = computed(() => this.#viewportWidth() < BREAKPOINTS.md);

  readonly tabs = computed<readonly TabItem[]>(() =>
    this.regions().map((region) => ({ id: region.id, label: region.label })),
  );

  readonly activeId = computed(() => this.#activeId() ?? this.regions()[0]?.id ?? '');

  constructor() {
    const view = this.#document.defaultView;
    if (view) {
      const onResize = (): void => this.#viewportWidth.set(view.innerWidth);
      view.addEventListener('resize', onResize);
      inject(DestroyRef).onDestroy(() => view.removeEventListener('resize', onResize));
    }
  }

  selectTab(id: string): void {
    this.#activeId.set(id);
  }

  isOpen(id: string): boolean {
    return this.#openAccordionIds().has(id);
  }

  toggle(id: string): void {
    const next = new Set(this.#openAccordionIds());
    if (!next.delete(id)) {
      next.add(id);
    }
    this.#openAccordionIds.set(next);
  }

  templateFor(id: string): TemplateRef<unknown> | undefined {
    return this.templates()[id];
  }
}
