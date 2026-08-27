import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { PermissionService } from '../../core/auth/permission.service';
import { PlaceholderComponent } from './placeholder.component';

async function renderWithRights(rights: Parameters<PermissionService['setRights']>[0]) {
  const permissions = new PermissionService();
  permissions.setRights(rights);

  return render(PlaceholderComponent, {
    providers: [{ provide: PermissionService, useValue: permissions }],
  });
}

describe('PlaceholderComponent', () => {
  it('shows the application name as the single level-1 heading when Dashboard is granted', async () => {
    await renderWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(screen.getByRole('heading', { level: 1 }).textContent).toContain('NexGen ERP');
    expect(screen.getByTestId('build-version').textContent).toContain('Version');
  });

  it('renders the permission-denied surface when Dashboard is absent from the rights map', async () => {
    await renderWithRights({
      'Sales Order': { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(screen.getByText(/Dashboard/)).toBeTruthy();
    expect(screen.queryByTestId('build-version')).toBeNull();
  });

  it('renders the zero-rights empty state when the caller has no screen rights at all', async () => {
    await renderWithRights({});

    expect(screen.getByText('Your account has no screen permissions yet.')).toBeTruthy();
    expect(screen.queryByTestId('build-version')).toBeNull();
  });
});
