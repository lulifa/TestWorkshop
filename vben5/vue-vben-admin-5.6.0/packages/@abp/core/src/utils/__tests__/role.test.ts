import { describe, expect, it } from 'vitest';

import { RoleConstants } from '../../constants/roles';
import { hasAnyRole, hasRole } from '../role';

describe('role utils', () => {
  it('checks whether the current roles contain a role', () => {
    expect(
      hasRole(
        [RoleConstants.Admin, RoleConstants.Tester],
        RoleConstants.Tester,
      ),
    ).toBe(true);
    expect(
      hasRole(
        [RoleConstants.Admin, RoleConstants.Tester],
        RoleConstants.Auditor,
      ),
    ).toBe(false);
  });

  it('checks whether the current roles contain any expected role', () => {
    expect(
      hasAnyRole(
        [RoleConstants.Admin, RoleConstants.Tester],
        [RoleConstants.Auditor, RoleConstants.Tester],
      ),
    ).toBe(true);
    expect(
      hasAnyRole(
        [RoleConstants.Admin, RoleConstants.Tester],
        [RoleConstants.Auditor],
      ),
    ).toBe(false);
  });

  it('returns false when current roles are missing', () => {
    expect(hasRole(undefined, RoleConstants.Admin)).toBe(false);
    expect(hasAnyRole(undefined, [RoleConstants.Admin])).toBe(false);
  });
});
