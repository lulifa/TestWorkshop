namespace TestWorkshop;

public class SendBroadCastMessageInput : IValidatableObject
{

    /// <summary>
    /// 消息标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; }


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
