import { readFileSync, readdirSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { SkeletonFormComponent } from './skeleton-form.component';
import { SkeletonTableComponent } from './skeleton-table.component';
import { SkeletonComponent } from './skeleton.component';

describe('app-skeleton', () => {
  it('is decorative: the bars are hidden from the accessibility tree', async () => {
    const view = await render(SkeletonComponent, { inputs: { width: '8rem' } });

    expect(view.container.querySelector('[aria-hidden="true"]')).toBeTruthy();
  });
});

describe('app-skeleton-table', () => {
  it('renders a shape matching the supplied dimensions', async () => {
    const view = await render(SkeletonTableComponent, {
      inputs: { rows: 4, columns: 3, label: 'Loading sales orders' },
    });

    const rows = view.container.querySelectorAll('.app-skeleton-table__row');
    // Four body rows plus the header strip.
    expect(rows.length).toBe(5);
    expect(view.container.querySelectorAll('app-skeleton').length).toBe(5 * 3);
  });

  it('announces the load once, politely, rather than once per bar', async () => {
    await render(SkeletonTableComponent, {
      inputs: { rows: 3, columns: 2, label: 'Loading sales orders' },
    });

    const region = screen.getByRole('status', { name: 'Loading sales orders' });
    expect(region.getAttribute('aria-live')).toBe('polite');
  });
});

describe('app-skeleton-form', () => {
  it('renders one label-and-control pair per field', async () => {
    const view = await render(SkeletonFormComponent, {
      inputs: { fields: 5, columns: 3 },
    });

    expect(view.container.querySelectorAll('.app-skeleton-form__field').length).toBe(5);
    expect(view.container.querySelectorAll('app-skeleton').length).toBe(10);
    expect(view.container.querySelector('[data-columns="3"]')).toBeTruthy();
  });
});

describe('the loading vocabulary', () => {
  it('uses no spinner anywhere in the skeleton presets - that is the whole point', () => {
    const directory = resolve(process.cwd(), 'src/app/shared/components/feedback');
    const spinnerUsers = readdirSync(directory)
      .filter((name) => name.startsWith('skeleton') && name.endsWith('.html'))
      .filter((name) => readFileSync(join(directory, name), 'utf8').includes('spinner'));

    expect(spinnerUsers).toEqual([]);
  });
});
