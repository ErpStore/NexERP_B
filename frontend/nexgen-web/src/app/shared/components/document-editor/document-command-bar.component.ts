import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, TemplateRef, input, output } from '@angular/core';

import type { DocumentEditorMode } from './document-editor.model';

/**
 * The sticky footer toolbar (M2-C08-01).
 *
 * Three fixed affordances - Cancel, Save, Save + New - and one **slot** for the
 * workflow commands, which M2-C08-03 fills. The bar renders nothing of its own
 * into that slot: what commands a document offers, and whether the server
 * permits them, is not this component's decision.
 *
 * Two of the three fixed affordances are **new**, and that is recorded rather
 * than implied: the survey found no Cancel and no Save + New button on any of
 * the five sampled Upsert screens - they carry a single Create/Update submit
 * whose label switches on `id > 0` (`MfgPOUpsert.razor:1882-1885`), plus
 * `SmartBackButton`. Cancel absorbs `SmartBackButton`; Save + New is a new
 * affordance for repeated data entry.
 *
 * **Saving never blocks the page** (KB-051 §State patterns): the actions
 * disable and the Save button shows an inline spinner. That is a deliberate
 * divergence from `ProcessingOverlay.razor`, which covered the whole screen.
 */
@Component({
  selector: 'app-document-command-bar',
  templateUrl: './document-command-bar.component.html',
  styleUrl: './document-command-bar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet],
})
export class DocumentCommandBarComponent {
  readonly mode = input.required<DocumentEditorMode>();
  readonly dirty = input(false);
  readonly saving = input(false);
  /** From the permission service - a UX affordance only; the server enforces independently. */
  readonly canSave = input(true);
  readonly noun = input('document');
  /** M2-C08-03's template. Nothing else is ever rendered between Cancel and Save. */
  readonly commandsTemplate = input<TemplateRef<unknown> | undefined>(undefined);

  readonly save = output<void>();
  readonly saveAndNew = output<void>();
  /** Named `cancelled`, not `cancel`: `cancel` is a native DOM event name. */
  readonly cancelled = output<void>();
}
