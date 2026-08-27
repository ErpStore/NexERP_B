import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { HasRightDirective } from './has-right.directive';
import { PermissionService } from './permission.service';

async function renderWithRights(rights: Parameters<PermissionService['setRights']>[0]) {
  const permissions = new PermissionService();
  permissions.setRights(rights);

  return render(
    `<button *appHasRight="'Sales Order'; right: 'create'">Create sales order</button>`,
    {
      imports: [HasRightDirective],
      providers: [{ provide: PermissionService, useValue: permissions }],
    },
  );
}

describe('HasRightDirective', () => {
  it('renders its content when the right is present', async () => {
    await renderWithRights({
      'Sales Order': { view: true, create: true, edit: false, delete: false, hidden: false },
    });

    expect(screen.getByRole('button', { name: 'Create sales order' })).toBeTruthy();
  });

  it('renders nothing for an absent screen key — deny-by-default', async () => {
    await renderWithRights({});

    expect(screen.queryByRole('button')).toBeNull();
  });

  it('renders nothing when the screen exists but the specific right is false', async () => {
    await renderWithRights({
      'Sales Order': { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(screen.queryByRole('button')).toBeNull();
  });

  it('defaults to the "view" right when none is specified', async () => {
    const permissions = new PermissionService();
    permissions.setRights({
      'Sales Order': { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    await render(`<span *appHasRight="'Sales Order'">Visible</span>`, {
      imports: [HasRightDirective],
      providers: [{ provide: PermissionService, useValue: permissions }],
    });

    expect(screen.getByText('Visible')).toBeTruthy();
  });
});
