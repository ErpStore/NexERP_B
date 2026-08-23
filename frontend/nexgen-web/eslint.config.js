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
          ],
        },
      ],
      'no-restricted-syntax': ['error', ...noRawColour],
    },
  },
  {
    // The token layer is the one place a colour value may be written. Nothing
    // in it is a .ts file today - tokens.css holds every value and tokens.ts
    // holds only names - but the exemption is declared here so that stays a
    // deliberate choice rather than an accident of file extensions.
    files: ['src/styles/**/*.ts', 'src/app/core/theme/tokens.ts'],
    rules: {
      'no-restricted-syntax': 'off',
    },
  },
  {
    // The generated client's own wrapper layer is the one place allowed to
    // reach into core/api/generated/**.
    files: ['src/app/core/api/**/*.ts'],
    rules: {
      'no-restricted-imports': ['error', bannedComponentLibraries],
    },
  },
  {
    files: ['**/*.html'],
    // Template accessibility rules are errors from commit one: a11y is a
    // build-time gate, not a later pass (KB-051 Accessibility commitments).
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {},
  },
]);
