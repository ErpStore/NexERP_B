// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

/**
 * ADR-007: "one component library, never mixed" (see also R-22 - the Blazor UI
 * loads MudBlazor *and* Bootstrap). PrimeNG is the only permitted component
 * library, and this rule is the enforcement point alongside package.json.
 */
const bannedComponentLibraries = {
  patterns: [
    {
      group: [
        '@angular/material',
        '@angular/material/*',
        '@mui/*',
        'antd',
        'antd/*',
        'bootstrap',
        'bootstrap/*',
        'primereact',
        'primereact/*',
        '@chakra-ui/*',
        'react',
        'react-dom',
        'moment',
      ],
      message:
        'ADR-007 permits exactly one component library: PrimeNG. Use dates from date-fns and money from decimal.js.',
    },
  ],
};

/**
 * The OpenAPI-generated client (M2-B10) is reachable only through core/api/*.
 * Written now so it never has to be retrofitted across feature code.
 */
const bannedGeneratedClientImports = {
  patterns: [
    {
      group: ['**/core/api/generated/*', '@/core/api/generated/*'],
      message:
        'Import the generated OpenAPI client only from core/api/**; feature code uses the typed services there.',
    },
  ],
};

/**
 * M2-C10: `decimal.js` is the only money representation (ADR-007, carried over
 * from ADR-003 unchanged) and it is imported in exactly one file, so that the
 * wire boundary, the rounding mode and the precision policy have one home.
 */
const bannedDecimalLibrary = {
  patterns: [
    {
      group: ['decimal.js', 'decimal.js/*', '**/utils/decimal/decimal', '**/utils/decimal/money'],
      message:
        'Import money helpers from src/app/shared/utils/decimal (its index.ts). Only that module may import decimal.js - M2-C10.',
    },
  ],
};

/**
 * M2-C10: JavaScript `number` is IEEE-754 binary floating point, so every one of
 * these silently converts an exact server `decimal` into an approximation.
 * `src/app/shared/utils/decimal/**` is exempt - it is where the conversion is
 * done deliberately and tested.
 */
const noFloatMoney = [
  {
    selector: "CallExpression[callee.name='parseFloat']",
    message:
      'parseFloat yields an IEEE-754 double. Use parseUserInput() from shared/utils/decimal - M2-C10.',
  },
  {
    selector: "MemberExpression[object.name='Number'][property.name='parseFloat']",
    message:
      'Number.parseFloat yields an IEEE-754 double. Use parseUserInput() from shared/utils/decimal - M2-C10.',
  },
  {
    selector: "UnaryExpression[operator='+']",
    message:
      'Unary + coerces to an IEEE-754 double. Use money()/qty() from shared/utils/decimal - M2-C10.',
  },
  {
    selector: "MemberExpression[object.name='Math'][property.name=/^(round|floor|ceil)$/]",
    message:
      'Math.round/floor/ceil round a double. Use round() from shared/utils/decimal - M2-C10.',
  },
];

/** M2-C10: formatting goes through format() or the `money` pipe, never toFixed. */
const noToFixed = [
  {
    property: 'toFixed',
    message:
      'toFixed() formats a double. Use format() or the `money` pipe from shared/utils/decimal - M2-C10.',
  },
];

/**
 * R-22: a second visual language appears the moment components start
 * hardcoding colours. Colour lives in src/styles/tokens.css and is reached
 * through var(--token) - see src/app/core/theme/README.md.
 *
 * ESLint can only police the halves angular.json lintFilePatterns covers
 * (TypeScript and templates). The .css/.scss half is covered by
 * src/app/core/theme/no-raw-colour.spec.ts rather than by a second linter.
 */
