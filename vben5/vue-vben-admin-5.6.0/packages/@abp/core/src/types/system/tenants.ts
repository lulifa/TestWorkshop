import type {
  ExtensibleAuditedEntityDto,
  ExtensibleObject,
  IHasConcurrencyStamp,
  NameValue,
  PagedAndSortedResultRequestDto,
} from '..';

type TenantConnectionStringDto = NameValue<string>;

type TenantConnectionStringSetInput = NameValue<string>;

interface FindTenantResultDto {
  isActive: boolean;
  name?: string;
  normalizedName?: string;
  success: boolean;
  tenantId?: string;
}

interface TenantDto
  extends ExtensibleAuditedEntityDto<string>,
    IHasConcurrencyStamp {
  /** 名称 */
  name: string;
  /** 名称 */
  normalizedName: string;
}

interface GetTenantPagedListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
}

interface TenantCreateOrUpdateBase extends ExtensibleObject {
  /** 名称 */
  name: string;
}

interface TenantCreateDto extends TenantCreateOrUpdateBase {
  adminEmailAddress: string;
  adminPassword: string;
  connectionStrings?: TenantConnectionStringSetInput[];
  defaultConnectionString?: string;
  useSharedDatabase: boolean;
}

interface TenantConnectionStringCheckInput {
  connectionString: string;
  name?: string;
  provider: string;
}

interface TenantUpdateDto
  extends IHasConcurrencyStamp,
    TenantCreateOrUpdateBase {}

export type {
  FindTenantResultDto,
  GetTenantPagedListInput,
  TenantConnectionStringCheckInput,
  TenantConnectionStringDto,
  TenantConnectionStringSetInput,
  TenantCreateDto,
  TenantDto,
  TenantUpdateDto,
};
