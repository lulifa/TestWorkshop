namespace TestWorkshop;

/// <summary>
/// 车间测试订单应用服务
/// </summary>
public interface IWorkshopOrderAppService : IApplicationService
{
    Task<PagedResultDto<WorkshopOrderDto>> GetListAsync(WorkshopOrderListInput input);

    Task<WorkshopOrderDto> GetAsync(Guid id);

    Task<WorkshopOrderDto> CreateAsync(WorkshopOrderCreateDto input);

    Task<WorkshopOrderDto> UpdateAsync(Guid id, WorkshopOrderUpdateDto input);

    Task DeleteAsync(Guid id);

    Task<WorkshopOrderDto> StartTestAsync(Guid id);

    Task<WorkshopOrderDto> EndTestAsync(Guid id);

    Task<WorkshopOrderDto> CancelAsync(Guid id);

    Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetTelemetryTasksAsync(
        Guid id,
        WorkshopTelemetryTaskListInput input);

    /// <summary>
    /// 获取订单的遥测图谱数据（按采集时间 Timestamp 分组，每组 8 条通道曲线）
    /// </summary>
    Task<WorkshopOrderTelemetryGraphDto> GetTelemetryGraphAsync(Guid id);
}
