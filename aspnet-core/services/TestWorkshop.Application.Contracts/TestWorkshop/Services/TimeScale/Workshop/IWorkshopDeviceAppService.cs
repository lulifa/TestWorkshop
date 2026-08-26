namespace TestWorkshop;

public interface IWorkshopDeviceAppService : IApplicationService
{
    /// <summary>
    /// 分页查询当前用户可访问的车间设备
    /// </summary>
    Task<PagedResultDto<WorkshopDeviceDto>> GetListAsync(WorkshopDeviceGetListInput input);

    /// <summary>
    /// 获取设备详情
    /// </summary>
    Task<WorkshopDeviceDto> GetAsync(Guid id);

    /// <summary>
    /// 新建设备
    /// </summary>
    Task<WorkshopDeviceDto> CreateAsync(WorkshopDeviceCreateDto input);

    /// <summary>
    /// 编辑设备
    /// </summary>
    Task<WorkshopDeviceDto> UpdateAsync(Guid id, WorkshopDeviceUpdateDto input);

    /// <summary>
    /// 删除设备
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 获取当前用户可访问的组织单元树
    /// </summary>
    Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync();

    /// <summary>
    /// 获取设备类型选项
    /// </summary>
    Task<ListResultDto<WorkshopDeviceTypeDto>> GetTypesAsync();
}
