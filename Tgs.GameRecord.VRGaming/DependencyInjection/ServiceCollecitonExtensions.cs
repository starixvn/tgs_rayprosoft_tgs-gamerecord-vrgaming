using Tgs.GameCommand.Client;
using Tgs.GameInterface.VRGaming.Client;
using Tgs.GameRecord.VRGaming;
using Tgs.GameRecord.VRGaming.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollecitonExtensions
{
	public static IServiceCollection AddGameRecord(
		this IServiceCollection services,
		Action<RedisOptions, IServiceProvider> redisDBconfigure,
	   Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
	   Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
	   Action<GameInterfaceVRGamingClientOptions, IServiceProvider> gameInterfaceVRGamingClientOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisDBconfigure)
		.Services
		.AddOptions<GameRecordInfoOptions>()
		.Configure(gameRecordInfoOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddGameRecordCore(
			gameCommandClientOptions: gameCommandClientOptions,
			gameInterfaceVRGamingClientOptions: gameInterfaceVRGamingClientOptions,
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
