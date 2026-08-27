import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { ConfirmDialogService } from '../../shared/components/overlay/confirm-dialog.service';
import { unsavedChangesGuard, useBeforeUnloadGuard, type DirtyForm } from './unsaved-changes.guard';

function fakeConfirmDialog(confirmed: boolean): Pick<ConfirmDialogService, 'confirm'> {
  return { confirm: vi.fn().mockResolvedValue({ confirmed, reason: null }) };
}

describe('unsavedChangesGuard', () => {
  it('activates immediately, asking nothing, when the form is clean', async () => {
    const confirmDialog = fakeConfirmDialog(true);
    TestBed.configureTestingModule({
      providers: [{ provide: ConfirmDialogService, useValue: confirmDialog }],
    });
    const component: DirtyForm = { isDirty: signal(false) };

    const result = await TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, {} as never, {} as never, {} as never),
    );

    expect(result).toBe(true);
    expect(confirmDialog.confirm).not.toHaveBeenCalled();
  });

  it('asks for confirmation when dirty, and proceeds on Discard', async () => {
    const confirmDialog = fakeConfirmDialog(true);
    TestBed.configureTestingModule({
      providers: [{ provide: ConfirmDialogService, useValue: confirmDialog }],
    });
    const component: DirtyForm = { isDirty: signal(true) };

    const result = await TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, {} as never, {} as never, {} as never),
    );

    expect(result).toBe(true);
    expect(confirmDialog.confirm).toHaveBeenCalledTimes(1);
  });

  it('stays on the page when the confirmation is cancelled', async () => {
    const confirmDialog = fakeConfirmDialog(false);
    TestBed.configureTestingModule({
      providers: [{ provide: ConfirmDialogService, useValue: confirmDialog }],
    });
    const component: DirtyForm = { isDirty: signal(true) };

    const result = await TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, {} as never, {} as never, {} as never),
    );

    expect(result).toBe(false);
  });
});

describe('useBeforeUnloadGuard', () => {
  @Component({ selector: 'app-test-dirty-host', template: '' })
  class TestDirtyHostComponent {
    readonly dirty = signal(false);
    constructor() {
      useBeforeUnloadGuard(this.dirty);
    }
  }

  it('registers a beforeunload listener only while the signal is dirty', () => {
    const addSpy = vi.spyOn(window, 'addEventListener');
    const removeSpy = vi.spyOn(window, 'removeEventListener');
    TestBed.configureTestingModule({});
    const fixture = TestBed.createComponent(TestDirtyHostComponent);
    fixture.detectChanges();

    expect(addSpy).not.toHaveBeenCalledWith('beforeunload', expect.anything());

    fixture.componentInstance.dirty.set(true);
    fixture.detectChanges();
    expect(addSpy).toHaveBeenCalledWith('beforeunload', expect.any(Function));

    fixture.componentInstance.dirty.set(false);
    fixture.detectChanges();
    expect(removeSpy).toHaveBeenCalledWith('beforeunload', expect.any(Function));

    // Not managed by @testing-library/angular's auto-cleanup (a bare TestBed.createComponent
    // fixture) — left undestroyed, its effect() keeps running and reacting for the rest of
    // this worker's test run (R-76-shaped leak, test-setup.ts).
    fixture.destroy();
    addSpy.mockRestore();
    removeSpy.mockRestore();
  });

  it('unregisters on destroy even while still dirty', () => {
    const removeSpy = vi.spyOn(window, 'removeEventListener');
    TestBed.configureTestingModule({});
    const fixture = TestBed.createComponent(TestDirtyHostComponent);
    fixture.componentInstance.dirty.set(true);
    fixture.detectChanges();

    fixture.destroy();

    expect(removeSpy).toHaveBeenCalledWith('beforeunload', expect.any(Function));
    removeSpy.mockRestore();
  });
});
