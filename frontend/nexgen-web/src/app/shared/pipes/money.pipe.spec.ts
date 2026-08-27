import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { MoneyPipe } from './money.pipe';
import { ABSENT_DISPLAY, format, money, qty, type Money } from '../utils/decimal';

@Component({
  imports: [MoneyPipe],
  template: `<span data-testid="out">{{ amount() | money: options }}</span>`,
})
class HostComponent {
  readonly amount = signal<Money | undefined>(money('1234.5'));
  options: { places?: number; locale?: string } = { locale: 'en-US' };
}

describe('MoneyPipe', () => {
  const pipe = new MoneyPipe();

  it('delegates to format()', () => {
    expect(pipe.transform(money('1234.5'), { locale: 'en-US' })).toBe(
      format(money('1234.5'), { locale: 'en-US' }),
    );
    expect(pipe.transform(qty('2.5'), { places: 3, locale: 'en-US' })).toBe('2.500');
  });

  it('accepts a bare places argument', () => {
    expect(pipe.transform(money('2.5'), 0)).toBe('3');
  });

  it('renders the em dash for an absent value', () => {
    expect(pipe.transform(undefined)).toBe(ABSENT_DISPLAY);
    expect(pipe.transform(null)).toBe(ABSENT_DISPLAY);
  });

  it('is declared pure', () => {
    const definition = (MoneyPipe as unknown as { ɵpipe: { pure: boolean } }).ɵpipe;
    expect(definition.pure).toBe(true);
  });

  it('renders in a template', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[data-testid="out"]')?.textContent).toBe('1,234.50');

    fixture.componentInstance.amount.set(undefined);
    fixture.detectChanges();
    expect(element.querySelector('[data-testid="out"]')?.textContent).toBe(ABSENT_DISPLAY);
  });
});