const noRawColour = [
  {
    selector: 'Literal[value=/#[0-9a-fA-F]{3,8}/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
  {
    selector: 'Literal[value=/(rgb|hsl)a?[(]/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
  {
    selector: 'TemplateElement[value.raw=/#[0-9a-fA-F]{3,8}/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
];

// The same ban expressed against the Angular template AST, which uses its own
// node types rather than ESTree Literal: a static attribute (style="color: #fff")
// parses to TextAttribute, an interpolated or bound value to LiteralPrimitive.
// angular.json lintFilePatterns covers src/**/*.html, so templates are inside the
// stated scope of the ban (KB-051, R-22).
const noRawColourTemplate = [
  {
    selector: 'TextAttribute[value=/#[0-9a-fA-F]{3,8}/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
  {
    selector: 'TextAttribute[value=/(rgb|hsl)a?[(]/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
  {
    selector: 'LiteralPrimitive[value=/#[0-9a-fA-F]{3,8}/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
  {
    selector: 'LiteralPrimitive[value=/(rgb|hsl)a?[(]/]',
    message:
      'No raw colour literal. Use a semantic token from src/styles/tokens.css, e.g. var(--accent) (KB-051, R-22).',
  },
];

module.exports = defineConfig([
  {
    ignores: ['dist/**', 'coverage/**', 'playwright-report/**', 'test-results/**', '.angular/**'],
  },
  {
    files: ['**/*.ts'],
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: __dirname,
      },
    },
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommendedTypeChecked,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        { type: 'attribute', prefix: 'app', style: 'camelCase' },
      ],
      '@angular-eslint/component-selector': [
        'error',
        { type: 'element', prefix: 'app', style: 'kebab-case' },
      ],
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            ...bannedComponentLibraries.patterns,
            ...bannedGeneratedClientImports.patterns,
            ...bannedDecimalLibrary.patterns,
          ],
        },
      ],
      'no-restricted-syntax': ['error', ...noRawColour, ...noFloatMoney],
      'no-restricted-properties': ['error', ...noToFixed],
    },
  },
  {
    // The token layer is the one place a colour value may be written. Nothing
    // in it is a .ts file today - tokens.css holds every value and tokens.ts
    // holds only names - but the exemption is declared here so that stays a
    // deliberate choice rather than an accident of file extensions.
    files: ['src/styles/**/*.ts', 'src/app/core/theme/tokens.ts'],
    rules: {
      // Only the colour half is lifted here; the M2-C10 numeric bans still apply.
      'no-restricted-syntax': ['error', ...noFloatMoney],
    },
  },
  {
    // The generated client's own wrapper layer is the one place allowed to
    // reach into core/api/generated/**.
    files: ['src/app/core/api/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        { patterns: [...bannedComponentLibraries.patterns, ...bannedDecimalLibrary.patterns] },
      ],
    },
  },
  {
    // M2-C10: the money module is where decimal.js, explicit rounding and
    // fixed-point formatting are done deliberately - the same shape of override
    // the generated-client ban uses above. The colour ban still applies.
    files: ['src/app/shared/utils/decimal/**/*.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            ...bannedComponentLibraries.patterns,
            ...bannedGeneratedClientImports.patterns,
          ],
        },
      ],
      'no-restricted-syntax': ['error', ...noRawColour],
      'no-restricted-properties': 'off',
    },
  },
  {
    // M2-C04-01's WCAG contrast check computes colour-contrast ratios, not
    // money: no ERP value passes through it, so Math.round and toFixed there are
    // not the hazard M2-C10 removes. Listed - with the same reason - in the
    // EXEMPT map of shared/utils/decimal/no-float-money.spec.ts.
    files: ['src/app/core/theme/contrast.spec.ts'],
    rules: {
      'no-restricted-syntax': ['error', ...noRawColour],
      'no-restricted-properties': 'off',
    },
  },
  {
    files: ['**/*.html'],
    // Template accessibility rules are errors from commit one: a11y is a
    // build-time gate, not a later pass (KB-051 Accessibility commitments).
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {
      'no-restricted-syntax': ['error', ...noRawColourTemplate],
    },
  },
]);
