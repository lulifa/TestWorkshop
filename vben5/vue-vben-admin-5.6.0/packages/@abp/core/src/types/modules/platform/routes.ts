import type { EntityDto } from '../..';

interface RouteDto extends EntityDto<string> {
  description?: string;
  displayName: string;
  meta: Record<string, string>;
  name: string;
  path: string;
  redirect?: string;
}

export type { RouteDto };
