/**
 * Development environment.
 *
 * DELIBERATELY CONTAINS NO API HOST. The API base URL is *configuration, not
 * source* (KB-050 "Environment configuration - a defect in the pilot, not a
 * pattern"); it is read at runtime from the JSON document named below and a
 * missing value fails loudly at startup. The Angular pilot's defect - a host
 * literal in both environment files, so its production build points at a
 * developer's machine - is not reproduced here.
 */
export const environment = {
  production: false,
  /** Relative to the app's base href. Fetched by `loadAppConfig` at startup. */
  configUrl: 'config/app-config.json',
} as const;
