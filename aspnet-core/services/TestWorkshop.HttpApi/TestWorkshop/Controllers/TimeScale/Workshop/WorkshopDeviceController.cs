namespace TestWorkshop;

/// <summary>
/// 车间设备管理
/// </summary>
[Route("api/workshop/device")]
public class WorkshopDeviceController : TestWorkshopController
{
    private readonly IWorkshopDeviceAppService Service;

    public WorkshopDeviceController(IWorkshopDeviceAppService workshopDeviceAppService)
    {
        Service = workshopDeviceAppService;
    }

    /// <summary>
    /// 分页查询车间设备
    /// </summary>
    [HttpGet]
    public virtual async Task<PagedResultDto<WorkshopDeviceDto>> GetListAsync(WorkshopDeviceGetListInput input)
    {
        return await Service.GetListAsync(input);
    }

    /// <summary>
    /// 获取设备详情
    /// </summary>
    /// <summary>
    /// 获取当前用户可访问的组织单元
    /// </summary>
    [HttpGet("organization-units")]
    public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync()
    {
        return await Service.GetOrganizationUnitsAsync();
    }

    /// <summary>
    /// 获取设备类型选项
    /// </summary>
    [HttpGet("types")]
    public virtual async Task<ListResultDto<WorkshopDeviceTypeDto>> GetTypesAsync()
    {
        return await Service.GetTypesAsync();
    }

    [HttpGet("{id}")]
    public virtual async Task<WorkshopDeviceDto> GetAsync(Guid id)
    {
        return await Service.GetAsync(id);
    }

    /// <summary>
    /// 新建设备
    /// </summary>
    [HttpPost]
    public virtual async Task<WorkshopDeviceDto> CreateAsync(WorkshopDeviceCreateDto input)
    {
        return await Service.CreateAsync(input);
    }

    /// <summary>
    /// 编辑设备
    /// </summary>
    [HttpPut("{id}")]
    public virtual async Task<WorkshopDeviceDto> UpdateAsync(Guid id, WorkshopDeviceUpdateDto input)
    {
        return await Service.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [HttpDelete("{id}")]
    public virtual async Task DeleteAsync(Guid id)
    {
        await Service.DeleteAsync(id);
    }
}
