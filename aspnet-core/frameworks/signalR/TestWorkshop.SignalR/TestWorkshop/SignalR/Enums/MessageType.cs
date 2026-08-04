using System.ComponentModel;

namespace TestWorkshop.SignalR;

public enum MessageType
{
    /// <summary>
    /// 广播消息
    /// </summary>
    [Description("BroadCast")]
    BroadCast = 10,

    /// <summary>
    /// 普通消息
    /// </summary>
    [Description("Common")]
    Common = 20,

}
