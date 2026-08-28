import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, TemplateRef, input } from '@angular/core';

import type { DocumentTotals, DocumentTotalsDescriptor } from './document-editor.model';

/**
 * The totals region - a **slot and nothing else** (M2-C08-01). M2-C08-02 fills
 * it, together with the `calculate` call and the totals semantics behind it.
 *
 * The slot renders exactly one thing: the caller's template, given the
 * server's totals values and the config's descriptor as context. It renders no
 * label, no ladder and no fallback of its own, because every one of those would
 * be a totals decision, and totals are the server's (BR-CALC-001). A document
 * with no totals - `LabourDcOutgoingUpsert.razor` and `ItemUpsert.razor`, per
 * the survey - simply supplies no template, and nothing is rendered.
 */
@Component({
  selector: 'app-totals-panel-slot',
  templateUrl: './totals-panel-slot.component.html',
  styleUrl: './totals-panel-slot.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet],
})
export class TotalsPanelSlotComponent {
  /** The caller's template. Absent means the region does not exist at all. */
  readonly template = input<TemplateRef<unknown> | undefined>(undefined);
  readonly descriptor = input<DocumentTotalsDescriptor | undefined>(undefined);
  /** Values as the server returned them - already formatted, never recomputed here. */
  readonly totals = input<DocumentTotals | undefined>(undefined);

  readonly context = (): Record<string, unknown> => ({
    $implicit: this.totals() ?? {},
    totals: this.totals() ?? {},
    descriptor: this.descriptor(),
  });
}
