namespace Tgs.GameRecord.VRGamingService.Core;

public record NatsSettingOptions
{
    public string ScheduleGroup { get; set; } = default!;
    public string ScheduleSubject { get; set; } = default!;
    public string ManualGroup { get; set; } = default!;
    public string ManualSubject { get; set; } = default!;
}
