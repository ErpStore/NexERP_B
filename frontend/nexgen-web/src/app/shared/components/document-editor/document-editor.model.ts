import { FormControl, FormGroup, Validators, type ValidatorFn } from '@angular/forms';
import type { Observable } from 'rxjs';

import type { DataGridColumn, DataGridRowId, DataGridRowIdFn } from '../data-grid';
import type { SelectOption } from '../form';
import type {
  LineItemColumn,
  LineItemRowError,
  LineItemRowEvent,
  LineItemRowFactory,
} from '../line-item-grid';
import type { RecordPickerFetchPage } from '../record-picker-dialog';

/**
 * `DocumentEditorConfig` (M2-C08-01) - the object a feature module supplies to
 * `<app-document-editor>`. Specification: `docs/kb/execution/tasks/M2-C08-01.md`.
 *
 * **Everything module-specific is data.** There is no subclass, no fork and no
 * module-specific field name, status value, noun or endpoint anywhere in this
 * directory: the load/save/calculate/command operations are supplied as
 * functions, so the shell imports no generated operation and hardcodes no URL
 * (ADR-007 `:162-164`; KB-050 §Document editor pattern).
 *
 * **Nothing here computes an ERP value.** Validators built from these
 * descriptors mirror the server's `DataAnnotations` for immediate feedback
 * only; the server stays authoritative for validation, calculation,
 * permissions and document numbering. Totals are a *descriptor* here and a
 * server response at run time (M2-C08-02); commands are a *vocabulary* here
 * and a server call at run time (M2-C08-03).
 *
 * ### What the layout survey found, and how this contract answers it
 *
 * The survey (INV-062, recorded in `docs/kb/investigation-registry.md`) read the
 * five largest Upsert screens named by the task. Each answer is load-bearing on
 * a field below:
 *
 * | Survey question | Answer | Contract |
 * |---|---|---|
 * | Two or more line grids? | **No** for all four document screens - exactly one each (`MfgPOUpsert.razor:750`, `PurchPOUpsert.razor:686`, `MfgInvUpsert.razor:990`, `LabourDcOutgoingUpsert.razor:994`). `ItemUpsert.razor`, a master, has four (`:1070`, `:1330`, `:1790`, `:1957`), one per tab | {@link DocumentEditorConfig.lineGrids} is an **array**, and a grid may name the header tab it belongs to |
 * | Tabbed header? | Only `ItemUpsert.razor` (`activeTab`, four tabs, declared `:2141`). The four documents are one flat scrolling form of `card` sections | {@link DocumentHeaderLayout} is a discriminated union of `flat` and `tabbed` |
 * | Totals panel? | **Three of five** (`MfgPOUpsert.razor:1507-1860`, `PurchPOUpsert.razor:1597-1940`, `MfgInvUpsert.razor:1431-1770`). `LabourDcOutgoingUpsert.razor` and `ItemUpsert.razor` have none | {@link DocumentEditorConfig.totals} is **optional** |
 * | Command vocabulary? | Create/Update, Back, Print, Order Acceptance, Revise, E-Way, E-Invoice (+ Preview), Download Template, Import, Add Revision, Copy. **No Cancel and no Save + New exists on any of the five** - both are new affordances. Document-level toggles (cancel document, short close) live in the *header* region, not the command bar (`MfgPOUpsert.razor:449`/`:469`) | {@link DocumentCommand} carries a `placement`, so a command can be declared for the header region rather than the footer |
 * | Sub-line (line -> sub-line) grid? | **None** in any of the four documents. The one nesting case is master -> detail -> sub-grid in `ItemUpsert.razor` (`:1070` -> `EditRM` `:1100` -> second form `:1122` -> process grid `:1330`) | {@link DocumentLineGrid.subLines} exists so the contract does not have to change under M3-5; see its own comment for what the shell does and does not do with it today |
 */

// --- Identity and modes ------------------------------------------------------

/** A document's server identity. `string` because a route parameter is one. */
export type DocumentId = string | number;

/**
 * One route plus a mode replaces the existing create / create-with-parent /
 * update / details split (KB-050 design constraint 2; the mapping is recorded
 * in KB-053 §Route conventions). Survey note: no `view`/details route exists
 * for any of the five sampled screens, so `view` is new capability, not a
 * consolidation of something that already shipped.
 */
export type DocumentEditorMode = 'create' | 'edit' | 'view';

// --- Header form -------------------------------------------------------------

/**
 * Which M2-C04-02 control renders a header field. Deliberately a small set:
 * a document header in the sampled screens is text, dates, selects, flags and
 * remarks. Anything richer belongs in a side region, not in a new control kind.
 */
export type DocumentFieldControl = 'text' | 'textarea' | 'number' | 'date' | 'select' | 'checkbox';

