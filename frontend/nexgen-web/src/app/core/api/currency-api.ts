/**
 * M2-D01 — the Currency wrapper layer `eslint.config.js`'s `bannedGeneratedClientImports`
 * rule requires: only files under `core/api/**` may import `core/api/generated/**`
 * directly, so this is the one seam `features/masters/currency/` is allowed to depend on.
 * Re-exports only — no logic — the generated `CurrencyService` is already a thin, typed
 * wrapper over `HttpClient`; a second layer of indirection beyond a barrel would be
 * duplication, not architecture. Mirrors `core/api/auth-api.ts`'s own pattern (M2-C02).
 *
 * `CurrencyApiService` also carries `exportCurrencies$Response` (`CurrencyExcelController`,
 * a different controller than `CurrencyController`, but the same OpenAPI tag — see that
 * controller's own doc comment) — one class either way, so the feature never has to know
 * the split exists.
 */
export { CurrencyService as CurrencyApiService } from './generated/services/currency.service';
export type { CurrencyVM } from './generated/models/currency-vm';
export type { CurrencyVMPagedResult } from './generated/models/currency-vm-paged-result';
export type { GetCurrencies$Params } from './generated/fn/currency/get-currencies';
export type { ExportCurrencies$Params } from './generated/fn/currency/export-currencies';
