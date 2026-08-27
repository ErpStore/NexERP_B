import { convertToParamMap, ActivatedRoute, Router } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { AuthService } from '../../../core/auth/auth.service';
import type { LoginFailure } from '../../../core/auth/auth.models';
import { LoginComponent } from './login.component';

async function renderLogin(
  login: (
    username: string,
    password: string,
  ) => Promise<{ ok: true } | { ok: false; failure: LoginFailure }>,
  returnUrl?: string,
) {
  const authStub = { login } as unknown as AuthService;
  const routeStub = {
    snapshot: { queryParamMap: convertToParamMap(returnUrl ? { returnUrl } : {}) },
  } as unknown as ActivatedRoute;
  const navigateByUrl = vi.fn().mockResolvedValue(true);
  const routerStub = { navigateByUrl } as unknown as Router;

  const result = await render(LoginComponent, {
    providers: [
      { provide: AuthService, useValue: authStub },
      { provide: ActivatedRoute, useValue: routeStub },
      { provide: Router, useValue: routerStub },
    ],
  });

  return { ...result, navigateByUrl };
}

describe('LoginComponent', () => {
  it('successful login navigates to returnUrl', async () => {
    const login = vi.fn().mockResolvedValue({ ok: true });
    const { navigateByUrl } = await renderLogin(login, '/sales-order/42');

    await userEvent.type(screen.getByLabelText('Username'), 'alice');
    await userEvent.type(screen.getByLabelText('Password'), 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(login).toHaveBeenCalledWith('alice', 'secret');
    expect(navigateByUrl).toHaveBeenCalledWith('/sales-order/42');
  });

  it('successful login with no returnUrl navigates to /', async () => {
    const login = vi.fn().mockResolvedValue({ ok: true });
    const { navigateByUrl } = await renderLogin(login);

    await userEvent.type(screen.getByLabelText('Username'), 'alice');
    await userEvent.type(screen.getByLabelText('Password'), 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('a returnUrl that does not start with "/" is not honoured (open-redirect guard)', async () => {
    const login = vi.fn().mockResolvedValue({ ok: true });
    const { navigateByUrl } = await renderLogin(login, 'https://evil.example/phish');

    await userEvent.type(screen.getByLabelText('Username'), 'alice');
    await userEvent.type(screen.getByLabelText('Password'), 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('401 shows one generic message — never which field was wrong', async () => {
    const login = vi
      .fn()
      .mockResolvedValue({ ok: false, failure: { reason: 'invalid-credentials' } });
    await renderLogin(login);

    await userEvent.type(screen.getByLabelText('Username'), 'alice');
    await userEvent.type(screen.getByLabelText('Password'), 'wrong');
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    const alert = await screen.findByText('Invalid username or password.');
    expect(alert).toBeTruthy();
    expect(screen.queryByText(/username.*not found/i)).toBeNull();
    expect(screen.queryByText(/password.*incorrect/i)).toBeNull();
  });

  it('a trial-expired 403 shows its own verbatim server message, not the 401 text', async () => {
    const login = vi.fn().mockResolvedValue({
      ok: false,
      failure: { reason: 'trial-expired', message: 'Your trial period has expired.' },
    });
    await renderLogin(login);

    await userEvent.type(screen.getByLabelText('Username'), 'alice');
    await userEvent.type(screen.getByLabelText('Password'), 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByText('Your trial period has expired.')).toBeTruthy();
    expect(screen.queryByText('Invalid username or password.')).toBeNull();
  });

  it('submitting with empty fields does not call login and marks the form touched', async () => {
    const login = vi.fn();
    await renderLogin(login);

    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(login).not.toHaveBeenCalled();
  });

  it('the whole flow is completable with the keyboard alone', async () => {
    const login = vi.fn().mockResolvedValue({ ok: true });
    await renderLogin(login);

    // No leading Tab: the component autofocuses the username field on mount.
    await userEvent.keyboard('alice');
    await userEvent.tab();
    await userEvent.keyboard('secret');
    await userEvent.keyboard('{Enter}');

    expect(login).toHaveBeenCalledWith('alice', 'secret');
  });
});
