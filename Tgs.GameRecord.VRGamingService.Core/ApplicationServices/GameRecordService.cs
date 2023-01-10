using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameProviders.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

internal class GameRecordService : IGetService<GameRecordQuery, GameRecordResult>
{
    private readonly IGameRecordListClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GameRecordService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly MongoSettingOptions _mongoSetting;
    private readonly IUtensil _utensil;
    private int _count;
    private int _failureCount;
    private DateTimeOffset? _lastGameRecordTime;
    private int _successCount;

    public GameRecordService(
        ILogger<GameRecordService> logger,
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<MongoSettingOptions> mongoSettingOptions,
        IGameRecordListClient client,
        IMessageSender messageSender,
        IUtensil utensil)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask<GameRecordResult> GetAsync(GameRecordQuery query, CancellationToken cancellationToken = default)
    {
        _count = 0;
        _successCount = 0;
        _failureCount = 0;
        _lastGameRecordTime = default;
        var unsettled = await GetGameRoundAsync(query, query.BrandCode, false, cancellationToken);
        if (!unsettled)
        {
            return new GameRecordResult
            {
                Success = false,
                SuccessCount = _successCount,
                FailureCount = _failureCount,
                Count = _count
            };
        }

        var settled = await GetGameRoundAsync(query, query.BrandCode, true, cancellationToken);
        return !settled
            ? new GameRecordResult
            {
                Success = false,
                LastGameRecordTime = _lastGameRecordTime,
                SuccessCount = _successCount,
                FailureCount = _failureCount,
                Count = _count
            }
            : new GameRecordResult
            {
                Success = true,
                LastGameRecordTime = _lastGameRecordTime,
                SuccessCount = _successCount,
                FailureCount = _failureCount,
                Count = _count
            };
    }

    /// <summary>
    /// Gets the game round asynchronous.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="brandCode">The brand code.</param>
    /// <param name="isUpdateTime">if set to <c>true</c> [is update time].</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<bool> GetGameRoundAsync(GameRecordQuery query, string brandCode, bool isUpdateTime, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var page = 1;
        var rows = 500;
        while (true)
        {
            var response = await _client.VRGameRecordListAsync(
                brandCode: brandCode,
                isUpdateTime: isUpdateTime,
                startTime: query.StartTime,
                endTime: query.EndTime,
                page: page,
                rows: rows,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (response == null || !response.Success)
            {
                return false;
            }

            var datas = response.Datas ?? Enumerable.Empty<GameRecordModle>();

            if (!datas.Any())
            {
                return true;
            }

            page++;

            foreach (var data in datas)
            {
                _count++;

                var success = true;

                var playerModel = await _utensil
                    .GetPlayerAsync(
                        playerCode: data.PlayerCode,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var gameId = await _utensil
                    .GetGameIdAsync(
                        gameProviderId: gameProviderId,
                        gameExternalId: data.ExternalGameId,
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
                    _successCount++;
                }
                else
                {
                    _failureCount++;
                }

                var message = new GameRecordMessage(
                    ExternalRoundId: data.ExternalRoundId,
                    GameProviderId: gameProviderId,
                    GameProviderCode: _gameRecordInfo.GameProviderCode,
                    GameId: gameId ?? -1,
                    ExternalGameId: data.ExternalGameId,
                    PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                    CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                    WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                    BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                    GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
                    Status: (GameRoundStatus)data.Status,
                    ClientStatus: (GameRoundClientStatus)data.ClientStatus,
                    ExternalCreatedOn: data.ExternalCreatedOn!.Value,
                    ExternalClosedOn: data.ExternalClosedOn,
                    BetAmount: data.BetAmount.GetValueOrDefault(0),
                    ValidBetAmount: data.ValidBetAmount.GetValueOrDefault(0),
                    SettleAmount: data.SettleAmount.GetValueOrDefault(0),
                    Commission: data.Commission.GetValueOrDefault(0),
                    Turnover: data.Turnover.GetValueOrDefault(0),
                    PlayerWinLoss: data.PlayerWinLoss.GetValueOrDefault(0),
                    AdditionalData: data.AdditionalData,
                    Success: success);

                _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{message}",
                    DateTime.Now.ToString("o"),
                    _mongoSetting.GlobPattern,
                    message);

                await _messageSender
                    .PublishAsync(
                        subject: _mongoSetting.GlobPattern,
                        data: message.ToBson(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                _lastGameRecordTime = data.ExternalClosedOn.HasValue ? data.ExternalClosedOn : data.ExternalCreatedOn;
            }
        }
    }
}
