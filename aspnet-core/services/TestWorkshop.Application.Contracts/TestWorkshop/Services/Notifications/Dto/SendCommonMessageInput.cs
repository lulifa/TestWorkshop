namespace TestWorkshop;

public class SendCommonMessageInput : IValidatableObject
{
    /// <summary>
    /// 消息标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public Guid ReceiveUserId { get; set; }

    /// <summary>
    /// 发送人名称
    /// </summary>
    public string ReceiveUserName { get; set; }

    public MessageLevel MessageLevel { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var localization = validationContext.GetRequiredService<IStringLocalizer<TestWorkshopResource>>();

        if (Title.IsNullOrWhiteSpace())
        {
            yield return new ValidationResult(localization[TestWorkshopErrorCodes.MessageTitle], [nameof(Title)]);
        }

        if (Content.IsNullOrWhiteSpace())
        {
            yield return new ValidationResult(localization[TestWorkshopErrorCodes.MessageContent], [nameof(Content)]);
        }
    }
}
