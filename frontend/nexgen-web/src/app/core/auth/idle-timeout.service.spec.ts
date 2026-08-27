import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  IDLE_TIMEOUT_MINUTES,
  IDLE_WARNING_SECONDS,
  IdleTimeoutService,
} from './idle-timeout.service';

/** 1-minute timeout, 10-second warning — small numbers so the fake-timer arithmetic in this
 * file stays readable. */
function createService(): IdleTimeoutService {
  TestBed.configureTestingModule({
    providers: [
      { provide: IDLE_TIMEOUT_MINUTES, useValue: 1 },
      { provide: IDLE_WARNING_SECONDS, useValue: 10 },
    ],
  });
  return TestBed.inject(IdleTimeoutService);
}

describe('IdleTimeoutService', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('is not warning immediately after start', () => {
    const service = createService();

    service.start(vi.fn());

    expect(service.isWarning()).toBe(false);
  });

  it('warns exactly at (timeout - warningSeconds), with the full countdown remaining', () => {
    const service = createService();
    service.start(vi.fn());

    vi.advanceTimersByTime(50_000); // 60s timeout - 10s warning = warn at 50s

    expect(service.isWarning()).toBe(true);
    expect(service.secondsRemaining()).toBe(10);
  });

  it('counts down one second at a time and expires at zero, calling onExpire exactly once', () => {
    const service = createService();
    const onExpire = vi.fn();
    service.start(onExpire);

    vi.advanceTimersByTime(50_000); // reach the warning
    expect(service.secondsRemaining()).toBe(10);

    vi.advanceTimersByTime(9_000);
    expect(service.secondsRemaining()).toBe(1);
    expect(onExpire).not.toHaveBeenCalled();

    vi.advanceTimersByTime(1_000);
    expect(onExpire).toHaveBeenCalledTimes(1);
    expect(service.isWarning()).toBe(false);

    // No further calls from timers that should already be cleared.
    vi.advanceTimersByTime(60_000);
    expect(onExpire).toHaveBeenCalledTimes(1);
  });

  it('recordActivity before the warning resets the full timeout window', () => {
    const service = createService();
    const onExpire = vi.fn();
    service.start(onExpire);

    vi.advanceTimersByTime(40_000);
    service.recordActivity();
    vi.advanceTimersByTime(40_000); // 80s total elapsed, but only 40s since the reset

    expect(service.isWarning()).toBe(false);
    expect(onExpire).not.toHaveBeenCalled();
  });

  it('staySignedIn during the warning dismisses it and restarts the full window', () => {
    const service = createService();
    const onExpire = vi.fn();
    service.start(onExpire);

    vi.advanceTimersByTime(50_000);
    expect(service.isWarning()).toBe(true);

    service.staySignedIn();

    expect(service.isWarning()).toBe(false);
    vi.advanceTimersByTime(49_000);
    expect(service.isWarning()).toBe(false);
    expect(onExpire).not.toHaveBeenCalled();
  });

  it('stop() cancels every pending timer — no warning, no expiry, ever', () => {
    const service = createService();
    const onExpire = vi.fn();
    service.start(onExpire);

    service.stop();
    vi.advanceTimersByTime(120_000);

    expect(service.isWarning()).toBe(false);
    expect(onExpire).not.toHaveBeenCalled();
  });

  it('R-17 regression: two independent instances never share a clock', () => {
    // SessionTimeoutService.cs is AddSingleton with one shared _lastActivity field
    // (V.SMART/V.SMART.Shared/Services/SessionTimeoutService.cs:11) — every concurrent user
    // shares one idle clock. This constructs two real instances directly (not through one
    // shared TestBed injector) and proves activity on one never resets the other's timer.
    TestBed.configureTestingModule({
      providers: [
        { provide: IDLE_TIMEOUT_MINUTES, useValue: 1 },
        { provide: IDLE_WARNING_SECONDS, useValue: 10 },
      ],
    });
    const a = TestBed.runInInjectionContext(() => new IdleTimeoutService());
    const b = TestBed.runInInjectionContext(() => new IdleTimeoutService());

    const onExpireA = vi.fn();
    const onExpireB = vi.fn();
    a.start(onExpireA);
    b.start(onExpireB);

    vi.advanceTimersByTime(30_000);
    a.recordActivity(); // resets A only

    vi.advanceTimersByTime(20_000); // B is now at 50s total — should be warning; A is at 20s since reset
    expect(b.isWarning()).toBe(true);
    expect(a.isWarning()).toBe(false);
  });
});
