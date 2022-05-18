namespace Tgs.GameRecord.VRGamingService.Core
{
    public record ScheduleConfig
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public DateTimeOffset LastGameRecordTime { get; set; }
    }
}
