using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GameRecord.VRGamingService.Core;

public interface IUtensil
{
    ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default);

    ValueTask<VRConfiguration> GetSettingConfigurationAsync(
        CancellationToken cancellationToken = default);
}
