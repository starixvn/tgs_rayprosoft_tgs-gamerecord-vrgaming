using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GameRecord.VRGamingService.Core;

internal class UtensilRepository : IUtensil
{
    private readonly IGameCommandClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IMemoryCache _memoryCache;

    public UtensilRepository(
        IOptions<GameRecordInfoOptions> options,
        IGameCommandClient client,
        IMemoryCache memoryCache)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default)
    {
        var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}";
        if (!_memoryCache.TryGetValue(key, out Guid gameProviderId))
        {
            gameProviderId = await _client
                .GameProviderGetGameProviderIdAsync(
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"The identifier for this game provider does not exist.({_gameRecordInfo.GameProviderCode})");

            return _memoryCache.Set(key, gameProviderId);
        }
        return gameProviderId;
    }

    public async ValueTask<VRConfiguration> GetSettingConfigurationAsync(
        CancellationToken cancellationToken = default)
        => await _client
        .SettingGetConfigurationAsync<VRConfiguration>(
            key: _gameRecordInfo.GameProviderConfiguration,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false)
         ?? throw new Exception($"This Game Provider Configuration does not exist.({_gameRecordInfo.GameProviderConfiguration})");
}
