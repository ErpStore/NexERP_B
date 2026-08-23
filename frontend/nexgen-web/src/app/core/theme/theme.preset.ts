import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

import { tokenRef } from './tokens';

/**
 * The PrimeNG preset, expressed entirely over the CSS variables in
 * `src/styles/tokens.css`. Not one colour value is restated here.
 *
 * ## Why this shape (the answer to Q-67)
 *
 * PrimeNG 22 emits every preset token as a `--p-*` custom property. Its value
 * emitter (`@primeuix/styled`, `getVariableValue`) rewrites `{a.b}` references
 * into `var(--p-a-b)` and passes any *brace-free* string through verbatim - so
 * a token whose value is `var(--accent)` reaches the stylesheet unchanged, and
 * the preset becomes a pure mapping onto the token layer. That is **route A**
 * of the two the task offered; the `styles.scss` `--p-*` override fallback was
 * not needed. `tokens.spec.ts` asserts the emitted declarations.
 *
 * ## Why `light-dark()` is left alone where we do not override
 *
 * Aura 3.0 does not carry a `colorScheme.dark` block: its semantic values use
 * the CSS `light-dark()` function, which is selected by the `color-scheme`
 * property. `tokens.css` sets `color-scheme: light` on `:root` and
 * `color-scheme: dark` under `[data-theme='dark']`, and `app.config.ts` sets
 * `darkModeSelector` to the same attribute. Both mechanisms therefore switch
 * off the one signal this app writes - `<html data-theme>` - so an Aura token
 * we have not overridden still flips at exactly the same moment as one we
 * have.
 *
 * Do not feed a `var(--…)` to `updatePrimaryPalette()`, `palette()`, `shade()`
 * or `tint()`: those helpers require a hex literal and silently return their
 * input otherwise. Set the semantic leaves, as below.
 */
/**
 * The extension applied over Aura - i.e. only what this project overrides.
 * Exported so `tokens.spec.ts` can assert that *our* half restates no colour
 * value; Aura's own base still carries the literals it shipped with.
 */
export const NexGenPresetExtension = {
  primitive: {
    borderRadius: {
      none: '0',
      xs: tokenRef('--radius-sm'),
      sm: tokenRef('--radius-sm'),
      md: tokenRef('--radius-md'),
      lg: tokenRef('--radius-lg'),
      xl: tokenRef('--radius-lg'),
    },
  },
  semantic: {
    transitionDuration: tokenRef('--motion-base'),
    typography: {
      fontFamily: tokenRef('--font-sans'),
      fontSize: tokenRef('--text-sm'),
      lineHeight: tokenRef('--leading-sm'),
      fontWeight: tokenRef('--weight-regular'),
    },
    // KB-051 Accessibility commitments: a visible 2 px ring at 2 px offset.
    focusRing: {
      width: '2px',
      style: 'solid',
      color: tokenRef('--focus-ring'),
      offset: '2px',
      shadow: 'none',
    },
    primary: {
      color: tokenRef('--accent'),
      hoverColor: tokenRef('--accent'),
      activeColor: tokenRef('--accent'),
    },
    text: {
      color: tokenRef('--text-primary'),
      hoverColor: tokenRef('--text-primary'),
      mutedColor: tokenRef('--text-secondary'),
      hoverMutedColor: tokenRef('--text-secondary'),
    },
    content: {
      background: tokenRef('--bg-surface'),
      hoverBackground: tokenRef('--bg-subtle'),
      borderColor: tokenRef('--border'),
      borderRadius: tokenRef('--radius-md'),
      color: tokenRef('--text-primary'),
      hoverColor: tokenRef('--text-primary'),
    },
    highlight: {
      background: tokenRef('--accent-subtle'),
      focusBackground: tokenRef('--accent-subtle'),
      color: tokenRef('--accent'),
      focusColor: tokenRef('--accent'),
    },
    formField: {
      background: tokenRef('--bg-surface'),
      disabledBackground: tokenRef('--bg-subtle'),
      filledBackground: tokenRef('--bg-subtle'),
      filledHoverBackground: tokenRef('--bg-subtle'),
      filledFocusBackground: tokenRef('--bg-subtle'),
      borderColor: tokenRef('--border'),
      hoverBorderColor: tokenRef('--border-strong'),
      focusBorderColor: tokenRef('--accent'),
      invalidBorderColor: tokenRef('--danger'),
      color: tokenRef('--text-primary'),
      disabledColor: tokenRef('--text-disabled'),
      placeholderColor: tokenRef('--text-secondary'),
      invalidPlaceholderColor: tokenRef('--danger'),
      floatLabelColor: tokenRef('--text-secondary'),
      floatLabelFocusColor: tokenRef('--accent'),
      floatLabelActiveColor: tokenRef('--text-secondary'),
      iconColor: tokenRef('--text-secondary'),
      borderRadius: tokenRef('--radius-sm'),
      fontSize: tokenRef('--text-sm'),
      shadow: 'none',
      focusRing: {
        width: '2px',
        style: 'solid',
        color: tokenRef('--focus-ring'),
        offset: '2px',
        shadow: 'none',
      },
    },
    list: {
      option: {
        color: tokenRef('--text-primary'),
        focusColor: tokenRef('--text-primary'),
        focusBackground: tokenRef('--bg-subtle'),
        selectedBackground: tokenRef('--accent-subtle'),
        selectedFocusBackground: tokenRef('--accent-subtle'),
        selectedColor: tokenRef('--accent'),
        selectedFocusColor: tokenRef('--accent'),
        borderRadius: tokenRef('--radius-sm'),
      },
    },
    navigation: {
      item: {
        color: tokenRef('--text-primary'),
        focusColor: tokenRef('--text-primary'),
        activeColor: tokenRef('--accent'),
        focusBackground: tokenRef('--bg-subtle'),
        activeBackground: tokenRef('--accent-subtle'),
        borderRadius: tokenRef('--radius-sm'),
      },
    },
    overlay: {
      select: {
        background: tokenRef('--bg-surface-raised'),
        borderColor: tokenRef('--border'),
        borderRadius: tokenRef('--radius-md'),
        color: tokenRef('--text-primary'),
        shadow: tokenRef('--elevation-2'),
      },
      popover: {
        background: tokenRef('--bg-surface-raised'),
        borderColor: tokenRef('--border'),
        borderRadius: tokenRef('--radius-md'),
        color: tokenRef('--text-primary'),
        shadow: tokenRef('--elevation-2'),
      },
      modal: {
        background: tokenRef('--bg-surface-raised'),
        borderColor: tokenRef('--border'),
        borderRadius: tokenRef('--radius-lg'),
        color: tokenRef('--text-primary'),
        shadow: tokenRef('--elevation-3'),
      },
    },
  },
} as const;

export const NexGenPreset = definePreset(Aura, NexGenPresetExtension);

/**
 * The `providePrimeNG` theme option. `darkModeSelector` is the same attribute
 * `ThemeService` writes, so PrimeNG never runs a second, competing dark-mode
 * mechanism.
 */
export const NexGenThemeOptions = {
  preset: NexGenPreset,
  options: {
    darkModeSelector: '[data-theme="dark"]',
  },
} as const;
