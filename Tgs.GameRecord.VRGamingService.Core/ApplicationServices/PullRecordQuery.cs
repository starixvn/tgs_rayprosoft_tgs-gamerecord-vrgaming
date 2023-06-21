namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

public record PullRecordQuery
{
    public string BrandCode { get; init; } = default!;

    public DateTimeOffset StartTime { get; init; } = default!;

    public DateTimeOffset EndTime { get; init; } = default!;
}
