namespace TestWorkshop.CAP;

[DisableConventionalRegistration]
public class FailedThresholdCallbackNotifier : IFailedThresholdCallbackNotifier
{
    protected AbpCAPEventBusOptions Options { get; }
    protected IExceptionNotifier ExceptionNotifier { get; }

    public FailedThresholdCallbackNotifier(
        IOptions<AbpCAPEventBusOptions> options,
        IExceptionNotifier exceptionNotifier)
    {
        Options = options.Value;
        ExceptionNotifier = exceptionNotifier;
    }

    public async virtual Task NotifyAsync(AbpCAPExecutionFailedException exception)
    {
        // 通过额外的选项来控制是否发送消息处理失败的事件
        if (Options.NotifyFailedCallback)
        {
            await ExceptionNotifier.NotifyAsync(exception);
        }
    }
}
