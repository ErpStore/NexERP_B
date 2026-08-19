// ESLint flat config. Two of the rules below are not style preferences: they are
// ADR-003 (docs/kb/decisions/ADR-003-react-stack.md) made mechanical.
//   1. Exactly ONE component library. ADR-003 "the two are never mixed"; the
//      live Blazor UI mixes MudBlazor 8 and Bootstrap 5 (risk R-22) and that is
//      the mistake this rule exists to prevent recurring here.
//   2. shared/api/generated/** is import-restricted to shared/api/**. The
//      generated OpenAPI client arrives in M2-B10; the rule is written now so it
//      never has to be retrofitted across a large codebase.
import js from '@eslint/js';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import reactHooks from 'eslint-plugin-react-hooks';
import simpleImportSort from 'eslint-plugin-simple-import-sort';
import globals from 'globals';
import tseslint from 'typescript-eslint';

const bannedComponentLibraries = [
  {
    group: ['@mui/*', '@mui'],
    message: 'ADR-003: Mantine is the only component library. Do not add MUI.',
  },
  {
    group: ['antd', 'antd/*'],
    message: 'ADR-003: Mantine is the only component library. Do not add Ant Design.',
  },
  {
    group: ['bootstrap', 'bootstrap/*', 'react-bootstrap', 'react-bootstrap/*'],
    message:
      'ADR-003: Mantine is the only component library. Do not add Bootstrap (see risk R-22).',
  },
  {
    group: ['primereact', 'primereact/*', 'primeng', 'primeflex', 'primeicons'],
    message:
      'ADR-003: Mantine is the only component library. PrimeReact/PrimeNG belong to the archived Angular pilot.',
  },
  {
    group: ['@chakra-ui/*', '@radix-ui/*', 'moment'],
    message: 'ADR-003: not part of the approved stack.',
  },
];

export default tseslint.config(
  {
    ignores: [
      'dist/**',
      'coverage/**',
      'playwright-report/**',
      'test-results/**',
      'node_modules/**',
    ],
  },
  js.configs.recommended,
  {
    // Type-aware linting applies to TypeScript only. eslint.config.js itself is
    // plain JS and is not in any tsconfig, so type-checked rules must not reach it.
    files: ['**/*.{ts,tsx}'],
    extends: [...tseslint.configs.recommendedTypeChecked],
    languageOptions: {
      parserOptions: {
        // Both projects, explicitly: tsconfig.json covers src/ and e2e/,
        // tsconfig.node.json covers the build/test config files at the root.
        project: ['./tsconfig.json', './tsconfig.node.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      globals: { ...globals.browser },
    },
    plugins: {
      'react-hooks': reactHooks,
      'jsx-a11y': jsxA11y,
      'simple-import-sort': simpleImportSort,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      ...jsxA11y.flatConfigs.recommended.rules,
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error',
      'no-restricted-imports': 'off',
      '@typescript-eslint/no-restricted-imports': [
        'error',
        {
          patterns: [
            ...bannedComponentLibraries,
            {
              group: ['@/shared/api/generated/*', '**/shared/api/generated/*'],
              message:
                'The generated OpenAPI client (M2-B10) may only be imported from src/shared/api/**. Re-export what you need from there.',
            },
          ],
        },
      ],
    },
  },
  {
    // shared/api/** is the one place allowed to reach into the generated client.
    files: ['src/shared/api/**/*.{ts,tsx}'],
    rules: { '@typescript-eslint/no-restricted-imports': 'off' },
  },
  {
    files: ['*.config.ts'],
    languageOptions: { globals: { ...globals.node } },
  },
  {
    files: ['e2e/**/*.ts', 'src/test/**/*.ts'],
    languageOptions: { globals: { ...globals.node } },
  },
);
