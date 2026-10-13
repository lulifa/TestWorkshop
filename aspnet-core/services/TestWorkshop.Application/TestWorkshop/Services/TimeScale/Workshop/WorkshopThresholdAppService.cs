using Volo.Abp.Guids;

namespace TestWorkshop;

/// <summary>
/// 阈值管理应用服务：按设备类型维护各数据类型的上下限阈值。
/// </summary>
[Authorize]
public class WorkshopThresholdAppService : TestWorkshopAppService, IWorkshopThresholdAppService
{
    private readonly IWorkshopThresholdRuleRepository _thresholdRepository;
    private readonly IGuidGenerator _guidGenerator;

    public WorkshopThresholdAppService(
        IWorkshopThresholdRuleRepository thresholdRepository,
        IGuidGenerator guidGenerator)
    {
        _thresholdRepository = thresholdRepository;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task<ListResultDto<WorkshopThresholdRuleDto>> GetByDeviceTypeAsync(DeviceTypeEnum deviceType)
    {
        var rules = await _thresholdRepository.GetListByDeviceTypeAsync(deviceType);
        var ruleMap = rules.ToDictionary(x => x.DataItemType, StringComparer.OrdinalIgnoreCase);

        // 固定按物理量集合返回 5 行，未配置的用空值占位，界面直接按行渲染
        var items = TelemetryQuantity.All
            .Select(quantity =>
            {
                ruleMap.TryGetValue(quantity.Code, out var rule);
                return new WorkshopThresholdRuleDto
                {
                    Id = rule?.Id,
                    DeviceType = deviceType,
                    DataItemType = quantity.Code,
                    DataItemName = quantity.Name,
                    Unit = quantity.Unit,
                    UpperLimit = rule?.UpperLimit,
                    LowerLimit = rule?.LowerLimit,
                    AlarmContent = rule?.AlarmContent,
                    Enabled = rule?.Enabled ?? true
                };
            })
            .ToList();

        return new ListResultDto<WorkshopThresholdRuleDto>(items);
    }

    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task SaveAsync(WorkshopThresholdSaveInput input)
    {
        var rules = await _thresholdRepository.GetListByDeviceTypeAsync(input.DeviceType);
        var ruleMap = rules.ToDictionary(x => x.DataItemType, StringComparer.OrdinalIgnoreCase);

        foreach (var item in input.Items)
        {
            var quantity = TelemetryQuantity.Find(item.DataItemType)
                ?? throw new UserFriendlyException($"未知的数据类型：{item.DataItemType}");

            ruleMap.TryGetValue(quantity.Code, out var rule);

            // 上下限都没填 = 该数据类型不设阈值，若已有规则则删除
            if (!item.UpperLimit.HasValue && !item.LowerLimit.HasValue)
            {
                if (rule != null)
                {
                    await _thresholdRepository.DeleteAsync(rule);
                }

                continue;
            }

            var alarmContent = item.AlarmContent.IsNullOrWhiteSpace()
                ? $"{quantity.Name}异常"
                : item.AlarmContent.Trim();

            if (rule == null)
            {
                rule = new WorkshopThresholdRule(
                    _guidGenerator.Create(),
                    input.DeviceType,
                    quantity.Code,
                    item.UpperLimit,
                    item.LowerLimit,
                    alarmContent,
                    item.Enabled);

                await _thresholdRepository.InsertAsync(rule);
            }
            else
            {
                rule.Update(item.UpperLimit, item.LowerLimit, alarmContent, item.Enabled);
                await _thresholdRepository.UpdateAsync(rule);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
