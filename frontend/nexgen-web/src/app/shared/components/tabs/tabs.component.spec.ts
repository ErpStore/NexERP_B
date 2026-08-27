import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { TabsComponent, type TabItem } from './tabs.component';

const TABS: TabItem[] = [
  { id: 'general', label: 'General' },
  { id: 'lines', label: 'Lines' },
  { id: 'history', label: 'History' },
];

describe('app-tabs', () => {
  it('marks the active tab selected and gives it the only tabbable stop', async () => {
    await render(TabsComponent, { inputs: { tabs: TABS, activeId: 'lines' } });

    const active = screen.getByRole('tab', { name: 'Lines' });
    expect(active.getAttribute('aria-selected')).toBe('true');
    expect(active.getAttribute('tabindex')).toBe('0');

    const inactive = screen.getByRole('tab', { name: 'General' });
    expect(inactive.getAttribute('aria-selected')).toBe('false');
    expect(inactive.getAttribute('tabindex')).toBe('-1');
  });

  it('a click selects that tab', async () => {
    const { fixture } = await render(TabsComponent, {
      inputs: { tabs: TABS, activeId: 'general' },
    });

    await userEvent.click(screen.getByRole('tab', { name: 'History' }));
    fixture.detectChanges();

    expect(screen.getByRole('tab', { name: 'History' }).getAttribute('aria-selected')).toBe(
      'true',
    );
  });

  it('ArrowRight/ArrowLeft move selection and focus, wrapping at the ends', async () => {
    const { fixture } = await render(TabsComponent, {
      inputs: { tabs: TABS, activeId: 'history' },
    });

    screen.getByRole('tab', { name: 'History' }).focus();
    await userEvent.keyboard('{ArrowRight}');
    fixture.detectChanges();

    const general = screen.getByRole('tab', { name: 'General' });
    expect(general.getAttribute('aria-selected')).toBe('true');
    expect(document.activeElement).toBe(general);

    await userEvent.keyboard('{ArrowLeft}');
    fixture.detectChanges();
    expect(screen.getByRole('tab', { name: 'History' }).getAttribute('aria-selected')).toBe(
      'true',
    );
  });

  it('Home and End jump to the first and last tab', async () => {
    const { fixture } = await render(TabsComponent, {
      inputs: { tabs: TABS, activeId: 'lines' },
    });

    screen.getByRole('tab', { name: 'Lines' }).focus();
    await userEvent.keyboard('{End}');
    fixture.detectChanges();
    expect(screen.getByRole('tab', { name: 'History' }).getAttribute('aria-selected')).toBe(
      'true',
    );

    await userEvent.keyboard('{Home}');
    fixture.detectChanges();
    expect(screen.getByRole('tab', { name: 'General' }).getAttribute('aria-selected')).toBe(
      'true',
    );
  });

  it('aria-controls points at a panel id sharing the tab id, namespaced by idPrefix', async () => {
    await render(TabsComponent, {
      inputs: { tabs: TABS, activeId: 'general', idPrefix: 'doc-editor' },
    });

    expect(screen.getByRole('tab', { name: 'General' }).getAttribute('aria-controls')).toBe(
      'doc-editor-panel-general',
    );
  });
});
