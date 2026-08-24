import { readFileSync, readdirSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { describe, expect, it } from 'vitest';

/**
 * **Stated jsdom limitation, not hidden.** jsdom applies no stylesheet and
 * computes no layout, and it cannot emulate a media feature: there is no way
 * to ask it what `transition-duration` resolves to under
 * `prefers-reduced-motion: reduce`. So this asserts the **stylesheet text** -
 * that every animated surface in these two directories carries a reduce block
 * that zeroes its durations - rather than a computed style.
 *
 * The behavioural check belongs in the Playwright suite, which drives a real
 * engine and can set the emulated media feature. That is not this task's to
 * add; the gap is recorded rather than papered over.
 */
const COMPONENTS = resolve(process.cwd(), 'src/app/shared/components');

/** Stylesheets whose components open, close, slide or shimmer. */
const ANIMATED = [
  'overlay/modal.component.scss',
  'overlay/drawer.component.scss',
  'overlay/confirm-dialog.component.scss',
  'overlay/popover.component.scss',
  'overlay/context-menu.component.scss',
  'feedback/busy-overlay.component.scss',
  'feedback/skeleton.component.scss',
  'feedback/progress-bar.component.scss',
];

function read(relativePath: string): string {
  return readFileSync(join(COMPONENTS, relativePath), 'utf8');
}

describe('prefers-reduced-motion', () => {
  it.each(ANIMATED)('%s zeroes its transitions under reduce', (stylesheet) => {
    const source = read(stylesheet);
    const reduceBlock = source.slice(source.indexOf('prefers-reduced-motion'));

    expect(source).toContain('@media (prefers-reduced-motion: reduce)');
    expect(reduceBlock).toMatch(/(transition-duration|animation-duration): 0ms|animation: none/);
  });

  it('covers every stylesheet in the two directories that animates anything', () => {
    const animating: string[] = [];
    for (const directory of ['overlay', 'feedback']) {
      for (const name of readdirSync(join(COMPONENTS, directory))) {
        if (!name.endsWith('.scss')) {
          continue;
        }
        const relativePath = `${directory}/${name}`;
        const source = read(relativePath);
        if (/transition|animation/.test(source)) {
          animating.push(relativePath);
        }
      }
    }

    // Fails the moment a new animated stylesheet is added without a reduce
    // block - which is the only way this stays true a year from now.
    expect(animating.sort()).toEqual([...ANIMATED].sort());
  });
});
