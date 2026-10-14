namespace TestWorkshop;

/// <summary>
/// 车间设备应用服务
/// </summary>
[Authorize]
public class WorkshopDeviceAppService : TestWorkshopAppService, IWorkshopDeviceAppService
{
    protected IWorkshopDeviceRepository WorkshopDeviceRepository { get; }
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected IWorkshopOrganizationUnitAccessChecker OrganizationUnitAccessChecker { get; }
    protected IAsyncQueryableExecuter AsyncQueryableExecuter { get; }

    public WorkshopDeviceAppService(
        IWorkshopDeviceRepository workshopDeviceRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IWorkshopOrganizationUnitAccessChecker organizationUnitAccessChecker,
        IAsyncQueryableExecuter asyncQueryableExecuter)
    {
        WorkshopDeviceRepository = workshopDeviceRepository;
        OrganizationUnitRepository = organizationUnitRepository;
        OrganizationUnitAccessChecker = organizationUnitAccessChecker;
        AsyncQueryableExecuter = asyncQueryableExecuter;
    }

    /// <summary>
    /// 分页查询当前用户可访问的车间设备
    /// </summary>
    public virtual async Task<PagedResultDto<WorkshopDeviceDto>> GetListAsync(WorkshopDeviceGetListInput input)
    {
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var query = await WorkshopDeviceRepository.GetQueryableAsync();
        var accessibleOuIds = await GetAccessibleOrganizationUnitIdsAsync();

        if (input.OrganizationUnitId.HasValue)
        {
            var descendantIds = await GetDescendantOrganizationUnitIdsAsync(input.OrganizationUnitId.Value);
            if (accessibleOuIds != null && !descendantIds.Any(accessibleOuIds.Contains))
            {
                return new PagedResultDto<WorkshopDeviceDto>(0, new List<WorkshopDeviceDto>());
            }

            query = query.Where(x => descendantIds.Contains(x.OrganizationUnitId));
        }

        if (accessibleOuIds != null)
        {
            query = query.Where(x => accessibleOuIds.Contains(x.OrganizationUnitId));
        }

        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => x.Code.Contains(filter) || x.Name.Contains(filter));
        }

        var totalCount = await AsyncQueryableExecuter.CountAsync(query);
        var items = await AsyncQueryableExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<WorkshopDeviceDto>(
            totalCount,
            ObjectMapper.Map<List<WorkshopDevice>, List<WorkshopDeviceDto>>(items));
    }

    /// <summary>
    /// 获取设备详情
    /// </summary>
    public virtual async Task<WorkshopDeviceDto> GetAsync(Guid id)
    {
        var device = await WorkshopDeviceRepository.GetAsync(id);
        await EnsureOrganizationUnitAccessibleAsync(device.OrganizationUnitId);
        return ObjectMapper.Map<WorkshopDevice, WorkshopDeviceDto>(device);
    }

    /// <summary>
    /// 新建设备
    /// </summary>
    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task<WorkshopDeviceDto> CreateAsync(WorkshopDeviceCreateDto input)
    {
        await EnsureOrganizationUnitAccessibleAsync(input.OrganizationUnitId);

        var code = input.Code.Trim();
        var existing = await WorkshopDeviceRepository.FindByCodeAsync(code);
        if (existing != null)
        {
            throw new UserFriendlyException(L["WorkshopDevice:DuplicateCode", code]);
        }

        var device = new WorkshopDevice
        {
            Code = code,
            Name = input.Name.Trim(),
            Type = input.Type,
            Model = input.Model.Trim(),
            OrganizationUnitId = input.OrganizationUnitId,
            TenantId = CurrentTenant.Id
        };

        await WorkshopDeviceRepository.InsertAsync(device);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<WorkshopDevice, WorkshopDeviceDto>(device);
    }

    /// <summary>
    /// 编辑设备
    /// </summary>
    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task<WorkshopDeviceDto> UpdateAsync(Guid id, WorkshopDeviceUpdateDto input)
    {
        var device = await WorkshopDeviceRepository.GetAsync(id);
        await EnsureOrganizationUnitAccessibleAsync(device.OrganizationUnitId);
        await EnsureOrganizationUnitAccessibleAsync(input.OrganizationUnitId);

        var code = input.Code.Trim();
        if (device.Code != code)
        {
            throw new UserFriendlyException(L["WorkshopDevice:CodeCannotBeChanged"]);
        }

        var existing = await WorkshopDeviceRepository.FindByCodeAsync(code);
        if (existing != null && existing.Id != id)
        {
            throw new UserFriendlyException(L["WorkshopDevice:DuplicateCode", code]);
        }

        device.Name = input.Name.Trim();
        device.Type = input.Type;
        device.Model = input.Model.Trim();
        device.OrganizationUnitId = input.OrganizationUnitId;

        await WorkshopDeviceRepository.UpdateAsync(device);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<WorkshopDevice, WorkshopDeviceDto>(device);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var device = await WorkshopDeviceRepository.GetAsync(id);
        await EnsureOrganizationUnitAccessibleAsync(device.OrganizationUnitId);
        await WorkshopDeviceRepository.DeleteAsync(device);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// 获取当前用户可访问的组织单元树
    /// </summary>
    public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync()
    {
        var allOrganizationUnits = await OrganizationUnitRepository.GetListAsync(false);
        var accessibleOuIds = await GetAccessibleOrganizationUnitIdsAsync();

        if (accessibleOuIds == null)
        {
            // 管理员可访问全部；这里也要显式排序，否则仓库不保证顺序，树会飘
            var allSorted = allOrganizationUnits.OrderBy(x => x.Code).ToList();
            return new ListResultDto<OrganizationUnitDto>(
                ObjectMapper.Map<List<OrganizationUnit>, List<OrganizationUnitDto>>(allSorted));
        }

        var idSet = accessibleOuIds.ToHashSet();
        var organizationUnitMap = allOrganizationUnits.ToDictionary(x => x.Id);

        foreach (var organizationUnit in allOrganizationUnits.Where(x => idSet.Contains(x.Id)).ToList())
        {
            var parentId = organizationUnit.ParentId;
            while (parentId.HasValue
                   && organizationUnitMap.TryGetValue(parentId.Value, out var parent)
                   && !idSet.Contains(parent.Id))
            {
                idSet.Add(parent.Id);
                parentId = parent.ParentId;
            }
        }

        var items = allOrganizationUnits
            .Where(x => idSet.Contains(x.Id))
            .OrderBy(x => x.Code)
            .ToList();

        return new ListResultDto<OrganizationUnitDto>(
            ObjectMapper.Map<List<OrganizationUnit>, List<OrganizationUnitDto>>(items));
    }

    /// <summary>
    /// 获取设备类型选项
    /// </summary>
    public virtual Task<ListResultDto<WorkshopDeviceTypeDto>> GetTypesAsync()
    {
        var types = Enum.GetValues<DeviceTypeEnum>()
            .Select(type => new WorkshopDeviceTypeDto
            {
                Value = (int)type,
                Name = type.ToString(),
                DisplayName = L[$"DeviceType:{type}"].ToString()
            })
            .ToList();

        return Task.FromResult(new ListResultDto<WorkshopDeviceTypeDto>(types));
    }

    // 车间可见性统一走共享的 OrganizationUnitAccessChecker，这里只保留设备自己的提示文案
    private async Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync()
    {
        return await OrganizationUnitAccessChecker.GetAccessibleOrganizationUnitIdsAsync();
    }

    private async Task EnsureOrganizationUnitAccessibleAsync(Guid organizationUnitId)
    {
        await OrganizationUnitAccessChecker.EnsureAccessibleAsync(
            organizationUnitId,
            L["WorkshopDevice:NotAuthorized"]);
    }

    private async Task<List<Guid>> GetDescendantOrganizationUnitIdsAsync(Guid organizationUnitId)
    {
        var allOrganizationUnits = await OrganizationUnitRepository.GetListAsync(false);
        var result = new HashSet<Guid>();

        void AddDescendants(Guid id)
        {
            result.Add(id);
            foreach (var child in allOrganizationUnits.Where(x => x.ParentId == id))
            {
                AddDescendants(child.Id);
            }
        }

        AddDescendants(organizationUnitId);
        return result.ToList();
    }
}
