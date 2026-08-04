using System.ComponentModel;

namespace TestWorkshop.SignalR;

public enum MessageLevel
{
    /// <summary>
    /// 警告消息
    /// </summary>
    [Description("Warning")]
    Warning = 10,

    /// <summary>
    /// 正常消息
    /// </summary>
    [Description("Information")]
    Information = 20,

    /// <summary>
    /// 错误消息
    /// </summary>
    [Description("Error")]
    Error = 30,

}
