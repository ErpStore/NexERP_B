import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DeferBlockBehavior } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { ConfirmationService } from 'primeng/api';
import { beforeAll, describe, expect, it } from 'vitest';

import { ConfirmDialogComponent } from './confirm-dialog.component';
import {
  ConfirmDialogService,
  type ConfirmRequest,
  type ConfirmResult,
} from './confirm-dialog.service';
import { installMatchMedia } from './jsdom-overlay-support';

const CANCEL_LINE: ConfirmRequest = {
  header: 'Cancel line',
  message: 'Cancel line 3 of SO-0001?',
  reasonRequired: true,
  destructive: true,
};

/**
 * The same shape as `app.component.html` after M2-C13: the single host lives
 * inside an `@defer` block whose trigger is the service's own `hostRequested`
 * latch. Kept in a spec-local harness rather than driving `AppComponent`
 * itself so the assertion is about the *mount boundary*, not about the root
 * component's provider set.
 */
@Component({
  selector: 'app-deferred-confirm-harness',
  imports: [ConfirmDialogComponent],
  template: `@defer (when confirmHostRequested()) {
    <app-confirm-dialog />
  }`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class DeferredConfirmHarness {
  readonly confirmHostRequested = inject(ConfirmDialogService).hostRequested;
}

async function setupNeverMounted() {
  // Playthrough, not TestBed's default 'manual': the point of these tests is
  // that the real `when` trigger mounts the real host, so nothing about the
  // deferral may be simulated.
  const view = await render(DeferredConfirmHarness, {
    providers: [ConfirmationService],
    deferBlockBehavior: DeferBlockBehavior.Playthrough,
  });
  const service = view.fixture.debugElement.injector.get(ConfirmDialogService);
  return { view, service };
}

/**
 * **This is the defect the deferral would otherwise introduce.** PrimeNG's
 * `requireConfirmation$` is a plain `Subject` (`primeng/api`), so the request
 * that *triggers* the mount emits into nothing unless
 * `ConfirmDialogService` holds it back - and the caller's promise never
 * resolves, silently and with no error. Every test here calls `confirm()`
 * against a host that has **never** mounted.
 */
describe('app-confirm-dialog, deferred - the first call is the one that mounts it', () => {
  beforeAll(installMatchMedia);

  it('does not render the host before anything asks for a confirmation', async () => {
    await setupNeverMounted();

    expect(screen.queryByRole('alertdialog')).toBeNull();
    expect(screen.queryByRole('button', { name: 'Confirm' })).toBeNull();
  });

  it('resolves the first call with the trimmed reason once the host has mounted', async () => {
    const { view, service } = await setupNeverMounted();

    let result: ConfirmResult | null = null;
    const pending = service.confirm(CANCEL_LINE).then((value) => {
      result = value;
    });

    view.fixture.detectChanges();
    await view.fixture.whenStable();

    expect(screen.getByText('Cancel line 3 of SO-0001?')).toBeTruthy();

    await userEvent.type(screen.getByRole('textbox'), '  Customer withdrew  ');
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    await pending;

    expect(result).toEqual({ confirmed: true, reason: 'Customer withdrew' });
  });

  it('resolves the first call as a cancellation when Escape closes it', async () => {
    const { view, service } = await setupNeverMounted();

    let result: ConfirmResult | null = null;
    const pending = service.confirm(CANCEL_LINE).then((value) => {
      result = value;
    });

    view.fixture.detectChanges();
    await view.fixture.whenStable();

    await userEvent.type(screen.getByRole('textbox'), 'Typed but not submitted');
    await userEvent.keyboard('{Escape}');
    await pending;

    expect(result).toEqual({ confirmed: false, reason: null });
  });

  it('refuses to confirm the first call without the required reason', async () => {
    const { view, service } = await setupNeverMounted();

    void service.confirm(CANCEL_LINE);
    view.fixture.detectChanges();
    await view.fixture.whenStable();

    const confirm = screen.getByRole<HTMLButtonElement>('button', { name: 'Confirm' });
    expect(confirm.disabled).toBe(true);

    await userEvent.type(screen.getByRole('textbox'), '   ');
    expect(confirm.disabled).toBe(true);
  });

  it('keeps serving confirmations from the now-mounted host', async () => {
    const { view, service } = await setupNeverMounted();

    const first = service.confirm(CANCEL_LINE);
    view.fixture.detectChanges();
    await view.fixture.whenStable();
    await userEvent.keyboard('{Escape}');
    expect(await first).toEqual({ confirmed: false, reason: null });

    const second = service.confirm({ header: 'Delete draft', message: 'Delete this draft?' });
    view.fixture.detectChanges();
    await view.fixture.whenStable();

    expect(screen.getByText('Delete this draft?')).toBeTruthy();
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    expect(await second).toEqual({ confirmed: true, reason: null });
  });
});
