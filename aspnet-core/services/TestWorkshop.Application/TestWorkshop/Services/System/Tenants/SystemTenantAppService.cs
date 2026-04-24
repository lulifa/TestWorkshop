namespace TestWorkshop;

[Authorize(TenantManagementPermissions.Tenants.Default)]
public class SystemTenantAppService : TestWorkshopAppService, ISystemTenantAppService
{
    protected IAbpTenantAppService AbpTenantAppService { get; }
    protected IDistributedEventBus EventBus { get; }
    protected ITenantRepository TenantRepository { get; }
    protected ITenantManager TenantManager { get; }
    protected IDataSeeder DataSeeder { get; }
    protected TenantConnectionStringCheckOptions ConnectionStringCheckOptions { get; }

    public SystemTenantAppService(
        IAbpTenantAppService abpTenantAppService,
        ITenantRepository tenantRepository,
        ITenantManager tenantManager,
        IDistributedEventBus eventBus,
        IDataSeeder dataSeeder,
        IOptions<TenantConnectionStringCheckOptions> connectionStringCheckOptions)
    {
        AbpTenantAppService = abpTenantAppService;
        EventBus = eventBus;
        TenantRepository = tenantRepository;
        TenantManager = tenantManager;
        DataSeeder = dataSeeder;
        ConnectionStringCheckOptions = connectionStringCheckOptions.Value;
    }


    [AllowAnonymous]
    public virtual async Task<FindTenantResultDto> FindTenantByNameAsync(string name)
    {
        var result = await AbpTenantAppService.FindTenantByNameAsync(name);

        return result;
    }

    public virtual async Task<TenantDto> GetAsync(Guid id)
    {
        var tenant = await TenantRepository.FindAsync(id);
        if (tenant == null)
        {
            throw new BusinessException(TenantErrorCodes.TenantIdOrNameNotFound)
                .WithData("Tenant", id);
        }
        return ObjectMapper.Map<Tenant, TenantDto>(tenant);
    }

    public virtual async Task<PagedResultDto<TenantDto>> GetListAsync(TenantGetListInput input)
    {
        var count = await TenantRepository.GetCountAsync(input.Filter);
        var list = await TenantRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter
        );

        return new PagedResultDto<TenantDto>(
            count,
            ObjectMapper.Map<List<Tenant>, List<TenantDto>>(list)
        );
    }

    [Authorize(TenantManagementPermissions.Tenants.Create)]
    public virtual async Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        var tenant = await TenantManager.CreateAsync(input.Name);

        input.MapExtraPropertiesTo(tenant);

        if (!input.UseSharedDatabase)
        {
            tenant.SetDefaultConnectionString(input.DefaultConnectionString);

            foreach (var connectionString in input.ConnectionStrings)
            {
                tenant.SetConnectionString(connectionString.Name, connectionString.Value);
            }

        }

        await TenantRepository.InsertAsync(tenant);

        await CurrentUnitOfWork.SaveChangesAsync();

        CurrentUnitOfWork.OnCompleted(async () =>
        {
            var eto = new TenantCreatedEto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Properties =
                {
                    { IdentityDataSeedContributor.AdminEmailPropertyName, input.AdminEmailAddress },
                    { IdentityDataSeedContributor.AdminPasswordPropertyName, input.AdminPassword }
                }
            };
            await EventBus.PublishAsync(eto);
        });

        using (CurrentTenant.Change(tenant.Id, tenant.Name))
        {
            await DataSeeder.SeedAsync(
                            new DataSeedContext(tenant.Id)
                                .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName, input.AdminEmailAddress)
                                .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName, input.AdminPassword)
                            );
        }

        return ObjectMapper.Map<Tenant, TenantDto>(tenant);

    }

    [Authorize(TenantManagementPermissions.Tenants.Update)]
    public virtual async Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        var tenant = await TenantRepository.GetAsync(id);

        if (!string.Equals(tenant.Name, input.Name))
        {
            await TenantManager.ChangeNameAsync(tenant, input.Name);
        }

        tenant.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

        input.MapExtraPropertiesTo(tenant);

        await TenantRepository.UpdateAsync(tenant);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<Tenant, TenantDto>(tenant);

    }

    [Authorize(TenantManagementPermissions.Tenants.Create)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var tenant = await TenantRepository.FindAsync(id);

        if (tenant == null)
        {
            return;
        }

        await TenantRepository.DeleteAsync(tenant);

        await CurrentUnitOfWork.SaveChangesAsync();

    }

    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)]
    public async virtual Task<TenantConnectionStringDto> GetConnectionStringAsync(Guid id, string name)
    {
        var tenant = await TenantRepository.GetAsync(id);

        var tenantConnectionString = tenant.FindConnectionString(name);

        return new TenantConnectionStringDto
        {
            Name = name,
            Value = tenantConnectionString
        };
    }

    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)]
    public async virtual Task<ListResultDto<TenantConnectionStringDto>> GetConnectionStringAsync(Guid id)
    {
        var tenant = await TenantRepository.GetAsync(id);

        return new ListResultDto<TenantConnectionStringDto>(
            ObjectMapper.Map<List<TenantConnectionString>, List<TenantConnectionStringDto>>(tenant.ConnectionStrings.ToList()));
    }

    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)]
    public async virtual Task<TenantConnectionStringDto> SetConnectionStringAsync(Guid id, TenantConnectionStringSetInput input)
    {
        var tenant = await TenantRepository.GetAsync(id);

        var oldConnectionString = tenant.FindConnectionString(input.Name);

        CurrentUnitOfWork.OnCompleted(async () =>
        {
            var eto = new TenantConnectionStringUpdatedEto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                NewValue = input.Value,
                ConnectionStringName = input.Name,
                OldValue = oldConnectionString,
            };

            await EventBus.PublishAsync(eto);
        });

        tenant.SetConnectionString(input.Name, input.Value);

        await TenantRepository.UpdateAsync(tenant);

        await CurrentUnitOfWork.SaveChangesAsync();

        return new TenantConnectionStringDto
        {
            Name = input.Name,
            Value = input.Value
        };
    }

    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)]
    public async virtual Task DeleteConnectionStringAsync(Guid id, string name)
    {
        var tenant = await TenantRepository.GetAsync(id);

        var oldConnectionString = tenant.FindConnectionString(name);

        tenant.RemoveConnectionString(name);

        CurrentUnitOfWork.OnCompleted(async () =>
        {
            var eto = new TenantConnectionStringUpdatedEto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                ConnectionStringName = name,
                OldValue = oldConnectionString,
            };

            await EventBus.PublishAsync(eto);
        });

        await TenantRepository.UpdateAsync(tenant);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async virtual Task CheckConnectionStringAsync(TenantConnectionStringCheckInput input)
    {
        if (!ConnectionStringCheckOptions.ConnectionStringCheckers.TryGetValue(input.Provider, out var connectionStringChecker))
        {
            throw new BusinessException(TenantErrorCodes.ConnectionStringProviderNotSupport)
                 .WithData("Name", input.Provider);
        }

        var checkResult = await connectionStringChecker.CheckAsync(input.ConnectionString);

        if (checkResult.Error != null)
        {
            Logger.LogWarning(checkResult.Error, "An error occurred while checking the database connection.");
        }

        // 检查连接是否可用
        if (!checkResult.Connected)
        {
            throw input.Name.IsNullOrWhiteSpace()
                ? new BusinessException(TenantErrorCodes.InvalidDefaultConnectionString)
                : new BusinessException(TenantErrorCodes.InvalidConnectionString)
                    .WithData("Name", input.Name);
        }
    }

}
