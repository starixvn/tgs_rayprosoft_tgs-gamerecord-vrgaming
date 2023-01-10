namespace Tgs.GameRecord.VRGamingService.ViewModels;

public record GameRecordQueryViewModel
{
    public string BrandCode { get; init; } = default!;

    public DateTimeOffset StartTime { get; init; } = default!;

    public DateTimeOffset EndTime { get; init; } = default!;
}
