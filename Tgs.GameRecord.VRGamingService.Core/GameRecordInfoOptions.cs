namespace Tgs.GameRecord.VRGamingService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 10;
    public double IntervalMinutesMax { get; set; } = 1440;
    public double IntervalMinutesMinimum { get; set; } = 1;
}
