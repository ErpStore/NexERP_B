/**
 * M2-D01 — re-exports only. `ADR-002 §6` and `ADR-007` make the generated OpenAPI client
 * the one sanctioned source of API types; this file never declares one. It re-exports from
 * `core/api/currency-api.ts` rather than `core/api/generated/**` directly, because
 * `eslint.config.js`'s `bannedGeneratedClientImports` rule confines the generated client's
 * import surface to `core/api/**` — this feature folder is not on that allow-list.
 */
export type { CurrencyVM } from '@/app/core/api/currency-api';
