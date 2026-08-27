import { TestBed } from '@angular/core/testing';
import { provideRouter, type Routes } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { describe, expect, it } from 'vitest';

import { BreadcrumbsComponent } from './breadcrumbs.component';

const ROUTES: Routes = [
  {
    path: 'masters',
    data: { breadcrumb: 'Masters' },
    children: [
      {
        path: 'currencies',
        data: { breadcrumb: 'Currency Master' },
        component: BreadcrumbsComponent,
      },
    ],
  },
  {
    path: 'no-crumb',
    // No `breadcrumb` key at all — a segment that opts out contributes nothing.
    component: BreadcrumbsComponent,
  },
];

describe('app-breadcrumbs', () => {
  it('derives the trail from route data, root to leaf, and links every crumb but the last', async () => {
    TestBed.configureTestingModule({ providers: [provideRouter(ROUTES)] });
    const harness = await RouterTestingHarness.create();
    const root = await harness.navigateByUrl('/masters/currencies');
    void root;

    const list = harness.routeNativeElement?.querySelectorAll('li') ?? [];
    expect(list).toHaveLength(2);

    const mastersLink = harness.routeNativeElement?.querySelector('a');
    expect(mastersLink?.textContent?.trim()).toBe('Masters');
    expect(mastersLink?.getAttribute('href')).toBe('/masters');

    const current = harness.routeNativeElement?.querySelector('[aria-current="page"]');
    expect(current?.textContent?.trim()).toBe('Currency Master');
    expect(current?.tagName).not.toBe('A');
  });

  it('a route segment with no breadcrumb data contributes no crumb', async () => {
    TestBed.configureTestingModule({ providers: [provideRouter(ROUTES)] });
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/no-crumb');

    expect(harness.routeNativeElement?.querySelectorAll('li')).toHaveLength(0);
  });
});
