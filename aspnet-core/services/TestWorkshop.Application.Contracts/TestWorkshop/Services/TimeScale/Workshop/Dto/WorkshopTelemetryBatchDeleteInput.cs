namespace TestWorkshop;

public class WorkshopTelemetryBatchDeleteInput
{
    [MinLength(1)]
    public List<long> Ids { get; set; } = [];
}