/** One header field. The generated OpenAPI shape supplies `THeader`; this describes how it is edited. */
export interface DocumentHeaderField<THeader> {
  readonly name: Extract<keyof THeader, string>;
  readonly label: string;
  readonly control: DocumentFieldControl;
  readonly hint?: string;
  /** Adds `Validators.required` and the required affordance. Mirrors `[Required]`; never the authority. */
  readonly required?: boolean;
  /** Anything beyond `required` - generated from OpenAPI, never hand-invented. */
  readonly validators?: readonly ValidatorFn[];
  /** Required when `control === 'select'`. */
  readonly options?: readonly SelectOption[];
  /** The value a `create` starts with. Never a derived default - those are the server's. */
  readonly initialValue?: unknown;
  /**
   * Read-only in every mode - a server-derived field such as a document
   * number. Read-only is not disabled: the value stays selectable, copyable and
   * in the tab order (KB-051 §Forms).
   */
  readonly readOnly?: boolean;
  readonly placeholder?: string;
}

/** A titled group of header fields; renders as `app-form-section`. */
export interface DocumentHeaderSection<THeader> {
  readonly id: string;
  readonly title: string;
  readonly description?: string;
  readonly collapsible?: boolean;
  /** The initial state only. What the operator chose afterwards is remembered per document type. */
  readonly initiallyCollapsed?: boolean;
  readonly fields: readonly DocumentHeaderField<THeader>[];
}

/** A header tab - the `ItemUpsert.razor` shape. Documents do not use these. */
export interface DocumentHeaderTab<THeader> {
  readonly id: string;
  readonly label: string;
  readonly sections: readonly DocumentHeaderSection<THeader>[];
}

/**
 * Flat (all four sampled documents) or tabbed (`ItemUpsert.razor` only). A
 * union rather than an optional `tabs` field, so a config cannot declare both.
 */
export type DocumentHeaderLayout<THeader> =
  | { readonly kind: 'flat'; readonly sections: readonly DocumentHeaderSection<THeader>[] }
  | { readonly kind: 'tabbed'; readonly tabs: readonly DocumentHeaderTab<THeader>[] };

/** Every section in a layout, flat or tabbed, in declaration order. */
export function headerSectionsOf<THeader>(
  layout: DocumentHeaderLayout<THeader>,
): readonly DocumentHeaderSection<THeader>[] {
  return layout.kind === 'flat' ? layout.sections : layout.tabs.flatMap((tab) => tab.sections);
}

/** Every field in a layout, in declaration order. */
export function headerFieldsOf<THeader>(
  layout: DocumentHeaderLayout<THeader>,
): readonly DocumentHeaderField<THeader>[] {
  return headerSectionsOf(layout).flatMap((section) => section.fields);
}

/** The section a control name belongs to - what "open the section containing the first error" needs. */
export function sectionOfField<THeader>(
  layout: DocumentHeaderLayout<THeader>,
  name: string,
): DocumentHeaderSection<THeader> | null {
  return (
    headerSectionsOf(layout).find((section) =>
      section.fields.some((field) => field.name === name),
    ) ?? null
  );
}

/**
 * Builds the header `FormGroup` from the field descriptors. One control per
 * field, flat - the server's `ModelState` keys are flat property names, so a
 * nested group would put every 400 error out of reach of `applyServerErrors`.
 */
export function buildHeaderForm<THeader>(
  layout: DocumentHeaderLayout<THeader>,
): FormGroup<Record<string, FormControl<unknown>>> {
  const controls: Record<string, FormControl<unknown>> = {};
  for (const field of headerFieldsOf(layout)) {
    const validators: ValidatorFn[] = [...(field.validators ?? [])];
    if (field.required) {
      // Wrapped rather than passed by reference: an unbound static method is a
      // scoping hazard the workspace lints against.
      validators.unshift((control) => Validators.required(control));
    }
    controls[field.name] = new FormControl<unknown>(field.initialValue ?? null, {
      nonNullable: false,
      validators,
    });
  }
  return new FormGroup<Record<string, FormControl<unknown>>>(controls);
}

// --- Line grids --------------------------------------------------------------

/**
 * The upstream-pull affordance `LineItemGrid` raises and `RecordPickerDialog`
 * serves. The shell owns neither: it opens the picker with the caller's
 * `fetchPage`, and hands whatever the caller's `toLines` returns straight to
 * the grid's `pullLines()`. Eligibility, duplicates, balance quantities and
 * pricing are the server's (INV-054).
 */
