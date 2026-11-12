export function hasRole(
  currentRoles: readonly string[] | undefined,
  role: string,
): boolean {
  return currentRoles?.includes(role) ?? false;
}

export function hasAnyRole(
  currentRoles: readonly string[] | undefined,
  expectedRoles: readonly string[],
): boolean {
  return expectedRoles.some((role) => currentRoles?.includes(role));
}
