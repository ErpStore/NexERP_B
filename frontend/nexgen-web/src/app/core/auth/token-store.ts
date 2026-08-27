import { Injectable } from '@angular/core';

/**
 * M2-C02 — the token-custody decision, in one narrow place.
 *
 * **Decision: both the access token and the refresh token live here, in memory only.**
 * Never `localStorage`, never `sessionStorage`, never a public signal. Real `#`-private
 * fields, not merely TypeScript `private` — so an accidental `JSON.stringify(tokenStore)` in
 * an error report or a devtools object inspector shows no own enumerable properties at all,
 * not just "hidden from the type checker."
 *
 * **Why not the task's recommended default (access token in memory, refresh token in an
 * httpOnly cookie).** That model needs the server to `Set-Cookie` the refresh token, and it
 * does not: `AuthController.RefreshResponse`/`LoginResponse` return the refresh token as a
 * plain string in the JSON body (`V.SMART/V.SMART.Api/Controllers/AuthController.cs`),
 * deliberately — M2-A04 chose body transport specifically because Q-16 (deployment
 * topology / TLS / cookie domain) is Unknown, recorded in `docs/kb/open-questions.md` Q-16
 * and `docs/kb/investigation-registry.md` INV-063. Making the server set a cookie is a
 * `V.SMART/` change this task's own Files That Must Not Change list forbids. In-memory-only
 * for *both* tokens is the strictest alternative actually available today: it keeps the
 * acceptance criterion ("zero writes to localStorage/sessionStorage across a full
 * login → refresh → logout cycle") true exactly as written, and it is strictly safer against
 * persistent token theft via XSS than any Web Storage option, since nothing here survives
 * past the JS heap.
 *
 * **The real, disclosed cost.** A hard page reload (F5, browser restart) loses this state
 * entirely — there is nothing to rehydrate from — so the bootstrap (`AuthService`) finds no
 * session and the user must log in again. This is a genuine UX regression versus a cookie
 * model, accepted because the alternative is a `V.SMART/` change out of this task's scope,
 * or a Web Storage write this task's own acceptance criteria treat as the thing to avoid.
 * Revisit once Q-16 is answered and M2-A04 (or a follow-up) can issue an httpOnly cookie.
 *
 * **M2-A05 adds `#tenant`, in the same custody tier as the tokens, for a different reason.**
 * `AuthController.LoginRequest`/`RefreshRequest`/`LogoutRequest` all gained a required
 * `tenant` field (ADR-002 §5) — the client must resend the same identifier it logged in with
 * on every refresh and on logout, or the server cannot even resolve which tenant database to
 * look the token up in (see `AuthController.cs`'s own constructor comment). A tenant
 * identifier is not a secret the way the tokens are, but it lives here anyway: it is part of
 * the same session, set and cleared in the same two places, and a hard reload losing it is
 * the correct behaviour — the session is already gone at that point regardless (`bootstrap()`
 * finds no refresh token and never reaches the point where it would need a tenant either).
 */
@Injectable({ providedIn: 'root' })
export class TokenStore {
  #accessToken: string | null = null;
  #refreshToken: string | null = null;
  #accessTokenExpiresAtUtc: Date | null = null;
  #tenant: string | null = null;

  setSession(accessToken: string, refreshToken: string, expiresAtUtc: Date, tenant: string): void {
    this.#accessToken = accessToken;
    this.#refreshToken = refreshToken;
    this.#accessTokenExpiresAtUtc = expiresAtUtc;
    this.#tenant = tenant;
  }

  /** Called after a rotation: both tokens change together, always. Tenant does not change on
   * a rotation — {@link setSession}'s original value stands — so this does not touch it. */
  rotate(accessToken: string, refreshToken: string, expiresAtUtc: Date): void {
    this.#accessToken = accessToken;
    this.#refreshToken = refreshToken;
    this.#accessTokenExpiresAtUtc = expiresAtUtc;
  }

  clear(): void {
    this.#accessToken = null;
    this.#refreshToken = null;
    this.#accessTokenExpiresAtUtc = null;
    this.#tenant = null;
  }

  get accessToken(): string | null {
    return this.#accessToken;
  }

  get refreshToken(): string | null {
    return this.#refreshToken;
  }

  get accessTokenExpiresAtUtc(): Date | null {
    return this.#accessTokenExpiresAtUtc;
  }

  get tenant(): string | null {
    return this.#tenant;
  }

  hasSession(): boolean {
    return this.#accessToken !== null && this.#refreshToken !== null;
  }
}