export interface DocumentUpstreamPull<TLine, TRow = unknown> {
  readonly header: string;
  readonly columns: readonly DataGridColumn<TRow>[];
  readonly fetchPage: RecordPickerFetchPage<TRow>;
  readonly getRowId: DataGridRowIdFn<TRow>;
  /** Rows the server says may not be pulled. The dialog derives none of this itself. */
  readonly disabledRowIds?: readonly DataGridRowId[];
  /** Maps chosen candidate rows to new line values. Pure mapping; it computes no money. */
  readonly toLines: (rows: readonly TRow[]) => readonly Partial<TLine>[];
  readonly confirmLabel?: string;
}

/**
 * A second-level child grid under a line.
 *
 * **No sampled document needs one** - the survey found none in `MfgPOUpsert`,
 * `PurchPOUpsert`, `MfgInvUpsert` or `LabourDcOutgoingUpsert`, and the single
 * nesting case in `ItemUpsert.razor` is master -> detail -> sub-grid, not
 * line -> sub-line. It is declared here because the task requires the contract
 * to accommodate the survey's answer *now* rather than reshape under M3-5, and
 * because the survey covered five of ~65 screens, not all of them.
 *
 * **What the shell does with it today: records it, and renders nothing.**
 * Rendering a child grid inside a parent row needs row expansion in
 * `LineItemGrid`, which M2-C08-01 must not modify (its *Files that must not
 * change* list). The first module that genuinely needs sub-lines raises a
 * change request against M2-C07; the shape it will bind to is already fixed
 * here, so that change is additive rather than a contract break. Recorded in
 * `docs/kb/risks/technical-debt-register.md`.
 */
export interface DocumentSubLineGrid<TSubLine> {
  readonly id: string;
  readonly title: string;
  /** The `FormArray` inside a parent row's `FormGroup` that holds the children. */
  readonly formArrayName: string;
  readonly columns: readonly LineItemColumn<TSubLine>[];
  readonly createRow: LineItemRowFactory<TSubLine>;
}

/** One `LineItemGrid` instance. Documents declare exactly one; a tabbed master may declare several. */
export interface DocumentLineGrid<TLine, TSubLine = unknown, TRow = unknown> {
  readonly id: string;
  readonly title: string;
  readonly columns: readonly LineItemColumn<TLine>[];
  readonly createRow: LineItemRowFactory<TLine>;
  /** Accessible name for the grid; defaults to `title`. */
  readonly ariaLabel?: string;
  /** Which header tab this grid belongs to. Omitted - the document case - means always visible. */
  readonly tabId?: string;
  readonly upstream?: DocumentUpstreamPull<TLine, TRow>;
  readonly subLines?: DocumentSubLineGrid<TSubLine>;
}

// --- Totals, commands, side regions -----------------------------------------

/** One line of the totals ladder. Its **value** arrives from the server (M2-C08-02). */
export interface DocumentTotalsRow {
  /** The key this row's value arrives under in the calculate response. */
  readonly key: string;
  readonly label: string;
  /** Grand Total and the like - emphasis only, never a computation. */
  readonly emphasis?: boolean;
}

/**
 * The totals descriptor. **Optional**: `LabourDcOutgoingUpsert.razor` and
 * `ItemUpsert.razor` show no totals at all. Two totals surfaces exist in the
 * legacy screens - a grid-footer row and this side panel - and `surface` says
 * which this descriptor is about, so M2-C08-02 and M2-C07 cannot disagree.
 */
export interface DocumentTotalsDescriptor {
  readonly title: string;
  readonly rows: readonly DocumentTotalsRow[];
  readonly surface?: 'panel' | 'grid-footer';
}

/** Values keyed by {@link DocumentTotalsRow.key}, already formatted by the server. */
export type DocumentTotals = Readonly<Record<string, string>>;

/**
 * A workflow command's *declaration*. Dispatch is M2-C08-03; this shell renders
 * no button from this array - the command slot renders only the caller's own
 * template. Declared now so the contract does not change under the sibling.
 */
export interface DocumentCommand {
  readonly id: string;
  readonly label: string;
  /**
   * Where the command belongs. `header` exists because the survey found the
   * document-level toggles (cancel document, short close) rendered in the
   * header region, not the command bar (`MfgPOUpsert.razor:449`/`:469`).
   */
  readonly placement?: 'command-bar' | 'header';
  /** Collect a reason and refuse without one - the BR-SO-003 *capability*, not the rule. */
  readonly requiresReason?: boolean;
  readonly destructive?: boolean;
  readonly availableInModes?: readonly DocumentEditorMode[];
  /** The screen right this command needs - a UX affordance; the server enforces independently. */
  readonly right?: 'view' | 'create' | 'edit' | 'delete';
}

/** Attachments / terms / remarks / audit trail. Content is projected, never invented here. */
export interface DocumentSideRegion {
  readonly id: string;
  readonly label: string;
}

