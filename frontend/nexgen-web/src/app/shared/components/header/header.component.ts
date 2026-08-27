import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';

import { BreakpointService } from '../../../core/theme/breakpoint.service';
import { FinancialYearSelectorComponent } from '../financial-year-selector/financial-year-selector.component';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { UserMenuComponent } from '../user-menu/user-menu.component';

/**
 * M2-C03 — the 48 px application header. Purely presentational, same discipline as every
 * other `shared/components/` primitive: identity/tenant come in as inputs, every action the
 * caller cares about goes out as an output. `layout/shell/` is the one place that wires
 * `AuthService`/`PermissionService` to these. `BreakpointService` (`core/theme/`, not the
 * authentication module) is read directly, same as `app-theme-toggle` reads `ThemeService`
 * directly — device/viewport state, not a `shared/components/` boundary concern.
 *
 * **Below 1024 px, the FY selector and tenant name move into the user menu rather than
 * disappearing** (KB-051 §Responsive behaviour: "FY is not optional information").
 */
@Component({
  selector: 'app-header',
  imports: [ThemeToggleComponent, UserMenuComponent, FinancialYearSelectorComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  private readonly breakpoints = inject(BreakpointService);

  readonly userName = input.required<string>();
  readonly tenantName = input<string | null>(null);

  readonly menuToggled = output<void>();
  readonly searchRequested = output<void>();
  readonly logoutRequested = output<void>();

  /** `true` at the `sm`/`xs` bands (<1024 px) — where the FY selector and tenant name move
   * into the user menu instead of the main header row. */
  protected readonly compactIdentity = computed(
    () => !this.breakpoints.isFullSidebar() && !this.breakpoints.isRailSidebar(),
  );
}
