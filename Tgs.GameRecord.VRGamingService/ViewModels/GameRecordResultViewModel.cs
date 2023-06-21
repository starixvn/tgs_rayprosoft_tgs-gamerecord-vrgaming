namespace Tgs.GameRecord.VRGamingService.ViewModels;

public record GameRecordResultViewModel
{
    public bool Success { get; init; }
    public DateTimeOffset? LastGameRecordTime { get; init; }
}
