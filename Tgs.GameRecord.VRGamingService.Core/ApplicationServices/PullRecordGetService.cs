using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.TFGamingService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService.Core.ApplicationServices;

internal class PullRecordGetService : IGetService<PullRecordQuery, PullRecordResult>
{
    private readonly IGameRecordListClient _client;
    private readonly ILogger<PullRecordGetService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;
    private DateTimeOffset? _lastGameRecordTime;

    public PullRecordGetService(
        ILogger<PullRecordGetService> logger,
        IGameRecordListClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<PullRecordResult> GetAsync(PullRecordQuery query, CancellationToken cancellationToken = default)
    {
        _lastGameRecordTime = default;
        var unsettled = await GetGameRoundAsync(query, query.BrandCode, false, cancellationToken);
        if (!unsettled)
        {
            return new PullRecordResult
            {
                Success = false
            };
        }

        var settled = await GetGameRoundAsync(query, query.BrandCode, true, cancellationToken);
        return !settled
            ? new PullRecordResult
            {
                Success = false,
                LastGameRecordTime = _lastGameRecordTime
            }
            : new PullRecordResult
            {
                Success = true,
                LastGameRecordTime = _lastGameRecordTime
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
    private async Task<bool> GetGameRoundAsync(PullRecordQuery query, string brandCode, bool isUpdateTime, CancellationToken cancellationToken = default)
    {
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
                try
                {
                    await _service
                    .ExecuteAsync(
                         command: new HandleRecordCommand(data),
                         cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                    _lastGameRecordTime = data.ExternalClosedOn.HasValue ? data.ExternalClosedOn : data.ExternalCreatedOn;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "結算遊戲紀錄發生例外.({record})", data);
                }
            }
        }
    }
}
