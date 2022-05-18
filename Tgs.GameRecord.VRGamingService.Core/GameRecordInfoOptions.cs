namespace Tgs.GameRecord.VRGamingService.Core
{
    public record GameRecordInfoOptions
    {
        public string GameProviderCode { get; set; } = default!;
        public string GameProviderConfiguration { get; set; } = default!;
        public decimal IntervalMinuteMax { get; set; } = 60;
        public decimal IntervalMinute { get; set; } = 20;
    }
}
