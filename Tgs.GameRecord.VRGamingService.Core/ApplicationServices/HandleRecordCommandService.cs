using System.Text.Json;
using Tgs.GameRecord.VRGamingService.Core;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;
using Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRoundsWorkClient _client;
    private readonly IUtensil _utensil;

    public HandleRecordCommandService(
        IGameRoundsWorkClient client,
        IUtensil utensil)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var success = true;
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var playerModel = await _utensil
            .GetPlayerAsync(
                playerCode: command.Record.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: command.Record.ExternalGameId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: playerModel.BrandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }
        if (success)
        {
            await _client
            .RoundBidding(
                gameProviderId: command.Record.GameProviderId,
                gameProviderCode: command.Record.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId ?? -1,
                externalGameId: command.Record.ExternalGameId,
                playerCode: command.Record.PlayerCode,
                walletId: playerModel!.DefaultWalletId,
                currencyCode: playerModel!.DefaultWalletCurrencyCode,
                brandId: playerModel!.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId!.Value,
                status: (RoundStatus)command.Record.Status,
                clientStatus: (RoundClientStatus)command.Record.ClientStatus,
                externalCreatedOn: command.Record.ExternalCreatedOn!.Value,
                externalClosedOn: command.Record.ExternalClosedOn,
                bet: command.Record.BetAmount.GetValueOrDefault(0),
                validBet: command.Record.ValidBetAmount.GetValueOrDefault(0),
                settle: command.Record.SettleAmount.GetValueOrDefault(0),
                commission: command.Record.Commission.GetValueOrDefault(0),
                turnover: command.Record.Turnover.GetValueOrDefault(0),
                playerWinLoss: command.Record.PlayerWinLoss.GetValueOrDefault(0),
                additionalData: command.Record.AdditionalData,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            await _client
            .RoundMissing(
                gameProviderId: command.Record.GameProviderId,
                gameProviderCode: command.Record.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId ?? -1,
                externalGameId: command.Record.ExternalGameId,
                playerCode: command.Record.PlayerCode,
                data: JsonSerializer.Serialize(command.Record),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}
