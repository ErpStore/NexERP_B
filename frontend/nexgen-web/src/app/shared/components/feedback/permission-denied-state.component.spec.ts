import { readFileSync, readdirSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { PermissionDeniedStateComponent } from './permission-denied-state.component';

describe('app-permission-denied-state', () => {
  it('names the missing right and the screen it belongs to', async () => {
    await render(PermissionDeniedStateComponent, {
      inputs: { screen: 'Sales Order', right: 'edit' },
    });

    const panel = screen.getByRole('status');
    expect(panel.textContent).toContain('edit');
    expect(panel.textContent).toContain('Sales Order');
  });

  it('offers no retry - retrying does not grant a right', async () => {
    await render(PermissionDeniedStateComponent, {
      inputs: { screen: 'Sales Order', right: 'edit' },
    });

    expect(screen.queryByRole('button')).toBeNull();
    expect(screen.queryByRole('link')).toBeNull();
  });

  it('imports nothing from the auth layer - it renders a denial, it does not evaluate one', () => {
    // Assembled from parts so this file is not itself a hit.
    const needle = 'core/' + 'auth';
    const directory = resolve(process.cwd(), 'src/app/shared/components');
    const offenders: string[] = [];

    const walk = (path: string): void => {
      for (const entry of readdirSync(path, { withFileTypes: true })) {
        const full = join(path, entry.name);
        if (entry.isDirectory()) {
          walk(full);
        } else if (entry.name.endsWith('.ts') || entry.name.endsWith('.html')) {
          if (readFileSync(full, 'utf8').includes(needle)) {
            offenders.push(full);
          }
        }
      }
    };
    walk(directory);

    expect(offenders).toEqual([]);
  });
});
