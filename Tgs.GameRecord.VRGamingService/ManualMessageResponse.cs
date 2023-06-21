namespace Tgs.GameRecord.VRGamingService;

public record ManualMessageResponse
{
    public bool Success { get; init; }
    public DateTimeOffset? LastGameRecordTime { get; init; }
}
