namespace TestWorkshop;

public static class SystemFileTypes
{

    /// <summary>
    /// 默认头像
    /// </summary>
    public const string DefaultAvatar = "DefaultAvatar";

    /// <summary>
    /// 所有允许的系统文件类型
    /// </summary>
    public static readonly HashSet<string> AllowedTypes = new()
    {
        DefaultAvatar
    };

    public static bool IsValid(string ownerType)
    {
        return !string.IsNullOrEmpty(ownerType) && AllowedTypes.Contains(ownerType);
    }

}
