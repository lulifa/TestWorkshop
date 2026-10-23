namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试报告获取状态。
/// 当前仅预留轮询对接所需的状态，外部接口接入后由后台任务更新。
/// </summary>
public enum WorkshopTestReportStatus
{
    /// <summary>
    /// 尚未获取报告
    /// </summary>
    NotReady = 0,

    /// <summary>
    /// 正在获取报告
    /// </summary>
    Fetching = 1,

    /// <summary>
    /// 报告已就绪，可审核
    /// </summary>
    Ready = 2,

    /// <summary>
    /// 报告获取失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 报告文件已超过保留期并清理
    /// </summary>
    Expired = 4
}
