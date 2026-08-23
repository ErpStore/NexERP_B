import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  forwardRef,
  inject,
  input,
  signal,
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

  private timer: ReturnType<typeof setTimeout> | null = null;
  /** Guards against an out-of-order response overwriting a newer one. */
  private requestSeq = 0;

  protected override controlElementSelector = 'input';

  constructor() {
    super();
    this.destroyRef.onDestroy(() => this.cancelPending());
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
