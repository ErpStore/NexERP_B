import { readFileSync, readdirSync } from 'node:fs';
import { join, resolve } from 'node:path';

import { describe, expect, it } from 'vitest';

/**
 * The R-31 regression test. `NavMenu.razor:36,148` gates on
 * `Roles="Administrator,ERPAdmin,User"` — a list containing every real role plus one that
 * does not exist, which is not a filter at all. The SPA sidebar must not reproduce it:
 * filtering is `PermissionService`/`NavFilterService`'s screen-rights map, never `role`.
 *
 * A behavioural test proving "changing role changes nothing" would need to inject a role
 * into rendering somewhere first — but `SidebarComponent` and its children take no `role`
 * input and inject no service that exposes one (`NavFilterService` reads only
 * `PermissionService.rights()`), so there is no code path *to* test. The stronger,
 * exhaustive check is this static scan, matching the acceptance criterion's own
 * verification command (`git grep -n "role" -- frontend/nexgen-web/src/app/shared/components`)
 * — case-insensitive here so `Role`, `ROLE` and `role` are all caught, not just the one
 * casing `git grep` (without `-i`) happens to be run with.
 */
describe('sidebar navigation — role is never read (R-31)', () => {
  it('no .ts or .html file under sidebar/nav-group/nav-item mentions "role"', () => {
    const roots = ['sidebar', 'nav-group', 'nav-item'].map((dir) =>
      resolve(process.cwd(), 'src/app/shared/components', dir),
    );
    const offenders: string[] = [];

    const walk = (path: string): void => {
      for (const entry of readdirSync(path, { withFileTypes: true })) {
        const full = join(path, entry.name);
        if (entry.isDirectory()) {
          walk(full);
        } else if (
          (entry.name.endsWith('.ts') || entry.name.endsWith('.html')) &&
          !entry.name.endsWith('.spec.ts') &&
          entry.name !== 'sidebar.roles.spec.ts'
        ) {
          if (/role/i.test(readFileSync(full, 'utf8'))) {
            offenders.push(full);
          }
        }
      }
    };
    roots.forEach(walk);

    expect(offenders).toEqual([]);
  });
});
