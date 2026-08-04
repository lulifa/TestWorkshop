namespace TestWorkshop;

public class NotificationDomainException : BusinessException
{
    public NotificationDomainException(string code = null, string message = null, string details = null, Exception innerException = null,
            LogLevel logLevel = LogLevel.Warning) : base(code, message, details,
            innerException,
            logLevel
        )
    {
    }
}
