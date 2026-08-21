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
