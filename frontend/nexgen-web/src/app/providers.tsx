import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import i18next from 'i18next';
import type { ReactNode } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { I18nextProvider, initReactI18next } from 'react-i18next';

import en from '@/shared/i18n/en.json';
import { ThemeProvider } from '@/shared/theme/ThemeProvider';

// Composition order is load-bearing and is asserted by src/app/App.test.tsx:
//   ErrorBoundary -> ThemeProvider (MantineProvider) -> QueryClientProvider -> I18nextProvider
// A provider that moves silently breaks every descendant; it must fail here,
// not three tasks later.

// ThemeProvider (M2-C04-01) wraps MantineProvider with the design-token theme
// and owns the light | dark | system preference. It substitutes for the bare
// MantineProvider at the SAME depth, so the composition order above is unchanged.
const i18n = i18next.createInstance();
void i18n.use(initReactI18next).init({
  lng: 'en',
  fallbackLng: 'en',
  resources: { en: { translation: en } },
  interpolation: { escapeValue: false },
});

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
});

function FallbackPanel({
  error,
  resetErrorBoundary,
}: {
  error: unknown;
  resetErrorBoundary: () => void;
}) {
  // Deliberately unstyled. The real ErrorState primitive is M2-C04-03.
  return (
    <div role="alert">
      <h1>Something went wrong</h1>
      <p>{error instanceof Error ? error.message : String(error)}</p>
      <button type="button" onClick={resetErrorBoundary}>
        Retry
      </button>
    </div>
  );
}

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <ErrorBoundary FallbackComponent={FallbackPanel}>
      <ThemeProvider>
        <QueryClientProvider client={queryClient}>
          <I18nextProvider i18n={i18n}>{children}</I18nextProvider>
        </QueryClientProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}
