import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { UserMenuComponent } from './user-menu.component';

describe('app-user-menu', () => {
  it('shows the user name on the trigger', async () => {
    await render(UserMenuComponent, { inputs: { userName: 'Alice' } });

    expect(screen.getByRole('button', { name: /Account menu for Alice/ })).toBeTruthy();
  });

  it('opens the menu and emits logoutRequested on Log out, closing the menu', async () => {
    const logoutRequested = vi.fn();
    await render(UserMenuComponent, {
      inputs: { userName: 'Alice' },
      on: { logoutRequested },
    });

    await userEvent.click(screen.getByRole('button', { name: /Account menu/ }));
    const logout = await screen.findByRole('menuitem', { name: /Log out/ });
    await userEvent.click(logout);

    expect(logoutRequested).toHaveBeenCalledTimes(1);
  });
});
