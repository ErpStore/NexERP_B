import { Injectable, signal } from '@angular/core';

export type SidebarMode = 'expanded' | 'rail';

const STORAGE_KEY = 'nexgen.sidebar.mode';

/**
 * M2-C03 — the caller's own choice between the 240 px expanded sidebar and the 56 px rail,
 * at the one breakpoint (`lg`, ≥1440) where there is room to offer the choice at all. Below
 * `lg` the band itself decides (`BreakpointService`) and this preference is not consulted —
 * see `sidebar.component.ts`'s `effectiveMode`.
 *
 * Root-provided because the toggle button lives in the header, not the sidebar (KB-051's
 * shell diagram puts `[☰]` at the header's left edge).
 */
@Injectable({ providedIn: 'root' })
export class SidebarModeService {
  private readonly modeSignal = signal<SidebarMode>(this.readStored());

  readonly mode = this.modeSignal.asReadonly();

  toggle(): void {
    const next: SidebarMode = this.modeSignal() === 'expanded' ? 'rail' : 'expanded';
    this.modeSignal.set(next);
    this.write(next);
  }

  private readStored(): SidebarMode {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw === 'rail' ? 'rail' : 'expanded';
    } catch {
      return 'expanded';
    }
  }

  private write(mode: SidebarMode): void {
    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      // A hostile or full localStorage loses persistence across reloads only — the signal
      // above is still correct for the rest of this session.
    }
  }
}
