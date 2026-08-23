import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  model,
  output,
  signal,
  untracked,
  viewChild,
  type ElementRef,
} from '@angular/core';
import { Drawer } from 'primeng/drawer';

import { focusFirstElementIn, OverlayFocusKeeper } from './overlay-focus';

/** Widths in px. A drawer narrower than this stops being a record detail. */
export const DRAWER_MIN_WIDTH = 320;
export const DRAWER_MAX_WIDTH = 960;
export const DRAWER_DEFAULT_WIDTH = 480;
/** One arrow press. Coarse enough to be quick, fine enough to be precise. */
export const DRAWER_RESIZE_STEP = 32;

const WIDTH_STORAGE_PREFIX = 'nexgen.drawer.width.';

function clampWidth(value: number): number {
  return Math.min(DRAWER_MAX_WIDTH, Math.max(DRAWER_MIN_WIDTH, Math.round(value)));
}

/**
 * A right-side drawer over `p-drawer`, for record detail **without losing the
 * list behind it** - the KB-051 rule a modal cannot satisfy. Reconciling a
 * record against the list it came from needs both on screen.
 *
 * Resizing is operable from the keyboard as well as by drag: a drag-only
 * handle is inoperable for anyone not using a mouse. The chosen width is
 * remembered locally, per `persistKey`.
 *
 * No ERP business rule lives here.
 */
@Component({
  selector: 'app-drawer',
  templateUrl: './drawer.component.html',
  styleUrl: './drawer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Drawer],
})
export class DrawerComponent {
  readonly title = input.required<string>();
  readonly visible = model(false);
  readonly closable = input(true);
  /**
   * Distinguishes one screen's drawer width from another's in local storage.
   * A single shared width would make every screen fight over it.
   */
  readonly persistKey = input('default');
  readonly resizable = input(true);
  readonly opened = output<void>();
  readonly closed = output<void>();

  private readonly body = viewChild<ElementRef<HTMLElement>>('body');
  private readonly focus = new OverlayFocusKeeper();
  private readonly chosenWidth = signal<number | null>(null);
  private readonly draggedWidth = signal<number | null>(null);
  private escapeTarget: HTMLElement | null = null;
  private wasOpen = false;

  readonly minWidth = DRAWER_MIN_WIDTH;
  readonly maxWidth = DRAWER_MAX_WIDTH;

  readonly width = computed(
    () =>
      this.draggedWidth() ??
      this.chosenWidth() ??
      this.readPersistedWidth() ??
      DRAWER_DEFAULT_WIDTH,
  );
  readonly widthStyle = computed(() => ({ width: `${String(this.width())}px` }));

  constructor() {
    // **Close is driven from `visible`, not from `(onHide)`.** Measured in
    // PrimeNG 22.1.0: `Drawer` emits `onHide` only from `close()` - the close
    // icon and the mask - while a programmatic `visible = false` reaches
    // `onAfterLeave`, which calls `hide(false)` and deliberately skips the
    // emit (`primeng-drawer.mjs`). Restoring focus from `(onHide)` would
    // therefore silently not happen on the commonest close of all: the screen
    // closing its own drawer after a save.
    effect(() => {
      const open = this.visible();
      untracked(() => {
        if (open) {
          this.focus.capture();
          this.wasOpen = true;
        } else if (this.wasOpen) {
          this.wasOpen = false;
          this.teardown();
        }
      });
    });
  }

  onShow(): void {
    const root = this.drawerRoot();
    if (root) {
      // `Esc` is ours, not PrimeNG's - see `closeOnEscape` in the template.
      root.addEventListener('keydown', this.escapeListener);
      this.escapeTarget = root;
      // PrimeNG hard-codes `role="complementary"` on the drawer root. A modal
      // record-detail overlay is a dialog: without these three attributes a
      // screen-reader user is not told they have entered one, nor what it is.
      // Set here rather than by forking the component, and asserted in the spec.
      root.setAttribute('role', 'dialog');
      root.setAttribute('aria-modal', 'true');
      root.setAttribute('aria-label', this.title());
    }
    focusFirstElementIn(root);
    this.opened.emit();
  }

  private teardown(): void {
    this.escapeTarget?.removeEventListener('keydown', this.escapeListener);
    this.escapeTarget = null;
    this.focus.restore();
    this.closed.emit();
  }

  /**
   * **Measured PrimeNG 22.1.0 defect, worked around rather than assumed away.**
   * `Drawer.onKeyDown` answers `Escape` with `hide(false)`, which tears down
   * the modality but never clears `visible` - so the drawer stays on screen;
   * and its document-level fallback listener keys off the deprecated
   * `event.which`, while `unbindDocumentEscapeListener()` calls *itself*
   * (`primeng-drawer.mjs`, PrimeNG 22.1.0). `[closeOnEscape]="false"` keeps
   * the component out of that path and this listener implements the contract
   * the acceptance criteria state. Recorded in KB-060.
   */
  private readonly escapeListener = (event: Event): void => {
    if (!(event instanceof KeyboardEvent) || event.key !== 'Escape' || !this.closable()) {
      return;
    }
    event.stopPropagation();
    this.visible.set(false);
  };

  /**
   * The drawer is on the right, so Left widens and Right narrows: the key
   * moves the edge, which is what the hand expects of the handle it is on.
   */
  onResizeKeydown(event: KeyboardEvent): void {
    const step = event.shiftKey ? DRAWER_RESIZE_STEP * 2 : DRAWER_RESIZE_STEP;
    let next: number | null = null;
    if (event.key === 'ArrowLeft') {
      next = this.width() + step;
    } else if (event.key === 'ArrowRight') {
      next = this.width() - step;
    } else if (event.key === 'Home') {
      next = DRAWER_MIN_WIDTH;
    } else if (event.key === 'End') {
      next = DRAWER_MAX_WIDTH;
    }
    if (next === null) {
      return;
    }
    event.preventDefault();
    this.setWidth(next);
  }

  onResizeStart(event: MouseEvent): void {
    event.preventDefault();
    const startX = event.clientX;
    const startWidth = this.width();
    const move = (moveEvent: MouseEvent): void => {
      this.draggedWidth.set(clampWidth(startWidth + (startX - moveEvent.clientX)));
    };
    const up = (): void => {
      window.removeEventListener('mousemove', move);
      window.removeEventListener('mouseup', up);
      const dragged = this.draggedWidth();
      this.draggedWidth.set(null);
      if (dragged !== null) {
        this.setWidth(dragged);
      }
    };
    window.addEventListener('mousemove', move);
    window.addEventListener('mouseup', up);
  }

  private setWidth(value: number): void {
    const clamped = clampWidth(value);
    this.chosenWidth.set(clamped);
    try {
      localStorage.setItem(this.storageKey(), String(clamped));
    } catch {
      // A drawer width is not worth failing over when storage is unavailable
      // (private mode, quota, a locked-down kiosk). It just does not persist.
    }
  }

  private readPersistedWidth(): number | null {
    try {
      const raw = localStorage.getItem(this.storageKey());
      if (raw === null) {
        return null;
      }
      const parsed = Number.parseInt(raw, 10);
      return Number.isFinite(parsed) ? clampWidth(parsed) : null;
    } catch {
      return null;
    }
  }

  private storageKey(): string {
    return `${WIDTH_STORAGE_PREFIX}${this.persistKey()}`;
  }

  private drawerRoot(): HTMLElement | null {
    const body = this.body()?.nativeElement;
    return body?.closest<HTMLElement>('.app-drawer') ?? body ?? null;
  }
}
