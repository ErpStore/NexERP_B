import { Directive, TemplateRef, inject, input } from '@angular/core';

/**
 * Names one side-region's content, so the shell can render attachments, terms,
 * remarks or an audit trail without knowing what any of them contain.
 *
 * ```html
 * <app-document-editor [config]="config">
 *   <ng-template appDocumentRegion="attachments"> ... </ng-template>
 * </app-document-editor>
 * ```
 *
 * A `TemplateRef` rather than plain projection because the regions are tabs:
 * only the active one is instantiated, which is what *"each region lazily
 * instantiated"* means. Note the survey finding this reflects - none of the
 * five sampled screens has a side region today (attachments are the inline
 * `CorrespondenceStatus` badge, terms a card in the form body, remarks a plain
 * textarea, and no priced document has an audit trail at all), so the region
 * set is entirely the caller's to declare.
 */
@Directive({
  selector: 'ng-template[appDocumentRegion]',
})
export class DocumentRegionDirective {
  /** Must match a {@link DocumentSideRegion} id declared in the config. */
  readonly appDocumentRegion = input.required<string>();

  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}
