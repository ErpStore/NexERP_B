import { inject, Injectable, signal } from '@angular/core';
import type { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { CurrencyApiService, type CurrencyVM } from '@/app/core/api/currency-api';
import type {
  DataGridPage,
  DataGridWireQuery,
} from '@/app/shared/components/data-grid/data-grid-query.adapter';
import type { GridExportOperation } from '@/app/shared/components/data-grid/grid-export.service';

/** A wire-query filter value, or `undefined` when the filter was not committed. */
function asString(value: string | number | undefined): string | undefined {
  return value === undefined ? undefined : String(value);
}

/**
 * M2-D01 — the Currency feature's one seam to the generated client.
 *
 * **Deliberately does not hold `list`/`loading` signals for the grid.** KB-050's general
 * data-fetching shape — a feature service holding `list`/`loading` signals with an explicit
 * `refresh()` — predates M2-C05-01's `DataGridQueryState`, which now owns exactly that: rows,
 * totalCount, loading, refetching, error, and its own `refresh()`, wired to the URL. Duplicating
 * that here would be the second grid-state mechanism this task's own Implementation Requirement 6
 * forbids ("`DataGridQueryState`... is the mechanism; do not invent a second one"). Recorded as a
 * real, disclosed tension in the Slice review — not a silent deviation from either document.
 *
 * What this service *does* own: the mutation lifecycle (`saving`). `DataGridQueryState` has no
 * notion of "a POST/PUT/DELETE is in flight" — that belongs to whichever surface (the drawer
 * form, the delete confirmation) triggered it, and both share this one flag rather than each
 * inventing its own.
 *
 * Named `CurrencyFeatureService`, not `CurrencyService` — the generated client already exports a
 * class of that name (`core/api/generated/services/currency.service.ts`), and a second one in the
 * same import graph would be an unreadable collision. Noted in the task file's own ⛔ banner
 * before this file existed.
 */
@Injectable({ providedIn: 'root' })
export class CurrencyFeatureService {
  private readonly api = inject(CurrencyApiService);

  private readonly savingState = signal(false);
  /** True while a create/update/delete is in flight. Bind a busy overlay's `visible` to it. */
  readonly saving = this.savingState.asReadonly();

  /**
   * `DataGridQueryState`'s `source`. `DataGridWireQuery`'s keys already match
   * `GetCurrencies$Params`'s wire names exactly (`data-grid-query.adapter.ts`'s own doc comment
   * confirms this against `get-currencies.ts:14-55`) — the explicit fields below exist for the
   * numeric/string typing `Record<string, string | number>` cannot carry, not because the names
   * disagree.
   */
  list(query: DataGridWireQuery): Observable<DataGridPage<CurrencyVM>> {
    return this.api.getCurrencies({
      pageNumber: Number(query['pageNumber']),
      pageSize: Number(query['pageSize']),
      sort: asString(query['sort']),
      currName: asString(query['currName']),
      createdBy: asString(query['createdBy']),
      fromDate: asString(query['fromDate']),
      toDate: asString(query['toDate']),
    });
  }

  getById(id: number): Observable<CurrencyVM> {
    return this.api.getCurrencyById({ id });
  }

  create(vm: CurrencyVM): Observable<CurrencyVM> {
    this.savingState.set(true);
    return this.api.createCurrency({ body: vm }).pipe(finalize(() => this.savingState.set(false)));
  }

  update(id: number, vm: CurrencyVM): Observable<CurrencyVM> {
    this.savingState.set(true);
    return this.api
      .updateCurrency({ id, body: vm })
      .pipe(finalize(() => this.savingState.set(false)));
  }

  remove(id: number): Observable<void> {
    this.savingState.set(true);
    return this.api.deleteCurrency({ id }).pipe(finalize(() => this.savingState.set(false)));
  }

  /**
   * `GridExportService`/`app-data-grid-toolbar`'s `exportOperation`. Returns the full
   * `HttpResponse`, not the body — the export filename lives in `Content-Disposition`, which
   * `exportCurrencies()` (body-only) would discard.
   */
  readonly exportOperation: GridExportOperation = (query, format) =>
    this.api.exportCurrencies$Response({
      pageNumber: Number(query['pageNumber']),
      pageSize: Number(query['pageSize']),
      sort: asString(query['sort']),
      currName: asString(query['currName']),
      createdBy: asString(query['createdBy']),
      fromDate: asString(query['fromDate']),
      toDate: asString(query['toDate']),
      format,
    });
}
