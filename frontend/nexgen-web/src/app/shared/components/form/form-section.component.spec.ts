import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FormSectionComponent } from './form-section.component';

const TEMPLATE = `
  <app-form-section
    [title]="'Party details'"
    [description]="description"
    [collapsible]="collapsible"
  >
    <p>Projected body</p>
  </app-form-section>`;

async function renderSection(overrides: Record<string, unknown> = {}) {
  return render(TEMPLATE, {
    imports: [FormSectionComponent],
    componentProperties: { description: undefined, collapsible: false, ...overrides },
  });
}

describe('app-form-section', () => {
  it('keeps its title in the accessibility tree as a real heading', async () => {
    await renderSection();

    expect(screen.getByRole('heading', { name: 'Party details', level: 3 })).toBeDefined();
  });

  it('names the region with its heading', async () => {
    await renderSection();

    expect(screen.getByRole('region', { name: 'Party details' })).toBeDefined();
  });

  it('renders the optional description', async () => {
    await renderSection({ description: 'Everything the invoice is addressed to.' });

    expect(screen.getByText('Everything the invoice is addressed to.')).toBeDefined();
  });

  it('projects its content', async () => {
    await renderSection();

    expect(screen.getByText('Projected body')).toBeDefined();
  });

  it('is not collapsible unless asked', async () => {
    await renderSection();

    expect(screen.queryByRole('button')).toBeNull();
  });

  it('collapses and expands from the keyboard, announcing state with aria-expanded', async () => {
    const { fixture } = await renderSection({ collapsible: true });

    const toggle = screen.getByRole('button');
    expect(toggle.getAttribute('aria-expanded')).toBe('true');

    toggle.focus();
    await userEvent.keyboard('{Enter}');
    fixture.detectChanges();

    expect(screen.getByRole('button').getAttribute('aria-expanded')).toBe('false');
    expect(document.querySelector('.form-section__body')?.hasAttribute('hidden')).toBe(true);

    await userEvent.keyboard(' ');
    fixture.detectChanges();

    expect(screen.getByRole('button').getAttribute('aria-expanded')).toBe('true');
  });

  it('points aria-controls at the body it toggles', async () => {
    await renderSection({ collapsible: true });

    const controls = screen.getByRole('button').getAttribute('aria-controls');
    expect(controls).not.toBeNull();
    expect(document.getElementById(controls!)).not.toBeNull();
  });
});
