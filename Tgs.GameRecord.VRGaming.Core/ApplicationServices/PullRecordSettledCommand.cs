namespace Tgs.GameRecord.VRGaming.Core.ApplicationServices;

public record PullRecordSettledCommand(
    Uri ApiUrl,
    string Version,
    string MerchantId,
    string Deskey,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
