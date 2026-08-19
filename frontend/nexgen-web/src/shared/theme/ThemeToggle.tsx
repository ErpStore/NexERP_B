import { type KeyboardEvent, useRef } from 'react';

import { useTheme } from './ThemeProvider';
import classes from './ThemeToggle.module.css';
import { COLOR_SCHEME_PREFERENCES, type ColorSchemePreference } from './useColorScheme';

/**
 * The three-state colour-scheme control: Light / Dark / System.
 *
 * EXPORTED BUT NOT PLACED. M2-C03 puts it in the application header; this task
 * only builds it.
 *
 * It is an ARIA radio group with a roving tabindex, which is what makes the
 * keyboard model honest: one Tab stop for the whole group, arrow keys to move
 * between options, Enter/Space to choose (native <button> activation). The
 * selected mode is announced through aria-checked, not through colour alone.
 *
 * Labels are English literals rather than i18n keys: shared primitives are wired
 * to i18n when the shell places them (M2-C03), and this component must render in
 * a test with no I18nextProvider above it.
 */
const OPTION_LABELS: Record<ColorSchemePreference, string> = {
  light: 'Light',
  dark: 'Dark',
  system: 'System',
};

export function ThemeToggle({ label = 'Colour scheme' }: { label?: string }) {
  const { preference, setPreference } = useTheme();
  const refs = useRef<Record<string, HTMLButtonElement | null>>({});

  const move = (delta: number) => {
    const index = COLOR_SCHEME_PREFERENCES.indexOf(preference);
    const from = index === -1 ? 0 : index;
    const next =
      COLOR_SCHEME_PREFERENCES[
        (from + delta + COLOR_SCHEME_PREFERENCES.length) % COLOR_SCHEME_PREFERENCES.length
      ];
    if (!next) return;
    setPreference(next);
    refs.current[next]?.focus();
  };

  // The handler lives on each option rather than on the group: an element with
  // an interactive role must itself be focusable, and the roving tabindex puts
  // focus on the options, never on the container.
  const onKeyDown = (event: KeyboardEvent<HTMLButtonElement>) => {
    if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
      event.preventDefault();
      move(1);
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
      event.preventDefault();
      move(-1);
    }
  };

  return (
    <div role="radiogroup" aria-label={label} className={classes.group}>
      {COLOR_SCHEME_PREFERENCES.map((option) => {
        const selected = option === preference;
        return (
          <button
            key={option}
            type="button"
            role="radio"
            aria-checked={selected}
            tabIndex={selected ? 0 : -1}
            ref={(element) => {
              refs.current[option] = element;
            }}
            className={classes.option}
            onKeyDown={onKeyDown}
            onClick={() => {
              setPreference(option);
            }}
          >
            {OPTION_LABELS[option]}
          </button>
        );
      })}
    </div>
  );
}
