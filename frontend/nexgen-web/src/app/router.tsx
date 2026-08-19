import { Suspense } from 'react';
import { useTranslation } from 'react-i18next';
import { createBrowserRouter, RouterProvider } from 'react-router';

// Single placeholder route. No auth guard: guards, the permission store and the
// login route are M2-C02. No app shell: that is M2-C03.
function PlaceholderPage() {
  const { t } = useTranslation();
  return (
    <main>
      <h1>{t('app.name')}</h1>
      <p>Build {__APP_VERSION__}</p>
    </main>
  );
}

// Suspense wraps the route element so React.lazy route splitting works from the
// first feature route onward (KB-050 "Performance targets").
export const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <Suspense fallback={<p>Loading…</p>}>
        <PlaceholderPage />
      </Suspense>
    ),
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
