using System.Reflection;
using StackExchange.Redis;

using Tgs.GameCommand.Client;
using Tgs.GameInterface.VRGaming.Client;
using Tgs.GameRecord.VRGaming.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(
	   this IServiceCollection services,
	   Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
	   Action<GameInterfaceVRGamingClientOptions, IServiceProvider> gameInterfaceVRGamingClientOptions,
	   Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
		.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient)
		.AddGameCommandClient(gameCommandClientOptions)
		.AddGameInterfaceVRGamingClient(gameInterfaceVRGamingClientOptions)
		.AddTransient<IHangfireUtensil, HangfireUtensil>()
		.AddTransient<IHangfireLock>(sp => ActivatorUtilities.CreateInstance<HangfireLock>(sp, redisDatabaseResolver(sp)));
}
