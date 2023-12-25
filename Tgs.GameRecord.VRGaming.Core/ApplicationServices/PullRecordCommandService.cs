using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGaming.Core.ApplicationServices;

internal class PullRecordCommandService(
    ICommandService<PullRecordUnsettledCommand> unsettledService,
    ICommandService<PullRecordSettledCommand> settledService) : ICommandService<PullRecordCommand>
{


    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var unsettledCommand = new PullRecordUnsettledCommand(
            ApiUrl: command.ApiUrl,
            Version: command.Version,
            MerchantId: command.MerchantId,
            Deskey: command.Deskey,
            StartTime: command.StartTime,
            EndTime: command.EndTime);

        await unsettledService.ExecuteAsync(
            command: unsettledCommand,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var settledCommand = new PullRecordSettledCommand(
            ApiUrl: command.ApiUrl,
            Version: command.Version,
            MerchantId: command.MerchantId,
            Deskey: command.Deskey,
            StartTime: command.StartTime,
            EndTime: command.EndTime);

        await settledService.ExecuteAsync(
            command: settledCommand,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
