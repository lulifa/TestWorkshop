namespace TestWorkshop;

/// <summary>
/// 订单测试报告文件
/// </summary>
public class WorkshopOrderReportFileDto
{
    /// <summary>
    /// 对外预览标识，不暴露原始文件ID
    /// </summary>
    public string Key { get; set; }

    public string FileName { get; set; }

    public long FileSize { get; set; }

    public string ContentType { get; set; }

    /// <summary>
    /// 当前文件类型是否支持浏览器预览
    /// </summary>
    public bool HasPreview { get; set; }

    public DateTime CreationTime { get; set; }
}
