/**
 * **Test fixture only.**
 *
 * jsdom implements no `window.matchMedia`, and PrimeNG's overlay reads it to
 * decide whether a panel is modal. Every control with a dropdown - select,
 * multi-select, combobox, date picker - therefore needs it installed before
 * the panel opens.
 *
 * It lives in a fixture rather than in `angular.json`'s `setupFiles` so that
 * this task changes no build configuration; if a second area needs it, that
 * is the moment to promote it to a global setup file.
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
