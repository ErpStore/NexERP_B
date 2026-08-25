import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  afterNextRender,
  computed,
  forwardRef,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AutoComplete, type AutoCompleteCompleteEvent } from 'primeng/autocomplete';

import { BaseFormControl } from './base-control';
import type { ComboboxLoader, SelectOption } from './types';

/**
 * Search-and-select against a list too large to ship to the browser: the
 * `SearchCustomers` / `SearchItems` interaction.
 *
 * **Scope, stated plainly.** This is only the search-and-select interaction.
 * The party *cascade* - customer -> currency, terms, consignee, cost centre,
 * tax mode - is ERP business logic. It exists today as a per-page
 * `ApplyCustomerSelectionAsync` (23 copies under
 * `V.SMART/V.SMART.Shared/Pages/`, e.g. `EstimationUpsert.razor:1125`) and it
 * is extracted **server-side** by the relevant wave's `-03` step. It must
 * never appear in this control.
 *
 * **This is a new capability, not a reproduction.** The Blazor
 * `CustomerSelection.razor` is a routable page that eagerly loads the whole
 * active-customer list into memory (`:119-148`) and filters it synchronously
 * (`:150-160`, `Task.FromResult`, `CancellationToken` ignored). There is no
 * debounce and no server round-trip there. The async loader below is better,
 * but it is not "preserving existing behaviour" and should not be described
 * that way.
 *
 * The control performs no request itself: the caller supplies a
 * {@link ComboboxLoader}. It has never heard of `HttpClient`.
 */
@Component({
  selector: 'app-combobox',
  templateUrl: './combobox.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AutoComplete, FormsModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ComboboxComponent), multi: true },
  ],
})
export class ComboboxComponent<TValue = unknown> extends BaseFormControl<SelectOption<TValue>> {
  private readonly destroyRef = inject(DestroyRef);

  /** Caller-supplied async search. Rejecting is fine - it becomes the error row. */
  readonly loader = input.required<ComboboxLoader<TValue>>();
  /** Debounce in ms. Keystroke-per-request is what makes a typeahead feel slow *and* costly. */
  readonly debounceMs = input(300);
  /** Minimum query length before the loader is called at all. */
  readonly minQueryLength = input(1);

