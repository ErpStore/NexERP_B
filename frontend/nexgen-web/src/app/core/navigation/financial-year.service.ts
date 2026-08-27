import { Injectable, signal } from '@angular/core';

import { financialYearFor, type FinancialYear } from './financial-year';

/**
 * M2-C03 — the caller's selected financial year. Root-provided, presentation state only
 * (see `financial-year.ts`'s file header for what this is and is not). Defaults to the
 * real current FY on construction and is never persisted — a stale FY choice surviving a
 * reload would be a worse default than just recomputing "now" fresh each time.
 */
@Injectable({ providedIn: 'root' })
export class FinancialYearService {
  private readonly selectedSignal = signal<FinancialYear>(financialYearFor(new Date()));

  readonly selected = this.selectedSignal.asReadonly();

  select(year: FinancialYear): void {
    this.selectedSignal.set(year);
  }
}