// --- Operations --------------------------------------------------------------

/** What the server returns for one document. Totals and row errors included: both are the server's. */
export interface DocumentSnapshot<THeader, TLine> {
  readonly id?: DocumentId;
  readonly header: THeader;
  readonly lines: readonly TLine[];
  /** Keyed by grid id when a config declares more than one grid. */
  readonly linesByGrid?: Readonly<Record<string, readonly TLine[]>>;
  readonly totals?: DocumentTotals;
  /** The status the header badge shows. A server value, rendered verbatim. */
  readonly status?: string;
  readonly documentNumber?: string;
  readonly rowErrors?: readonly LineItemRowError[];
}

/** What the shell sends. It adds nothing of its own - no number, no default, no total. */
export interface DocumentPayload<THeader, TLine> {
  readonly header: Partial<THeader>;
  readonly lines: readonly Partial<TLine>[];
  readonly linesByGrid: Readonly<Record<string, readonly Partial<TLine>[]>>;
}

/** Passed to the config's row-event handler so it can reach the rest of the document. */
export interface DocumentRowEventContext<THeader> {
  readonly gridId: string;
  readonly mode: DocumentEditorMode;
  readonly documentId: DocumentId | null;
  readonly header: Partial<THeader>;
}

/**
 * Every server interaction, as functions. The shell calls them and interprets
 * nothing: `rowEvent` in particular is forwarded **verbatim**, `respond`
 * callback included, so the answer to "what does this edit mean" is always the
 * caller's and ultimately the server's.
 */
export interface DocumentEditorOperations<THeader, TLine> {
  readonly load: (id: DocumentId) => Observable<DocumentSnapshot<THeader, TLine>>;
  readonly create: (
    payload: DocumentPayload<THeader, TLine>,
  ) => Observable<DocumentSnapshot<THeader, TLine>>;
  readonly update: (
    id: DocumentId,
    payload: DocumentPayload<THeader, TLine>,
  ) => Observable<DocumentSnapshot<THeader, TLine>>;
  /** M2-C08-02 wires this. Declared now so the sibling changes no type. */
  readonly calculate?: (payload: DocumentPayload<THeader, TLine>) => Observable<DocumentTotals>;
  /** M2-C08-03 wires this. `reason` carries BR-SO-003's mandatory cancellation reason. */
  readonly command?: (
    id: DocumentId,
    commandId: string,
    input: { readonly reason?: string },
  ) => Observable<DocumentSnapshot<THeader, TLine>>;
  /** Forwarded, never interpreted. The grid's own `respond()` applies whatever the caller returns. */
  readonly rowEvent?: (
    event: LineItemRowEvent<TLine>,
    context: DocumentRowEventContext<THeader>,
  ) => void;
}

// --- The config --------------------------------------------------------------

export interface DocumentEditorConfig<THeader, TLine> {
  /**
   * A stable key for this document type - the key section open/closed state is
   * remembered under, and nothing else. Supplied by the feature module, so no
   * module-specific noun is written into this directory.
   */
  readonly documentType: string;
  /** The `<h1>`. */
  readonly title: string;
  /** The singular noun used in shell-composed copy - "Save {noun}", "{noun} saved". */
  readonly noun: string;
  /** The resource slug, for a config that uses {@link createHttpDocumentOperations}. */
  readonly resource: string;
  /** `Screens.ScreenName` verbatim - used only to name the screen in a 403 panel. */
  readonly screenName?: string;
  /**
   * Whether Save is offered at all. A **UX affordance**; the server enforces
   * the right independently (ADR-004). Supplied as a predicate by the feature
   * module - which reads M2-C02's `PermissionService` - because nothing under
   * `shared/components/**` may import the authentication layer
   * (`feedback/permission-denied-state.component.spec.ts:29-49`). Read inside a
   * `computed()`, so a signal-backed predicate stays reactive.
   */
  readonly canSave?: () => boolean;
  readonly header: DocumentHeaderLayout<THeader>;
  readonly lineGrids: readonly DocumentLineGrid<TLine>[];
  /** Optional - not every document is priced. */
  readonly totals?: DocumentTotalsDescriptor;
  readonly sideRegions?: readonly DocumentSideRegion[];
  /** Declared for M2-C08-03; this task renders none of them. */
  readonly commands?: readonly DocumentCommand[];
  readonly operations: DocumentEditorOperations<THeader, TLine>;
  /** Where Cancel goes. A router commands array; the shell names no route of its own. */
  readonly cancelRoute?: readonly unknown[];
}

// --- Persistence key ---------------------------------------------------------

/** Local-storage key for one document type's section open/closed state. */
export function sectionStateKey(documentType: string): string {
  return `nexgen.document-editor.sections.${documentType}`;
}
