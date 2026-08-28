import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChildren,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import {
  CheckboxComponent,
  DatePickerComponent,
  FormFieldComponent,
  FormLayoutComponent,
  FormSectionComponent,
  NumberInputComponent,
  SelectComponent,
  TextInputComponent,
  TextareaComponent,
  type SelectOption,
} from '../form';
import { TabsComponent, type TabItem } from '../tabs/tabs.component';
import {
  sectionStateKey,
  type DocumentEditorMode,
  type DocumentHeaderField,
  type DocumentHeaderLayout,
  type DocumentHeaderSection,
} from './document-editor.model';

/**
 * The header region of `<app-document-editor>` (M2-C08-01).
 *
 * It renders M2-C04-02's `FormLayout` / `FormSection` / `FormField` from the
 * config's field descriptors - flat sections for a document, tabs for the one
 * sampled master shape (`ItemUpsert.razor`) - and it does nothing else. It
 * derives no default, cascades no value and validates nothing beyond the
 * shape validators the config handed it.
 *
 * Section open/closed state is remembered per document type in local storage,
 * read from `FormSection`'s own `isCollapsed()` signal rather than by
 * duplicating its state: `FormSection` owns collapsing, this component owns
 * persistence.
 */
@Component({
  selector: 'app-document-header-form',
  templateUrl: './document-header-form.component.html',
  styleUrl: './document-header-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    FormLayoutComponent,
    FormSectionComponent,
    FormFieldComponent,
    TextInputComponent,
    TextareaComponent,
    NumberInputComponent,
    DatePickerComponent,
    SelectComponent,
    CheckboxComponent,
    TabsComponent,
  ],
})
export class DocumentHeaderFormComponent<THeader> {
  readonly #host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly layout = input.required<DocumentHeaderLayout<THeader>>();
  readonly form = input.required<FormGroup<Record<string, FormControl<unknown>>>>();
  /** Persistence key for section state. Never a module noun written into this directory. */
  readonly documentType = input.required<string>();
  readonly mode = input<DocumentEditorMode>('edit');
  /** `view` mode, or a viewport below 768 px. Read-only is not disabled (KB-051 Forms). */
  readonly readOnly = input(false);
  readonly loading = input(false);

  readonly #activeTab = signal<string | null>(null);
  /** What the operator last chose for this document type. */
  readonly #collapsedOverrides = computed(() =>
    readSectionMap(sectionStateKey(this.documentType())),
  );

  private readonly sectionRefs = viewChildren(FormSectionComponent);

  readonly tabs = computed<readonly TabItem[]>(() => {
    const layout = this.layout();
    return layout.kind === 'tabbed'
      ? layout.tabs.map((tab) => ({ id: tab.id, label: tab.label }))
      : [];
  });

  readonly activeTabId = computed(() => this.#activeTab() ?? this.tabs()[0]?.id ?? '');

  /** The sections currently in the DOM - all of them when flat, one tab's when tabbed. */
  readonly visibleSections = computed<readonly DocumentHeaderSection<THeader>[]>(() => {
    const layout = this.layout();
    if (layout.kind === 'flat') {
      return layout.sections;
    }
    const active = this.activeTabId();
    return layout.tabs.find((tab) => tab.id === active)?.sections ?? [];
  });

  constructor() {
    // Persistence: read FormSection's own state rather than shadowing it.
    effect(() => {
      const sections = this.sectionRefs();
      const visible = this.visibleSections();
      const key = sectionStateKey(this.documentType());
      const state: Record<string, boolean> = { ...readSectionMap(key) };
      let changed = false;
      sections.forEach((section, index) => {
        const descriptor = visible[index];
        if (!descriptor) {
          return;
        }
        const collapsed = section.isCollapsed();
        if (state[descriptor.id] !== collapsed) {
          state[descriptor.id] = collapsed;
          changed = true;
        }
      });
      if (changed) {
        writeSectionState(key, state);
      }
    });

    // `create` puts the caret in the first header field. `edit`/`view` leave
    // focus where the operator put it.
    afterNextRender(() => {
      if (this.mode() === 'create' && !this.loading()) {
        this.#firstFocusable()?.focus();
      }
    });
  }

  selectTab(id: string): void {
    this.#activeTab.set(id);
  }

  controlFor(field: DocumentHeaderField<THeader>): FormControl<unknown> {
    const control = this.form().controls[field.name];
    if (!control) {
      throw new Error(`No header control named "${field.name}".`);
    }
    return control;
  }

  isFieldReadOnly(field: DocumentHeaderField<THeader>): boolean {
    return this.readOnly() || field.readOnly === true;
  }

  optionsFor(field: DocumentHeaderField<THeader>): readonly SelectOption[] {
    return field.options ?? [];
  }

  /** The initial collapsed state: what the operator last chose, else what the config declared. */
  initiallyCollapsed(section: DocumentHeaderSection<THeader>): boolean {
    const remembered = this.#collapsedOverrides()[section.id];
    return remembered ?? section.initiallyCollapsed ?? false;
  }

  /**
   * Opens the section containing `name`, then focuses its control. Called by
   * the editor after a 400 maps server errors onto the form.
   */
  revealField(name: string): void {
    const sectionIndex = this.visibleSections().findIndex((section) =>
      section.fields.some((field) => field.name === name),
    );
    if (sectionIndex >= 0) {
      const section = this.sectionRefs()[sectionIndex];
      if (section?.isCollapsed()) {
        section.toggle();
      }
    }
    const wrapper = this.#host.nativeElement.querySelector<HTMLElement>(
      `[data-field="${cssEscape(name)}"]`,
    );
    focusableIn(wrapper)?.focus();
  }

  /** Focus the first header field - `create`, and Save + New's fresh document. */
  revealFirstField(): void {
    this.#firstFocusable()?.focus();
  }

  #firstFocusable(): HTMLElement | null {
    return focusableIn(this.#host.nativeElement);
  }
}

function focusableIn(root: HTMLElement | null): HTMLElement | null {
  return (
    root?.querySelector<HTMLElement>(
      'input:not([type="hidden"]):not([disabled]), textarea:not([disabled]), select:not([disabled])',
    ) ?? null
  );
}

/** Attribute selectors are the only place a field name reaches the DOM; keep it valid. */
function cssEscape(value: string): string {
  return value.replace(/["\\]/g, '\\$&');
}

/**
 * One document type's section state. Read defensively: local storage is
 * user-writable and may be absent entirely (a locked-down browser, or a
 * non-browser platform).
 */
function readSectionMap(key: string): Readonly<Record<string, boolean>> {
  if (typeof localStorage === 'undefined') {
    return {};
  }
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(key) ?? '{}');
    return parsed !== null && typeof parsed === 'object' ? (parsed as Record<string, boolean>) : {};
  } catch {
    // A corrupt entry is not worth failing a page load over.
    return {};
  }
}

function writeSectionState(key: string, value: Readonly<Record<string, boolean>>): void {
  if (typeof localStorage === 'undefined') {
    return;
  }
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    // Quota or a private-mode restriction. Losing a UI preference is not an error.
  }
}
