using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.VRGamingService.Core
{
    public interface IGameRecordSchedule
    {
        ValueTask<GameRecordScheduleReply?> GetAsync(string brandCode, string merchantid, CancellationToken cancellationToken = default);

        ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule);

        ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, string brandCode, string merchantid, CancellationToken cancellationToken = default);

        ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecorTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default);
    }
}
