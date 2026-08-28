/**
 * The document editor shell (M2-C08-01). Specification:
 * `docs/kb/execution/tasks/M2-C08-01.md`.
 *
 * Three rules govern everything exported here:
 *   1. **One component, configured, never forked.** Every module-specific
 *      name - field, status, endpoint, noun - is data in
 *      `DocumentEditorConfig`. Nothing in this directory names a document type.
 *   2. **No ERP business rule.** No total, tax or round-off is computed here;
 *      no status or transition is decided here; `rowEvent` is forwarded, never
 *      interpreted. The server stays authoritative (ADR-007 `:162-164`).
 *   3. **The siblings extend, they do not reshape.** The totals slot
 *      (M2-C08-02) and the workflow-command slot (M2-C08-03) render only
 *      caller-supplied templates, and their config types are already declared.
 */
export {
  DocumentEditorComponent,
  DOCUMENT_EDITOR_READONLY_BREAKPOINT_PX,
} from './document-editor.component';
export type { DocumentEditorHost, UnsavedChangesChoice } from './document-editor.component';

export { DocumentHeaderFormComponent } from './document-header-form.component';
export { DocumentCommandBarComponent } from './document-command-bar.component';
export { DocumentSideRegionComponent } from './document-side-region.component';
export { TotalsPanelSlotComponent } from './totals-panel-slot.component';
export { DocumentRegionDirective } from './document-region.directive';

export {
  DocumentEditorStore,
  createHttpDocumentOperations,
  injectHttpDocumentOperations,
  toApiProblem,
} from './document-editor.service';

export { unsavedChangesGuard } from './unsaved-changes.guard';

export {
  buildHeaderForm,
  headerFieldsOf,
  headerSectionsOf,
  sectionOfField,
  sectionStateKey,
} from './document-editor.model';
export type {
  DocumentCommand,
  DocumentEditorConfig,
  DocumentEditorMode,
  DocumentEditorOperations,
  DocumentFieldControl,
  DocumentHeaderField,
  DocumentHeaderLayout,
  DocumentHeaderSection,
  DocumentHeaderTab,
  DocumentId,
  DocumentLineGrid,
  DocumentPayload,
  DocumentRowEventContext,
  DocumentSideRegion,
  DocumentSnapshot,
  DocumentSubLineGrid,
  DocumentTotals,
  DocumentTotalsDescriptor,
  DocumentTotalsRow,
  DocumentUpstreamPull,
} from './document-editor.model';
