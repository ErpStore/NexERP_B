import { afterEach } from 'vitest';

/**
 * Global unit-test setup. Wired through `angular.json`'s `setupFiles`, so it
 * runs for every spec file in the workspace.
 *
 * **Why this exists — R-76.** `@angular/build:unit-test`'s `isolate` option
 * *"Defaults to false to align with the Karma/Jasmine experience"* (the
 * builder's own schema), so every spec file in a worker shares one jsdom
 * document. That is fine for anything a component renders inside its own
 * host — `@testing-library/angular`'s automatic cleanup unmounts it — but
 * PrimeNG's overlay components (`BlockUI`, `Toast`, `ConfirmDialog`, `Dialog`,
 * `Drawer`, connected overlays) attach their host to `document.body`, outside
 * that host. Nothing unmounts those, so they survive into the next spec file
 * and start answering its global role queries.
 *
 * The symptom is always the same and never points at the file that caused it:
 * *"Found multiple elements with the role …"*. Measured on `master` at
 * `e9a8e7a` before this fix, five consecutive `npm run test:ci` runs gave two
 * clean and three red — `feedback/permission-denied-state.component.spec.ts`
 * losing to a leaked `role="status"`, `feedback/progress-bar.component.spec.ts`
 * to a leaked `role="progressbar"`, both orphaned by
 * `feedback/busy-overlay.component.spec.ts`. Which files collide depends on how
 * vitest partitions the file list, so **adding any spec file anywhere changes
 * who loses**, and running the three files together in isolation passes — which
 * is why it stayed invisible.
 *
 * `isolate: true` would also fix it, by giving every file its own environment.
 * This is the cheaper half of that trade: the leak is swept after each test and
 * the files keep sharing a worker.
 *
 * `afterEach` is imported rather than taken from the globals so this file
 * typechecks under `tsconfig.app.json`, which includes `src/**` with
 * `"types": []`.
 */

/**
 * A direct child of `<body>` that belongs to a component library overlay, not
 * to the test's own root. `@testing-library/angular` mounts into `div#root<n>`
 * and removes it itself, so nothing here can reach a container still in use.
 *
 * PrimeNG 22 marks every component host with `data-pc-name`, and every masked
 * overlay additionally carries `p-overlay-mask`. Matching the attribute rather
 * than a list of class names means a component added later is covered without
 * anyone remembering to extend this.
 */
const ORPHANED_OVERLAY = '[data-pc-name], .p-overlay-mask, .p-connected-overlay';

afterEach(() => {
  for (const child of Array.from(document.body.children)) {
    if (child.matches(ORPHANED_OVERLAY)) {
      child.remove();
    }
  }
});
