import { readFileSync, readdirSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { render, screen } from '@testing-library/angular';
import { Toast } from 'primeng/toast';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  provideToast,
  TOAST_POLITE_PASS_THROUGH,
  TOAST_SUCCESS_LIFE_MS,
  ToastService,
} from './index';

const TEMPLATE = `<p-toast [pt]="pt" />`;

async function setup() {
  const view = await render(TEMPLATE, {
    imports: [Toast],
    providers: [provideToast()],
    componentProperties: { pt: TOAST_POLITE_PASS_THROUGH },
  });
  const toast = view.fixture.debugElement.injector.get(ToastService);
  return { view, toast };
}

function flush(view: { fixture: { detectChanges: () => void } }): void {
  view.fixture.detectChanges();
}

describe('ToastService', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('auto-dismisses a success toast at 4 s', async () => {
    const { view, toast } = await setup();

    toast.success('Sales order saved');
    flush(view);
    expect(screen.getByText('Sales order saved')).toBeTruthy();

    await vi.advanceTimersByTimeAsync(TOAST_SUCCESS_LIFE_MS + 500);
    flush(view);

    expect(screen.queryByText('Sales order saved')).toBeNull();
  });

  it('keeps an error toast on screen indefinitely, with a dismiss control', async () => {
    const { view, toast } = await setup();

    // A 409's title, verbatim - the shape ApiProblems.cs:47-53 produces.
    toast.error('Cannot delete this Sales Order as a Sales DC transaction exists.');
    flush(view);

    await vi.advanceTimersByTimeAsync(60_000);
    flush(view);

    expect(
      screen.getByText('Cannot delete this Sales Order as a Sales DC transaction exists.'),
    ).toBeTruthy();
    expect(document.querySelector('.p-toast-close-button')).toBeTruthy();
  });

  it('announces politely rather than assertively', async () => {
    const { view, toast } = await setup();

    toast.info('Prices refreshed');
    flush(view);

    const item = document.querySelector('.p-toast-message');
    expect(item?.getAttribute('aria-live')).toBe('polite');
  });

  it('is the only file in src/** that touches PrimeNG’s message service', () => {
    // Assembled from parts so this assertion does not itself become a hit.
    const needle = 'Message' + 'Service';
    const src = resolve(process.cwd(), 'src');
    const offenders: string[] = [];

    const walk = (directory: string): void => {
      for (const entry of readdirSync(directory, { withFileTypes: true })) {
        const full = join(directory, entry.name);
        if (entry.isDirectory()) {
          walk(full);
        } else if (entry.name.endsWith('.ts') || entry.name.endsWith('.html')) {
          if (readFileSync(full, 'utf8').includes(needle)) {
            offenders.push(full.slice(src.length + 1).replaceAll('\\', '/'));
          }
        }
      }
    };
    walk(src);

    expect(offenders).toEqual(['app/shared/components/feedback/toast.service.ts']);
  });
});
