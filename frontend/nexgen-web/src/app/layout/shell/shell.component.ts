import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Drawer } from 'primeng/drawer';

import { AuthService } from '../../core/auth/auth.service';
import { PermissionService } from '../../core/auth/permission.service';
import { BreakpointService } from '../../core/theme/breakpoint.service';
import { SidebarModeService } from '../../core/navigation/sidebar-mode.service';
import { CommandPaletteComponent } from '../../shared/components/command-palette/command-palette.component';
import { SkeletonComponent } from '../../shared/components/feedback/skeleton.component';
import { HeaderComponent } from '../../shared/components/header/header.component';
import type { NavLink } from '../../core/navigation/navigation.models';
import {
  SidebarComponent,
  type SidebarVisualMode,
} from '../../shared/components/sidebar/sidebar.component';

/**
 * M2-C03 — the authenticated app frame: header, sidebar, `<router-outlet>` and the command
 * palette. The one place under `layout/` (not `shared/components/`) that wires
 * `AuthService`/`PermissionService` to the presentational primitives below it.
 */
@Component({
  selector: 'app-shell',
  imports: [
    HeaderComponent,
    SidebarComponent,
    RouterOutlet,
    CommandPaletteComponent,
    Drawer,
    SkeletonComponent,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);
  private readonly breakpoints = inject(BreakpointService);
  private readonly sidebarModeService = inject(SidebarModeService);
  private readonly router = inject(Router);

  protected readonly userName = computed(() => this.auth.user()?.userName ?? '');
  /**
   * `/me` deliberately carries only `tenantId`, never a display name — `MeController.cs`'s
   * own doc comment: a name would mean reading `TenantInfo`, which also carries a plaintext
   * connection string (R-01). Until a task adds a name-only tenant lookup, this is the
   * honest placeholder, not a real name — disclosed, not silently invented.
   */
  protected readonly tenantName = computed(() => {
    const tenantId = this.auth.user()?.tenantId;
    return tenantId ? `Tenant ${tenantId}` : null;
  });

  /** Never a spinner, never a flash of an empty sidebar (Target Result) — true only in the
   * narrow, currently-unreachable window between the shell mounting and
   * `PermissionService.setRights()` running; kept because `authGuard` awaiting
   * `whenBootstrapped()` should make this state impossible in practice, but "impossible
   * today" is not the same guarantee as "structurally impossible", and a skeleton costs
   * nothing on the one render where it might still be needed. */
  protected readonly bootstrapping = computed(() => !this.permissions.hasBootstrapped());

  protected readonly sidebarMode = computed<SidebarVisualMode>(() => {
    if (this.breakpoints.isFullSidebar()) {
      return this.sidebarModeService.mode();
    }
    if (this.breakpoints.isRailSidebar()) {
      return 'rail';
    }
    return 'overlay';
  });

  protected readonly isOverlaySidebar = computed(() => this.sidebarMode() === 'overlay');

  protected readonly mobileNavOpen = signal(false);
  protected readonly paletteVisible = signal(false);

  constructor() {
    const destroyRef = inject(DestroyRef);
    const onKeydown = (event: KeyboardEvent): void => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault();
        this.paletteVisible.set(true);
      }
    };
    document.addEventListener('keydown', onKeydown);
    destroyRef.onDestroy(() => document.removeEventListener('keydown', onKeydown));

    // A navigation from inside the overlay drawer should close it — staying open over the
    // new page would just be a mis-sized sidebar sitting on top of the content it linked to.
    const routeSub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.mobileNavOpen.set(false);
      }
    });
    destroyRef.onDestroy(() => routeSub.unsubscribe());
  }

  protected onMenuToggled(): void {
    if (this.breakpoints.isFullSidebar()) {
      this.sidebarModeService.toggle();
    } else {
      this.mobileNavOpen.update((open) => !open);
    }
  }

  protected onPaletteNavigated(link: NavLink): void {
    void this.router.navigateByUrl(link.route);
  }

  protected async onLogout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigate(['/login']);
  }
}
