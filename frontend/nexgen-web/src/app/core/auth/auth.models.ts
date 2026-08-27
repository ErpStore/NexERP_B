/**
 * M2-C02 — the auth/permission domain vocabulary. No HTTP, no signals, no framework
 * dependency: this file is the shape every other file in `core/auth/` and `core/http/`
 * agrees on.
 */

/** The four operations `RightsHelper.cs`/`ScreenRight` (M2-A07) express per screen. */
export type Right = 'view' | 'create' | 'edit' | 'delete';

/**
 * One screen's rights, exactly as `GET /api/v1/me` returns them
 * (`V.SMART/V.SMART.Api/Controllers/MeController.cs`'s `ScreenRight` record).
 *
 * `hidden` carries `IsHide`. It is a **navigation-listing** hint only — `view && !hidden`
 * decides whether a screen appears in navigation (M2-C03's job, not this one's). It is
 * **not a second access gate**: a screen with `view: true, hidden: true` is still a fully
 * reachable, fully rendered route. Confirmed from source, not assumed — see
 * `docs/kb/investigation-registry.md` INV-004's M2-C02 amendment: `IsHide` has no
 * enforcement effect anywhere in the current Blazor UI (`BaseUserRightsComponent.cs`'s
 * `IsHidden` property is read by exactly one representative page and its `Hidden` branch is
 * dead code; `NavMenu.razor` does not filter by rights at all today), and the server's own
 * `MeController.cs` doc comment states the intended contract this client implements:
 * *"navigation is filtered on view && !hidden ... it is not, and must not become, a second
 * gate."*
 */
export interface ScreenRight {
  readonly view: boolean;
  readonly create: boolean;
  readonly edit: boolean;
  readonly delete: boolean;
  readonly hidden: boolean;
}

/** Every field `false` — the deny-by-default value for a screen with no `UserRight` row. */
export const DENIED_SCREEN_RIGHT: ScreenRight = Object.freeze({
  view: false,
  create: false,
  edit: false,
  delete: false,
  hidden: false,
});

/**
 * Keyed by `Screens.ScreenName` verbatim — ordinal, case-sensitive (BR-AUTH-002). **A screen
 * the caller holds no row for has no key at all** — the map is never padded to every seeded
 * screen. Looking up a missing key must yield {@link DENIED_SCREEN_RIGHT}, never `undefined`
 * treated as "allow".
 */
export type ScreenRights = Readonly<Record<string, ScreenRight>>;

/** The bootstrapped identity `GET /api/v1/me` returns, normalised from the wire shape. */
export interface UserIdentity {
  readonly userId: number;
  readonly userName: string;
  readonly tenantId: number;
  readonly role: string;
  readonly rights: ScreenRights;
}

/**
 * The session lifecycle. `'unknown'` is the bootstrap-in-flight state — nothing
 * route-dependent may render while in it, or a hard refresh flashes the login page at an
 * already-authenticated user (Target Result, Acceptance Criteria).
 */
export type AuthStatus = 'unknown' | 'anonymous' | 'authenticating' | 'authenticated';

/**
 * Every distinguishable login failure the login screen must render differently. Deliberately
 * does **not** distinguish "unknown username" from "wrong password" — the server's own 401
 * body doesn't either (`AuthController.cs`, `UnauthenticatedProblem`).
 */
export type LoginFailureReason =
  | 'invalid-credentials'
  | 'tenant-unresolved'
  | 'trial-expired'
  | 'account-gate'
  | 'network'
  | 'unknown';

export interface LoginFailure {
  readonly reason: LoginFailureReason;
  /** The server's verbatim message, when it has one worth showing (e.g. the trial-expired
   * title). Never set for `'invalid-credentials'` — the whole point is not to leak which
   * field was wrong. */
  readonly message?: string;
}
