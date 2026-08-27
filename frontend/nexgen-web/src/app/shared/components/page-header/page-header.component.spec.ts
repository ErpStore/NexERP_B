import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { PageHeaderComponent } from './page-header.component';

describe('app-page-header', () => {
  it('renders the title as a level-1 heading', async () => {
    await render(PageHeaderComponent, {
      inputs: { title: 'Currency Master' },
      providers: [provideRouter([])],
    });

    expect(screen.getByRole('heading', { level: 1, name: 'Currency Master' })).toBeTruthy();
  });

  it('projects page-status and page-action content into their own slots', async () => {
    await render(
      `<app-page-header title="Currency Master">
         <span appPageStatus>Draft</span>
         <button appPageActions type="button">New</button>
       </app-page-header>`,
      {
        imports: [PageHeaderComponent],
        providers: [provideRouter([])],
      },
    );

    expect(screen.getByText('Draft')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'New' })).toBeTruthy();
  });
});
