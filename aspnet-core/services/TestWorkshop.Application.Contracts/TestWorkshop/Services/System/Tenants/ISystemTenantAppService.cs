namespace TestWorkshop;

public interface ISystemTenantAppService :
        ICrudAppService<
            TenantDto,
            Guid,
            TenantGetListInput,
            TenantCreateDto,
            TenantUpdateDto>
{

    Task<FindTenantResultDto> FindTenantByNameAsync([Required] string name);

    Task<TenantConnectionStringDto> GetConnectionStringAsync(Guid id, [Required] string connectionName);

    Task<ListResultDto<TenantConnectionStringDto>> GetConnectionStringAsync(Guid id);

    Task<TenantConnectionStringDto> SetConnectionStringAsync(Guid id, TenantConnectionStringSetInput input);

    Task DeleteConnectionStringAsync(Guid id, [Required] string connectionName);

    Task CheckConnectionStringAsync(TenantConnectionStringCheckInput input);

}
