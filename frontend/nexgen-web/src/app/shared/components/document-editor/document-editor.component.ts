import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  TemplateRef,
  computed,
  contentChild,
  contentChildren,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
  viewChild,
  viewChildren,
} from '@angular/core';
import { FormArray, FormGroup, ReactiveFormsModule, type FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import type { ApiProblem } from '@/app/core/http/api-problem';
import { BREAKPOINTS } from '@/app/core/theme/breakpoints';

import {
  ErrorStateComponent,
  InlineAlertComponent,
  PermissionDeniedStateComponent,
  SkeletonFormComponent,
  SkeletonTableComponent,
  ToastService,
} from '../feedback';
import { applyServerErrors } from '../form';
import {
  LineItemGridComponent,
  type LineItemFormArrayModel,
  type LineItemFormGroup,
  type LineItemRowError,
  type LineItemRowEvent,
} from '../line-item-grid';
import { ModalComponent } from '../overlay';
import { PageHeaderComponent } from '../page-header/page-header.component';
import { RecordPickerDialogComponent } from '../record-picker-dialog';
import { DocumentCommandBarComponent } from './document-command-bar.component';
import { DocumentHeaderFormComponent } from './document-header-form.component';
import {
  buildHeaderForm,
  type DocumentEditorConfig,
  type DocumentEditorMode,
  type DocumentId,
  type DocumentLineGrid,
  type DocumentPayload,
  type DocumentSnapshot,
} from './document-editor.model';
import { DocumentEditorStore } from './document-editor.service';
import { DocumentRegionDirective } from './document-region.directive';
import { DocumentSideRegionComponent } from './document-side-region.component';
import { TotalsPanelSlotComponent } from './totals-panel-slot.component';

/** Below this width the editor renders read-only (KB-051 §Responsive behaviour). */
export const DOCUMENT_EDITOR_READONLY_BREAKPOINT_PX = BREAKPOINTS.sm;

/**
 * What `unsavedChangesGuard` needs of a component. Kept structural so the guard
 * never imports the editor and any future screen can satisfy it.
 */
export interface DocumentEditorHost {
  canDeactivateDocument: () => boolean | Promise<boolean>;
}

/** The three outcomes the unsaved-changes prompt offers. */
export type UnsavedChangesChoice = 'save' | 'discard' | 'stay';

/**
 * `DocumentEditorComponent<THeader, TLine>` (M2-C08-01) - the one editor all
 * ~65 Upsert screens are configurations of.
 *
 * ```
 * PageHeader   title . breadcrumbs . status badge . document number
 * HeaderForm   typed Reactive Form built from the config's field descriptors
 * LineItemGrid M2-C07, composed whole
 * TotalsPanel  SLOT - M2-C08-02
 * SideRegion   attachments . terms . remarks . audit trail (tabs, accordion < 1024)
 * CommandBar   sticky: Cancel . workflow-command SLOT (M2-C08-03) . Save . Save + New
 * ```
 *
 * **It implements no ERP business rule.** It computes no total, tax or round
 * off; it decides no status or transition; it derives no document number or
 * default; it does not interpret `rowEvent` - it forwards the event, `respond`
 * callback included, to the config's operation. Every module-specific name is
 * config data, which is why nothing in this directory mentions a document type.
 *
 * **Saving does not block the page.** The footer actions disable and Save shows
 * an inline spinner; the rest stays readable. A deliberate divergence from
 * `ProcessingOverlay.razor` (KB-051 §State patterns), noted in KB-051.
 */
@Component({
  selector: 'app-document-editor',
  templateUrl: './document-editor.component.html',
  styleUrl: './document-editor.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DocumentEditorStore],
  imports: [
    ReactiveFormsModule,
    PageHeaderComponent,
    DocumentHeaderFormComponent,
    DocumentCommandBarComponent,
    DocumentSideRegionComponent,
    TotalsPanelSlotComponent,
    LineItemGridComponent,
    RecordPickerDialogComponent,
    ModalComponent,
    ErrorStateComponent,
    InlineAlertComponent,
    PermissionDeniedStateComponent,
    SkeletonFormComponent,
    SkeletonTableComponent,
  ],
})
export class DocumentEditorComponent<THeader, TLine> implements DocumentEditorHost {
  readonly #store = inject<DocumentEditorStore<THeader, TLine>>(DocumentEditorStore);
  readonly #route = inject(ActivatedRoute, { optional: true });
  readonly #router = inject(Router, { optional: true });
  readonly #toast = inject(ToastService, { optional: true });
  readonly #destroyRef = inject(DestroyRef);

  // --- Inputs ---------------------------------------------------------------

  readonly config = input.required<DocumentEditorConfig<THeader, TLine>>();
  /** Overrides the route. `null` means "resolve from the route", which is the normal case. */
  readonly mode = input<DocumentEditorMode | null>(null);
  readonly documentId = input<DocumentId | null>(null);

  readonly saved = output<DocumentSnapshot<THeader, TLine>>();
  readonly cancelled = output<void>();

  // --- Slots ----------------------------------------------------------------

  /** M2-C08-02's template. The shell renders nothing of its own into this region. */
  readonly totalsTemplate = contentChild<TemplateRef<unknown>>('totals');
  /** M2-C08-03's template, between Cancel and Save. Same rule. */
  readonly commandsTemplate = contentChild<TemplateRef<unknown>>('commands');
  readonly regionTemplates = contentChildren(DocumentRegionDirective);

  private readonly anchorRef = viewChild<ElementRef<HTMLElement>>('anchor');
  private readonly headerFormRef = viewChild(DocumentHeaderFormComponent);
  private readonly gridRefs = viewChildren(LineItemGridComponent);

  // --- State ----------------------------------------------------------------

  readonly #routeId = signal<DocumentId | null>(null);
  readonly #routeMode = signal<DocumentEditorMode | null>(null);
  readonly #revision = signal(0);
  readonly #formErrors = signal<readonly string[]>([]);
  readonly #announcement = signal('');
  readonly #promptOpen = signal(false);
  readonly #pickerGridId = signal<string | null>(null);
  readonly #viewportWidth = signal(
    typeof window === 'undefined' ? Number.POSITIVE_INFINITY : window.innerWidth,
  );

  #promptResolve: ((choice: UnsavedChangesChoice) => void) | null = null;

  readonly loading = this.#store.loading;
  readonly saving = this.#store.saving;
  readonly loadProblem = this.#store.loadProblem;
  readonly saveProblem = this.#store.saveProblem;
  readonly snapshot = this.#store.snapshot;
  readonly formErrors = this.#formErrors.asReadonly();
  readonly announcement = this.#announcement.asReadonly();
  readonly promptOpen = this.#promptOpen.asReadonly();

  /** One `FormGroup` per config, built from the field descriptors. */
  readonly headerForm = computed<FormGroup<Record<string, FormControl<unknown>>>>(() =>
    buildHeaderForm(this.config().header),
  );

  /** One `FormArray` per declared grid - documents declare exactly one (survey). */
  readonly lineForms = computed<ReadonlyMap<string, LineItemFormArrayModel<TLine>>>(() => {
    const map = new Map<string, LineItemFormArrayModel<TLine>>();
    for (const grid of this.config().lineGrids) {
      map.set(grid.id, new FormArray<LineItemFormGroup<TLine>>([]));
    }
    return map;
  });

  readonly resolvedId = computed<DocumentId | null>(() => this.documentId() ?? this.#routeId());

  readonly effectiveMode = computed<DocumentEditorMode>(
    () => this.mode() ?? this.#routeMode() ?? (this.resolvedId() === null ? 'create' : 'edit'),
  );

  readonly belowMobileBreakpoint = computed(
    () => this.#viewportWidth() < DOCUMENT_EDITOR_READONLY_BREAKPOINT_PX,
  );

  /** `view`, or a phone-sized viewport: editing is not offered, but everything is readable. */
  readonly readOnly = computed(
    () => this.effectiveMode() === 'view' || this.belowMobileBreakpoint(),
  );

  readonly dirty = computed(() => {
    this.#revision();
    if (this.headerForm().dirty) {
      return true;
    }
    for (const array of this.lineForms().values()) {
      if (array.dirty) {
        return true;
      }
    }
    return false;
  });

  readonly permissionProblem = computed<ApiProblem | null>(() => {
    const problem = this.saveProblem();
    return problem?.status === 403 ? problem : null;
  });

  /**
   * A UX affordance only - the server enforces the same right independently
   * (ADR-004).
   *
   * The predicate is **config data**, not an injected service, because nothing
   * under `shared/components/**` may reach into the authentication layer - an invariant this
   * workspace enforces by test
   * (`feedback/permission-denied-state.component.spec.ts:29-49`, *"it renders a
   * denial, it does not evaluate one"*). The feature module reads M2-C02's
   * `PermissionService` and passes the answer in.
   */
  readonly canSave = computed(() => this.config().canSave?.() ?? true);

  /**
   * A grid with no `tabId` is always shown - the document case, where the
   * survey found exactly one grid per screen. A grid that names a tab follows
   * the header form's active tab, which is the `ItemUpsert.razor` shape.
   */
  readonly visibleGrids = computed<readonly DocumentLineGrid<TLine>[]>(() => {
    const activeTab = this.headerFormRef()?.activeTabId() ?? null;
    return this.config().lineGrids.filter(
      (grid) => grid.tabId === undefined || grid.tabId === activeTab,
    );
  });

  readonly rowErrors = computed<readonly LineItemRowError[]>(
    () => this.snapshot()?.rowErrors ?? [],
  );

  readonly regionTemplateMap = computed<Readonly<Record<string, TemplateRef<unknown>>>>(() => {
    const map: Record<string, TemplateRef<unknown>> = {};
    for (const directive of this.regionTemplates()) {
      map[directive.appDocumentRegion()] = directive.template;
    }
    return map;
  });

  readonly pickerGrid = computed<DocumentLineGrid<TLine> | null>(() => {
    const id = this.#pickerGridId();
    return id === null ? null : (this.config().lineGrids.find((g) => g.id === id) ?? null);
  });

  constructor() {
    this.#watchRoute();
    this.#watchViewport();

    // Operations come from the config, so the store is configured whenever the
    // config changes and never before.
    effect(() => {
      const config = this.config();
      untracked(() => this.#store.configure(config.operations));
    });

    // Dirty tracking: `FormGroup.dirty` is not a signal, so the form's own
    // event stream is what makes `dirty()` reactive.
    effect((onCleanup) => {
      const header = this.headerForm();
      const arrays = [...this.lineForms().values()];
      const subscriptions = [header, ...arrays].map((control) =>
        control.events.subscribe(() => this.#revision.update((n) => n + 1)),
      );
      onCleanup(() => subscriptions.forEach((s) => s.unsubscribe()));
    });

    // Load / reset when the identity or the mode changes.
    effect(() => {
      const id = this.resolvedId();
      const mode = this.effectiveMode();
      untracked(() => {
        if (mode === 'create' || id === null) {
          this.#store.reset();
          this.#resetForm();
        } else {
          void this.#load(id);
        }
      });
    });

    // `beforeunload` is registered **only while dirty**, and removed on destroy.
    effect((onCleanup) => {
      if (!this.dirty() || typeof window === 'undefined') {
        return;
      }
      const listener = (event: BeforeUnloadEvent): void => {
        event.preventDefault();
        event.returnValue = '';
      };
      window.addEventListener('beforeunload', listener);
      onCleanup(() => window.removeEventListener('beforeunload', listener));
    });

    this.#destroyRef.onDestroy(() => this.#resolvePrompt('stay'));
  }

  // --- Public API -----------------------------------------------------------

  linesFor(gridId: string): LineItemFormArrayModel<TLine> {
    const array = this.lineForms().get(gridId);
    if (!array) {
      throw new Error(`No line FormArray for grid "${gridId}".`);
    }
    return array;
  }

  /** Forwarded verbatim to the config's operation. The shell interprets nothing. */
  onRowEvent(gridId: string, event: LineItemRowEvent<TLine>): void {
    this.config().operations.rowEvent?.(event, {
      gridId,
      mode: this.effectiveMode(),
      documentId: this.resolvedId(),
      header: this.headerForm().getRawValue() as Partial<THeader>,
    });
  }

  onPullFromUpstream(gridId: string): void {
    this.#pickerGridId.set(gridId);
  }

  onUpstreamConfirmed(rows: readonly unknown[]): void {
    const grid = this.pickerGrid();
    const upstream = grid?.upstream;
    this.#pickerGridId.set(null);
    if (!grid || !upstream) {
      return;
    }
    const index = this.visibleGrids().findIndex((candidate) => candidate.id === grid.id);
    const component = this.gridRefs()[index] as LineItemGridComponent<TLine> | undefined;
    component?.pullLines(upstream.toLines(rows));
  }

  onUpstreamCancelled(): void {
    this.#pickerGridId.set(null);
  }

  retryLoad(): void {
    const id = this.resolvedId();
    if (id !== null) {
      void this.#load(id);
    }
  }

  /**
   * Save. Returns `true` only when the server accepted it.
   *
   * The order is fixed: shape-validate for immediate feedback, POST/PUT, then
   * an **explicit `refresh()`** - never a local mutation of the cached
   * document, and never an optimistic write of the outgoing payload
   * (BR-CALC-001, BR-STK-001; KB-050 §Data-fetching conventions).
   */
  async save(): Promise<boolean> {
    if (this.readOnly()) {
      return false;
    }
    const header = this.headerForm();
    header.markAllAsTouched();
    if (header.invalid) {
      this.#announce('The document has validation errors.');
      this.#focusFirstInvalid();
      return false;
    }

    const id = this.effectiveMode() === 'create' ? null : this.resolvedId();
    const result = await this.#store.save(this.#payload(), id);
    if (result === null) {
      this.#handleSaveProblem();
      return false;
    }

    this.#formErrors.set([]);
    this.#markPristine();
    this.#toast?.success(`${this.config().noun} saved.`);
    this.#announce(`${this.config().noun} saved.`);
    await this.#store.refresh();
    this.#applySnapshot(this.#store.snapshot());
    this.#focusAnchor();
    this.saved.emit(result);
    return true;
  }

  /** Save, then start a genuinely new document - no refresh, because nothing is being re-read. */
  async saveAndNew(): Promise<void> {
    if (this.readOnly()) {
      return;
    }
    const header = this.headerForm();
    header.markAllAsTouched();
    if (header.invalid) {
      this.#focusFirstInvalid();
      return;
    }
    const result = await this.#store.save(this.#payload(), null);
    if (result === null) {
      this.#handleSaveProblem();
      return;
    }
    this.saved.emit(result);
    this.#toast?.success(`${this.config().noun} saved.`);
    this.#announce(`${this.config().noun} saved. A new one is ready.`);
    this.#store.reset();
    this.#resetForm();
    this.headerFormRef()?.revealFirstField();
  }

  /** Template entry points - the template never holds a promise. */
  onSave(): void {
    void this.save();
  }

  onSaveAndNew(): void {
    void this.saveAndNew();
  }

  cancel(): void {
    this.cancelled.emit();
    const route = this.config().cancelRoute;
    if (route && this.#router) {
      void this.#router.navigate([...route]);
    }
  }

  // --- Dirty guard ----------------------------------------------------------

  /**
   * The `CanDeactivateFn`'s implementation. Three outcomes, not two - the
   * existing `UnsavedChangesModal.razor` offers Continue / Discard / Cancel and
   * the shared `app-confirm-dialog` is a two-outcome surface by contract
   * (INV-006's M2-C04-03 amendment), so this composes `app-modal` instead.
   */
  async canDeactivateDocument(): Promise<boolean> {
    if (!this.dirty()) {
      return true;
    }
    const choice = await this.#prompt();
    if (choice === 'stay') {
      return false;
    }
    if (choice === 'discard') {
      return true;
    }
    return await this.save();
  }

  choosePrompt(choice: UnsavedChangesChoice): void {
    this.#resolvePrompt(choice);
  }

  #prompt(): Promise<UnsavedChangesChoice> {
    this.#promptOpen.set(true);
    return new Promise<UnsavedChangesChoice>((resolve) => {
      this.#promptResolve = resolve;
    });
  }

  #resolvePrompt(choice: UnsavedChangesChoice): void {
    const resolve = this.#promptResolve;
    this.#promptResolve = null;
    this.#promptOpen.set(false);
    resolve?.(choice);
  }

  // --- Internals ------------------------------------------------------------

  async #load(id: DocumentId): Promise<void> {
    await this.#store.load(id);
    this.#applySnapshot(this.#store.snapshot());
  }

  #payload(): DocumentPayload<THeader, TLine> {
    const linesByGrid: Record<string, readonly Partial<TLine>[]> = {};
    for (const [gridId, array] of this.lineForms()) {
      linesByGrid[gridId] = array.getRawValue() as Partial<TLine>[];
    }
    const first = this.config().lineGrids[0];
    return {
      header: this.headerForm().getRawValue() as Partial<THeader>,
      lines: first ? (linesByGrid[first.id] ?? []) : [],
      linesByGrid,
    };
  }

  #applySnapshot(snapshot: DocumentSnapshot<THeader, TLine> | null): void {
    if (snapshot === null) {
      return;
    }
    this.headerForm().patchValue(snapshot.header as Record<string, unknown>, {
      emitEvent: false,
    });
    for (const grid of this.config().lineGrids) {
      const rows =
        snapshot.linesByGrid?.[grid.id] ??
        (grid.id === this.config().lineGrids[0]?.id ? snapshot.lines : []);
      const array = this.linesFor(grid.id);
      array.clear({ emitEvent: false });
      for (const row of rows) {
        array.push(grid.createRow(row), { emitEvent: false });
      }
    }
    this.#markPristine();
  }

  #resetForm(): void {
    this.headerForm().reset(undefined, { emitEvent: false });
    for (const grid of this.config().lineGrids) {
      this.linesFor(grid.id).clear({ emitEvent: false });
    }
    this.#markPristine();
    this.#formErrors.set([]);
  }

  #markPristine(): void {
    this.headerForm().markAsPristine();
    for (const array of this.lineForms().values()) {
      array.markAsPristine();
    }
    this.#revision.update((n) => n + 1);
  }

  /**
   * A refused save. 400 maps onto the controls and opens the offending section;
   * 403 renders inline; 409 shows the server's `title` **verbatim**.
   */
  #handleSaveProblem(): void {
    const problem = this.#store.saveProblem();
    if (!problem) {
      return;
    }
    if (problem.status === 403) {
      this.#announce('You do not have permission to save this document.');
      return;
    }
    const result = applyServerErrors(this.headerForm(), problem);
    this.#formErrors.set(result.formLevel);
    if (problem.title) {
      this.#toast?.error(problem.title);
    }
    this.#announce(problem.title ?? 'The document could not be saved.');
    const firstApplied = result.applied[0];
    if (firstApplied) {
      this.headerFormRef()?.revealField(firstApplied);
    }
  }

  #focusFirstInvalid(): void {
    const controls = this.headerForm().controls;
    const firstInvalid = Object.keys(controls).find((name) => controls[name]?.invalid);
    if (firstInvalid) {
      this.headerFormRef()?.revealField(firstInvalid);
    }
  }

  /** The deterministic post-save focus anchor: the document's own heading. */
  #focusAnchor(): void {
    this.anchorRef()?.nativeElement.focus();
  }

  #announce(message: string): void {
    this.#announcement.set(message);
  }

  #watchRoute(): void {
    const route = this.#route;
    if (!route) {
      return;
    }
    const params = route.paramMap.subscribe((map) => {
      const raw = map.get('id');
      this.#routeId.set(raw === null || raw === '' ? null : raw);
    });
    const queries = route.queryParamMap.subscribe((map) => {
      const raw = map.get('mode');
      this.#routeMode.set(raw === 'view' || raw === 'edit' || raw === 'create' ? raw : null);
    });
    this.#destroyRef.onDestroy(() => {
      params.unsubscribe();
      queries.unsubscribe();
    });
  }

  #watchViewport(): void {
    if (typeof window === 'undefined') {
      return;
    }
    const onResize = (): void => this.#viewportWidth.set(window.innerWidth);
    window.addEventListener('resize', onResize, { passive: true });
    this.#destroyRef.onDestroy(() => window.removeEventListener('resize', onResize));
  }
}
