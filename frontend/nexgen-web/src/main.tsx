import '@mantine/core/styles.css';
import '@/shared/theme/tokens.css';
import '@/styles/global.css';

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

import { App } from './app/App';

// Import order matters: Mantine's base styles first, then the tokens they are
// pointed at, then the global reset and base typography, which must win.

const container = document.getElementById('root');
if (!container) {
  throw new Error('Root element #root not found in index.html');
}

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
