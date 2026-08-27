/**
 * M2-C02 — the auth/me wrapper layer `eslint.config.js`'s `bannedGeneratedClientImports` rule
 * requires: only files under `core/api/**` may import `core/api/generated/**` directly, so
 * this is the one seam `core/auth/` is allowed to depend on. Re-exports only — no logic — the
 * generated `AuthService`/`MeService` are already thin, typed wrappers over `HttpClient`; a
 * second layer of indirection beyond a barrel would be duplication, not architecture.
 */
export { AuthService as AuthApiService } from './generated/services/auth.service';
export { MeService as MeApiService } from './generated/services/me.service';
export type { LoginResponse } from './generated/models/login-response';
export type { RefreshResponse } from './generated/models/refresh-response';
export type { MeResponse } from './generated/models/me-response';
export type { ScreenRight as ApiScreenRight } from './generated/models/screen-right';
