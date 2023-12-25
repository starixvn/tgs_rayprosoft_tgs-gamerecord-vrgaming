using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.GameRecord.VRGaming.Core.ApplicationServices;
using Tgs.GameRecord.VRGaming.Core;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGaming.Core.ApplicationServices;

internal class HandleRecordCommandService(
    IOptions<GameRecordInfoOptions> options,
    IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {

        var wagerState = WagerStates.Running;
        var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = 0.0m;
        var turnover = 0.0m;
        var winLoss = 0.0m;
        var resettleState = ResettleStates.None;
        var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = DateTimeOffset.UnixEpoch;
        var externalResettleOn = DateTimeOffset.UnixEpoch;
        if (command.Info.IsClosed)
        {
            externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
            turnover = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
            settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
            winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
            wagerState = (winLoss > 0) ? WagerStates.Win : WagerStates.Lose;

            if (command.Info.IsResettle)
            {
                wagerState = WagerStates.Reject;
            }

            if (command.Info.IsResettle)
            {
                resettleState = ResettleStates.Resettle;
                externalResettleOn = command.Info.ExternalResettleOn.ToDateTimeOffset();
            }
        }

        return client.MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.Info.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: resettleState,
            externalRoundId: command.Info.ExternalRoundId,
            externalWagerId: command.Info.ExternalWagerId,
            externalGameId: command.Info.ExternalGameId,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: externalResettleOn,
            bet: bet,
            validBet: bet,
            settle: settle,
            commission: 0.0m,
            turnover: turnover,
            winLoss: winLoss,
            additionalData: command.Info.AdditionalData,
            cancellationToken: cancellationToken);
    }
}
