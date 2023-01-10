namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

public record GameRecordResult
{
    public bool Success { get; init; }
    public DateTimeOffset? LastGameRecordTime { get; init; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int Count { get; init; }
}
