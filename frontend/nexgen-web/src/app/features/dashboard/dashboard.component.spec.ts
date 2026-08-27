import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { PermissionService } from '../../core/auth/permission.service';
import { DashboardComponent } from './dashboard.component';

async function renderWithRights(rights: Parameters<PermissionService['setRights']>[0]) {
  const permissions = new PermissionService();
  permissions.setRights(rights);

  return render(DashboardComponent, {
    providers: [provideRouter([]), { provide: PermissionService, useValue: permissions }],
  });
}

describe('DashboardComponent', () => {
  it('shows "Dashboard" as the page title and the app version when Dashboard is granted', async () => {
    await renderWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(screen.getByRole('heading', { level: 1 }).textContent).toContain('Dashboard');
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
