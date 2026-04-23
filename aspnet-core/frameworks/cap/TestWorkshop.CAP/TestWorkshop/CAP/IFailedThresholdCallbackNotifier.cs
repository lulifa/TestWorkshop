namespace TestWorkshop.CAP;

public interface IFailedThresholdCallbackNotifier
{
    Task NotifyAsync(AbpCAPExecutionFailedException exception);
}
