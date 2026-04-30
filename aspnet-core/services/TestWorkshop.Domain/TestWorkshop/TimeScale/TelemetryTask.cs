namespace TestWorkshop.TimeScale;

public class TelemetryTask
{
    public long Id { get; set; }

    public Guid FileId { get; set; }

    public string BlobName { get; set; }

    public int Status { get; set; } // 0=Pending 1=Processing 2=Success 3=Failed

    public int RetryCount { get; set; }

    public DateTime? NextRetryTime { get; set; }

    public string Error { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
