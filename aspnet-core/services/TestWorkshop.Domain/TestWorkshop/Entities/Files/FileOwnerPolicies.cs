namespace TestWorkshop;

/// <summary>
/// 文件业务归属类型的生命周期策略。
/// </summary>
public static class FileOwnerPolicies
{
    /// <summary>
    /// 只允许追加、不允许覆盖的业务文件类型。
    /// </summary>
    public static bool IsAppendOnly(string ownerType)
    {
        return ownerType == nameof(WorkshopTelemetryTask)
               || ownerType == SystemFileTypes.WorkshopTestReport;
    }
}
