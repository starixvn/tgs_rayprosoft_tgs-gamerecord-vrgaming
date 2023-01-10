namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

public record GameRecordQuery
{
    public string BrandCode { get; init; } = default!;

    public DateTimeOffset StartTime { get; init; } = default!;

    public DateTimeOffset EndTime { get; init; } = default!;
}
