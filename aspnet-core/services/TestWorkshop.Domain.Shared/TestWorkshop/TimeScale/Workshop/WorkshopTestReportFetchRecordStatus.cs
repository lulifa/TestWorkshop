namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试报告获取记录状态。
/// </summary>
public enum WorkshopTestReportFetchRecordStatus
{
    /// <summary>
    /// 等待执行
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 正在请求外部接口
    /// </summary>
    Processing = 1,

    /// <summary>
    /// 外部接口暂无报告
    /// </summary>
    NoReport = 2,

    /// <summary>
    /// 获取成功
    /// </summary>
    Succeeded = 3,

    /// <summary>
    /// 获取失败
    /// </summary>
    Failed = 4
}
