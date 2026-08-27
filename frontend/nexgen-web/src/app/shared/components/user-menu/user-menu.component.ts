import { ChangeDetectionStrategy, Component, input, output, viewChild } from '@angular/core';

import { PopoverComponent } from '../overlay/popover.component';

/**
 * M2-C03 — the header's user menu: a menu button (`aria-haspopup`) opening a small
 * `role="menu"` with Logout. Purely presentational — the caller supplies `userName` and
 * listens for `logoutRequested`; nothing here calls `AuthService` directly, matching every
 * other `shared/components/` primitive (`permission-denied-state.component.spec.ts`'s
 * repo-wide scan bans importing from the authentication module here). `layout/shell/`
 * owns the real session and reacts to the event.
 */
@Component({
  selector: 'app-user-menu',
  imports: [PopoverComponent],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMenuComponent {
  readonly userName = input.required<string>();

  readonly logoutRequested = output<void>();

  private readonly popover = viewChild.required<PopoverComponent>('menu');

  protected toggle(event: Event): void {
    this.popover().toggle(event);
  }

  protected onLogoutClick(): void {
    this.popover().hide();
    this.logoutRequested.emit();
  }
}
