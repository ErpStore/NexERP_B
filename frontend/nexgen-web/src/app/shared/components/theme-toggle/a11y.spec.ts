import axe, { type Result } from 'axe-core';
import { render, screen } from '@testing-library/angular';
import { afterEach, describe, expect, it } from 'vitest';

import { ThemeToggleComponent } from './theme-toggle.component';

/**
 * Runtime accessibility scan. The `angular-eslint` template-a11y rules run in
 * addition to this, not instead of it: they read the template, axe reads the
 * rendered DOM.
 *
 * Scope note: this is per-component and local to the spec suite. The
 * repository-wide axe-in-CI pass is M5-09.
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet
 * and computes no layout, so axe's colour-contrast rule cannot run here. That
 * rule is covered by computation instead, in
 * `src/app/core/theme/contrast.spec.ts`.
 */
async function criticalViolations(): Promise<Result[]> {
  const results = await axe.run(screen.getByRole('radiogroup'), {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('app-theme-toggle accessibility', () => {
  afterEach(() => {
    document.documentElement.removeAttribute('data-theme');
    localStorage.clear();
  });

  it('has zero critical axe violations in the light theme', async () => {
    document.documentElement.setAttribute('data-theme', 'light');
    await render(ThemeToggleComponent);

    expect(await criticalViolations()).toEqual([]);
  });

  it('has zero critical axe violations in the dark theme', async () => {
    document.documentElement.setAttribute('data-theme', 'dark');
    await render(ThemeToggleComponent);

    expect(await criticalViolations()).toEqual([]);
  });
});
