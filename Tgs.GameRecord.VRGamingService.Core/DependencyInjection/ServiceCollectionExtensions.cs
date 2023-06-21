using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.VRGamingService.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
       this IServiceCollection services,
       Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
       Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
       Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
       Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions)
        => services
        .AddOptions<GameRecordInfoOptions>()
        .Configure(gameRecordInfoOptions)
        .Services
        .RegisterAllServiceUtilityTypes(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Transient)
        .AddMemoryCache()
        .AddGameProvidersClient(gameProvidersClientOptions)
        .AddGameCommandClient(gameCommandClientOptions)
        .AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
        .AddScoped<ISystemClock, SystemClock>()
        .AddScoped<IGameRecordSchedule, GameRecordScheduleRepository>()
        .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)));
}
