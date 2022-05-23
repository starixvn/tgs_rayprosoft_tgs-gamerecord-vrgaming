namespace Tgs.GameRecord.VRGamingService
{
    public record ScheduleMessageModel
    {
        public string GameProviderCode { get; init; } = default!;
        public DateTimeOffset SendDateTime { get; init; }
    }
}
