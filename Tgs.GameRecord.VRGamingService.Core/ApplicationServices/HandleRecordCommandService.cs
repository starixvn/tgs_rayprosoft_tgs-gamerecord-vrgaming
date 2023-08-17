using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
        => _client
        .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Record.ExternalRoundId,
            externalGameId: command.Record.ExternalGameId,
            playerCode: command.Record.PlayerCode,
            status: (GameRecordStatus)command.Record.Status,
            clientStatus: (GameRecordClientStatus)command.Record.ClientStatus,
            externalCreatedOn: command.Record.ExternalCreatedOn!.Value,
            externalClosedOn: command.Record.ExternalClosedOn,
            bet: command.Record.BetAmount.GetValueOrDefault(0),
            validBet: command.Record.ValidBetAmount.GetValueOrDefault(0),
            settle: command.Record.SettleAmount.GetValueOrDefault(0),
            commission: command.Record.Commission.GetValueOrDefault(0),
            turnover: command.Record.Turnover.GetValueOrDefault(0),
            playerWinLoss: command.Record.PlayerWinLoss.GetValueOrDefault(0),
            additionalData: command.Record.AdditionalData,
            cancellationToken: cancellationToken);
}
