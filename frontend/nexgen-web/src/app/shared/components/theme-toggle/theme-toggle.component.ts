import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  viewChildren,
} from '@angular/core';

import { ThemeService, type ThemePreference } from '@/app/core/theme/theme.service';

interface ThemeOption {
  readonly value: ThemePreference;
  readonly label: string;
  readonly icon: string;
}

/**
 * Three-state colour-scheme control: Light / Dark / System.
 *
 * Presentational and stateless - it renders `ThemeService.preference` and
 * calls `setPreference`. It is exported but **not placed**; M2-C03 puts it in
 * the application header.
 *
 * It is an ARIA radio group with a roving tabindex: one stop in the `Tab`
 * order, arrow keys move between the options, `Enter`/`Space` activate the
 * focused one (the native `button` behaviour), and the selected mode is
 * announced through `aria-checked`.
 */
@Component({
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.component.html',
  styleUrl: './theme-toggle.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeToggleComponent {
  private readonly theme = inject(ThemeService);
  private readonly buttons = viewChildren<ElementRef<HTMLButtonElement>>('option');

  readonly preference = this.theme.preference;

  readonly options: readonly ThemeOption[] = [
    { value: 'light', label: 'Light', icon: 'sun' },
    { value: 'dark', label: 'Dark', icon: 'moon' },
    { value: 'system', label: 'System', icon: 'system' },
  ];

  select(value: ThemePreference): void {
    this.theme.setPreference(value);
  }

  onKeydown(event: KeyboardEvent, index: number): void {
    const last = this.options.length - 1;
    let next: number;
    switch (event.key) {
      case 'ArrowRight':
      case 'ArrowDown':
        next = index === last ? 0 : index + 1;
        break;
      case 'ArrowLeft':
      case 'ArrowUp':
        next = index === 0 ? last : index - 1;
        break;
      case 'Home':
        next = 0;
        break;
      case 'End':
        next = last;
        break;
      default:
        return;
    }
    event.preventDefault();
    const option = this.options[next];
    if (!option) {
      return;
    }
    // Radio-group convention: moving the focus also moves the selection.
    this.select(option.value);
    this.buttons()[next]?.nativeElement.focus();
  }
}