  readonly suggestions = signal<readonly SelectOption<TValue>[]>([]);
  /** PrimeNG mutates the array it is handed, so it gets a copy. */
  readonly suggestionList = computed(() => [...this.suggestions()]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly lastQuery = signal('');
  readonly searched = signal(false);

  private readonly autoComplete = viewChild.required(AutoComplete);

  private timer: ReturnType<typeof setTimeout> | null = null;
  /** Guards against an out-of-order response overwriting a newer one. */
  private requestSeq = 0;

  protected override controlElementSelector = 'input';

  constructor() {
    super();
    this.destroyRef.onDestroy(() => this.cancelPending());
    afterNextRender(() => this.guardStaleEnterConfirmation());
  }

  /**
   * Second PrimeNG defect this control contains, distinct from the one
   * {@link onPanelHide} handles, and found by the same R-76 flake.
   *
   * `p-overlay`'s enter transition begins **asynchronously**. When it begins,
   * the overlay confirms visibility back up through its `visible` model:
   * `onOverlayBeforeEnter()` -> `show()` -> `onVisibleChange(true)`
   * (`primeng-overlay.mjs:457-459,474-475,489-492`), which the AutoComplete
   * template echoes straight into its own state -
   * `(visibleChange)="overlayVisible.set($event)"`
   * (`primeng-autocomplete.mjs:1726`) - **without going through
   * `AutoComplete.show()`**. An Escape that lands in the window between the
   * panel opening and the enter transition starting is therefore overwritten:
   * `hide()` sets `overlayVisible` false, then the stale confirmation sets it
   * back to true, and the panel the user just dismissed reopens for good.
   *
   * Probed rather than assumed: under a loaded test run the panel stayed open
   * for a full second after Escape with `hide()` called once and `show()`
   * never - only this echo writes `overlayVisible` outside `show()`.
   *
   * The invariant enforced here: **a confirmation may confirm, never
   * resurrect.** The overlay's `show` is wrapped to no-op when the
   * AutoComplete no longer wants the panel visible. This reaches into two
   * PrimeNG internals (`overlayViewChild`, `Overlay.show`) - the same
   * accepted, upgrade-watched trade R-71 records - and R-77 tracks it against
   * PrimeNG upgrades.
   */
  private guardStaleEnterConfirmation(): void {
    const ac = this.autoComplete() as unknown as {
      overlayVisible: () => boolean;
      overlayViewChild?: () =>
        { show?: (overlay?: unknown, isFocus?: boolean) => void } | undefined;
    };
    const overlay = ac.overlayViewChild?.();
    const originalShow = overlay?.show?.bind(overlay);
    if (!overlay || !originalShow) {
      // A PrimeNG upgrade moved the seam. Fail open: the control still works,
      // the Escape race returns, and the spec that simulates the stale
      // confirmation fails - which is the alarm R-77 relies on.
      return;
    }
    overlay.show = (overlayEl?: unknown, isFocus?: boolean): void => {
      if (!ac.overlayVisible()) {
        return;
      }
      originalShow(overlayEl, isFocus);
    };
  }

  onComplete(event: AutoCompleteCompleteEvent): void {
    const query = event.query;
    this.lastQuery.set(query);
    this.cancelPending();
    if (query.length < this.minQueryLength()) {
      this.suggestions.set([]);
      this.searched.set(false);
      return;
    }
    this.timer = setTimeout(() => void this.runSearch(query), this.debounceMs());
  }

  /** Retry the last query after an error, from the error row inside the dropdown. */
  retry(): void {
    void this.runSearch(this.lastQuery());
  }

  onSelect(option: SelectOption<TValue>): void {
    this.emitValue(option);
    this.markTouched();
  }

  onClear(): void {
    this.emitValue(null);
    this.markTouched();
  }

  /**
   * The panel closed - Escape, an outside click, Tab, or a selection. A search
   * still pending or in flight exists only to populate that panel, so it is
   * discarded here rather than allowed to land.
   *
   * Without this, PrimeNG re-opens the panel the user just dismissed:
   * `search()` sets PrimeNG's own `loading` flag
   * (`primeng-autocomplete.mjs:1265`), `hide()` never clears it
   * (`:1361-1376`), and `handleSuggestionsChange()` answers any
   * suggestions change that arrives while that flag is set with `show()`
   * (`:772-778`). In production that is a slow search response forcing a
   * dismissed dropdown back open; under jsdom it was the R-76 combobox
   * flake, where the late response raced the test's Escape.
   */
  onPanelHide(): void {
    this.cancelPending();
    // An in-flight loader resolution is stale from this point on: runSearch's
    // seq check discards it, so it can never reach suggestions.set() - which
    // is the write that would make PrimeNG show() the panel again.
    this.requestSeq += 1;
    this.loading.set(false);
    // PrimeNG clears its own loading flag only in handleSuggestionsChange(),
    // which the discard above prevents from running. Left set, it hides the
    // clear icon ($showClear) and swallows container clicks until the next
    // keystroke. Reset it the way the delivered change would have. This
    // reaches into a PrimeNG internal - the same accepted trade R-71 records
    // for the overlay and feedback layers.
    this.autoComplete().loading.set(false);
  }

  private async runSearch(query: string): Promise<void> {
    const seq = ++this.requestSeq;
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const results = await this.loader()(query);
      if (seq !== this.requestSeq) {
        return;
      }
      // The previous list stays on screen until the new one arrives
      // (KB-050 Data-fetching conventions) - it is only replaced here.
      this.suggestions.set(results);
      this.searched.set(true);
    } catch (error) {
      if (seq !== this.requestSeq) {
        return;
      }
      this.errorMessage.set(error instanceof Error ? error.message : 'Search failed.');
    } finally {
      if (seq === this.requestSeq) {
        this.loading.set(false);
      }
    }
  }

  private cancelPending(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
