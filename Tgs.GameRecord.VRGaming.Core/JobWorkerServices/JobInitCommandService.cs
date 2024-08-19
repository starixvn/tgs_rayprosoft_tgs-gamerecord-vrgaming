using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGaming.Core.JobWorkerServices;

internal class JobInitCommandService(
		ILogger<JobInitCommandService> logger,
		IOptions<GameRecordInfoOptions> options,
		IHangfireLockRepository hangfireLock,
		IGameCommandClient client) : ICommandService<JobInitCommand>
{
	public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
	{
		if (!await hangfireLock.LockingAsync(
			application: typeof(JobInitCommandService).Name,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false))
		{
			logger.LogWarning("LogOn:{logOn} Message:進行初始化動作，已經有其他服務進行", DateTime.Now.ToString("o"));
			return;
		}

		using (var connection = JobStorage.Current.GetConnection())
		{
			foreach (var recurringJob in connection.GetRecurringJobs())
			{
				RecurringJob.RemoveIfExists(recurringJob.Id);
			}
		}

		try
		{
			var settingConfiguration = await client
				.SettingGetConfigurationStringAsync(
				key: options.Value.GameProviderConfiguration,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new Exception($"This game product setting does not exist.({options.Value.GameProviderConfiguration})");

			var settingConfigurationJson = JsonNode.Parse(settingConfiguration)
				?? throw new Exception($"This game product setting does not exist.({options.Value.GameProviderConfiguration})");

			var gameProviderConfigs = await client
				.GameProviderConfigurationQueryAsync(
				gameProviderCode: options.Value.GameProviderCode,
				cancellationToken: cancellationToken)
				.ToArrayAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<GameProviderConfigInfo>();

			if (gameProviderConfigs == null || !gameProviderConfigs.Any())
			{
				return;
			}

			var gameProviderAgentMapper = new Dictionary<string, JobExecuteCommand>();

			foreach (var gameProviderConfig in gameProviderConfigs)
			{
				if (string.IsNullOrWhiteSpace(gameProviderConfig.ConfigurationData) || gameProviderConfig.Status == GameProviderConfigStatus.Inactive)
				{
					continue;
				}

				var configurationDataJson = JsonNode.Parse(gameProviderConfig.ConfigurationData)
					?? throw new Exception($"Game Provider Configuration does not exist.({gameProviderConfig.GameProviderConfigId})");

				var apiUrl = new Uri(configurationDataJson["ApiUrl"]?.ToString()
					?? settingConfigurationJson["ApiUrl"]?.ToString()
					?? throw new Exception($"{gameProviderConfig.Name} Required parameters(ApiUrl)"));

				var version = configurationDataJson["Version"]?.ToString()
					?? settingConfigurationJson["Version"]?.ToString()
					?? throw new Exception($"{gameProviderConfig.Name} Required parameters(Version)");

				var merchantId = configurationDataJson["MerchantId"]?.ToString()
					?? settingConfigurationJson["MerchantId"]?.ToString()
					?? throw new Exception($"{gameProviderConfig.Name} Required parameters(MerchantId)");

				var deskey = configurationDataJson["Deskey"]?.ToString()
					?? settingConfigurationJson["Deskey"]?.ToString()
					?? throw new Exception($"{gameProviderConfig.Name} Required parameters(Deskey)");

				if (gameProviderAgentMapper.ContainsKey(merchantId))
				{
					continue;
				}

				var jobExecuteCommand = new JobExecuteCommand(
				   ApiUrl: apiUrl,
				   Version: version,
				   MerchantId: merchantId,
				   Deskey: deskey);

				gameProviderAgentMapper.Add(merchantId, jobExecuteCommand);

				var recurringJobId = $"{gameProviderConfig.Name}|{merchantId}|{gameProviderConfig.GameProviderConfigId}";

				RecurringJob.AddOrUpdate<IHangfireUtensilRepository>(
					recurringJobId: recurringJobId,
					methodCall: service => service.JobExecuteAsync(jobExecuteCommand),
					cronExpression: Cron.Minutely(),
					options: new RecurringJobOptions()
					{
						TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
					});

				//初始化強制第一次處理
				_ = BackgroundJob.Enqueue<IHangfireUtensilRepository>(service => service.JobExecuteAsync(jobExecuteCommand));
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
		}
		finally
		{
			_ = BackgroundJob.Schedule<IHangfireUtensilRepository>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
		}
	}
}
