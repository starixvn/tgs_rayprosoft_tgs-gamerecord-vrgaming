namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

public record PullRecordResult
{
    public bool Success { get; init; }
    public DateTimeOffset? LastGameRecordTime { get; init; }
}
