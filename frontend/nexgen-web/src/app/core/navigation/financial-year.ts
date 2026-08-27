/**
 * M2-C03 — the client-side financial-year label, derived from the **same rule**
 * `V.SMART/V.SMART.Shared/Services/FinancialYearHelper.cs#GetFinancialYearSuffix` uses
 * server-side (April-start FY), because no financial-year reference-data endpoint exists
 * (`M2-B09` ships no such route — confirmed by reading its controllers and the generated
 * OpenAPI document directly) and this task's own instruction is to derive client-side from
 * a documented rule rather than invent an endpoint.
 *
 * **What this is not.** The Blazor app has no FY *switcher* anywhere — `GetFinancialYearSuffix`
 * is called only at document-creation time, always with `DateTime.Now`, purely to compute a
 * numbering suffix; there is no persisted "which FY am I currently viewing" concept to port.
 * The header's FY selector is therefore genuinely new UI, not a port, and — disclosed rather
 * than silently pretended otherwise — selecting a past year here changes nothing yet: no
 * screen built so far reads the selection. `financial-year.service.ts` exists so a future
 * task has one root-provided signal to read once a screen needs to filter by it, rather than
 * each screen inventing its own.
 */
export interface FinancialYear {
  /** e.g. `"2026-27"`, matching `GetFinancialYearSuffix`'s `{year}-{nextYear[2:]}` shape,
   * minus the leading slash (that character is a route-suffix concern, not a label one). */
  readonly label: string;
  /** The FY's start year, e.g. `2026` for `"2026-27"`. */
  readonly startYear: number;
}

/** The FY containing `date`, by the same April-start rule as the server. */
export function financialYearFor(date: Date): FinancialYear {
  const startYear = date.getMonth() + 1 >= 4 ? date.getFullYear() : date.getFullYear() - 1;
  return financialYearFromStartYear(startYear);
}

export function financialYearFromStartYear(startYear: number): FinancialYear {
  const endYearSuffix = String(startYear + 1).slice(-2);
  return { label: `${startYear}-${endYearSuffix}`, startYear };
}

/** The current FY plus `count - 1` preceding ones, current first. */
export function recentFinancialYears(current: FinancialYear, count: number): FinancialYear[] {
  return Array.from({ length: count }, (_, i) =>
    financialYearFromStartYear(current.startYear - i),
  );
}
