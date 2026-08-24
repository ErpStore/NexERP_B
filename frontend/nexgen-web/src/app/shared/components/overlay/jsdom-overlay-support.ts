/**
 * **Test fixture only.**
 *
 * jsdom implements no `window.matchMedia`, and PrimeNG's overlay layer reads
 * it to decide whether a panel is modal. Anything that opens - dialog,
 * drawer, popover, context menu - needs it installed first.
 *
 * `form/jsdom-overlay-support.ts` (M2-C04-02) says the moment a second area
 * needs this is the moment to promote it to `angular.json`'s `setupFiles`.
 * That is a build-configuration change, `form/**` is outside this task's
 * scope to edit, and cross-importing a fixture between two task-owned
 * directories couples them; so it is duplicated here and the decision is
 * recorded in `docs/kb/risks/technical-debt-register.md` instead of taken
 * unilaterally.
 */
export function installMatchMedia(): void {
  if (typeof window.matchMedia === 'function') {
    return;
  }
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: (query: string): MediaQueryList =>
      ({
        matches: false,
        media: query,
        onchange: null,
        addListener: () => undefined,
        removeListener: () => undefined,
        addEventListener: () => undefined,
        removeEventListener: () => undefined,
        dispatchEvent: () => false,
      }) as MediaQueryList,
  });
}
