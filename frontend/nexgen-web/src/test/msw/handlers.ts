import type { RequestHandler } from 'msw';

// Deliberately empty. M2-C01 creates the HARNESS, not the handlers -- so that
// M2-C02 (auth) adds handlers rather than infrastructure.
export const handlers: RequestHandler[] = [];
