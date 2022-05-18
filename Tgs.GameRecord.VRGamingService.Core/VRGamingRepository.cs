using Microsoft.Extensions.Options;
using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.VRGamingService.Core
{
    internal class VRGamingRepository : IVRGaming
    {
        private readonly IGameProvidersQueryServerClient _client;
        private readonly GameRecordInfoOptions _gameRecordInfo;

        public VRGamingRepository(
            IGameProvidersQueryServerClient client,
            IOptions<GameRecordInfoOptions> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _gameRecordInfo = options.Value;
        }

        public ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
            => _client.BrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId);

        public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
        {
            var gameModel = await _client.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);
            return gameModel?.GameId;
        }

        public async ValueTask<Guid> GetGameProviderIdAsync()
        {
            var gameProviderId = await _client.GameProviderGetGameProviderIdAsync(_gameRecordInfo.GameProviderCode).ConfigureAwait(false);
            return gameProviderId!.Value;
        }

        public ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
            => _client.PlayerGetAsync(playerCode);

        public ValueTask<VRConfiguration> GetSettingConfigurationAsync()
            => _client.SettingGetConfigurationAsync<VRConfiguration>(_gameRecordInfo.GameProviderConfiguration)!;
    }
}
