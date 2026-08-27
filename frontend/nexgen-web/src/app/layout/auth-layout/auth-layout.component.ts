import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * M2-C03 — the layout route for `/login` (and, if `M2-A05`/a later task ever adds one, a QR
 * variant). No chrome to add: `LoginComponent` (`M2-C02`) already centres its own card and
 * fills the viewport (`login.component.css`'s `.app-login { min-height: 100vh; }`), which
 * nests safely under this layout without any change there — this component exists so
 * `app.routes.ts` has a named layout route matching KB-050's structure, not because the
 * login screen needs chrome around it.
 */
@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  template: `<router-outlet />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthLayoutComponent {}
