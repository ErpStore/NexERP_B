import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { NavItemComponent } from './nav-item.component';

const LINK = { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' };

describe('app-nav-item', () => {
  it('renders the label and links to the route', async () => {
    await render(NavItemComponent, {
      inputs: { link: LINK },
      providers: [provideRouter([{ path: 'masters/currencies', children: [] }])],
    });

    const link = screen.getByRole('link', { name: /Currency Master/ });
    expect(link.getAttribute('href')).toBe('/masters/currencies');
  });

  it('marks itself aria-current="page" once the route is active', async () => {
    await render(NavItemComponent, {
      inputs: { link: LINK },
      providers: [provideRouter([{ path: 'masters/currencies', children: [] }])],
    });
    await TestBed.inject(Router).navigateByUrl('/masters/currencies');

    expect(screen.getByRole('link', { name: /Currency Master/ }).getAttribute('aria-current')).toBe(
      'page',
    );
  });

  it('rail mode hides the label and shows a single-letter glyph instead', async () => {
    const { container } = await render(NavItemComponent, {
      inputs: { link: LINK, rail: true },
      providers: [provideRouter([])],
    });

    expect(container.querySelector('.app-nav-item__rail-glyph')?.textContent).toBe('C');
  });

  it('toggling the favourite star does not navigate, and emits favouriteToggled', async () => {
    const favouriteToggled = vi.fn();
    await render(NavItemComponent, {
      inputs: { link: LINK },
      on: { favouriteToggled },
      providers: [provideRouter([{ path: 'masters/currencies', children: [] }])],
    });

    await userEvent.click(screen.getByRole('button', { name: /Add.*Favourites/ }));

    expect(favouriteToggled).toHaveBeenCalledWith(LINK);
    expect(TestBed.inject(Router).url).toBe('/');
  });
});
