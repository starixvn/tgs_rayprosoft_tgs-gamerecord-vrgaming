using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameRecord.Handle.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.VRGaming.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		var betState = Handle.Common.Protos.BetState.Running;
		var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
		var settle = 0.0m;
		var turnover = 0.0m;
		var winLoss = 0.0m;
		var resettleState = Handle.Common.Protos.ResettleState.None;
		var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
		var externalClosedOn = DateTimeOffset.UnixEpoch;
		var externalResettleOn = DateTimeOffset.UnixEpoch;
		if (command.Info.IsClosed)
		{
			externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
			turnover = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
			settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
			winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
			betState = command.Info.IsCanceled ? Handle.Common.Protos.BetState.Cancel : (winLoss > 0) ? Handle.Common.Protos.BetState.Win : Handle.Common.Protos.BetState.Loss;

			if (command.Info.IsResettle)
			{
				resettleState = Handle.Common.Protos.ResettleState.Resettle;
				externalResettleOn = command.Info.ExternalResettleOn.ToDateTimeOffset();
			}
		}

		var source = new Handle.Common.ViewModels.GameRecordHandleViewModel
		{
			GameProviderCode = _gameRecordInfoOptions.GameProviderCode,
			PlayerCode = command.Info.PlayerCode,
			ExternalRoundId = command.Info.ExternalRoundId,
			ExternalBetId = command.Info.ExternalWagerId,
			ExternalGameId = command.Info.ExternalGameId,
			ExternalCreatedOn = externalCreatedOn,
			ExternalClosedOn = externalClosedOn,
			ExternalPayoffOn = externalClosedOn,
			ExternalResettleOn = externalResettleOn,
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = Handle.Common.Protos.CashoutState.None,
			ResettleState = resettleState,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = bet,
			ValidBet = bet,
			Settle = settle,
			Commission = 0.0m,
			Turnover = turnover,
			WinLoss = winLoss,
			Jackpot = 0.0M,
			AdditionalData = command.Info.AdditionalData
		};

		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning(exception, "{logWarning}",
					new
					{
						LogOn = DateTime.Now.ToString("u"),
						RetryTime = retryTime,
						RetryCount = retryCount,
						Data = new
						{
							source.GameProviderCode,
							source.PlayerCode,
							source.ExternalRoundId,
							source.ExternalBetId,
							source.ExternalGameId,
							ExternalCreatedOn = source.ExternalCreatedOn.ToString("u"),
							ExternalClosedOn = source.ExternalClosedOn.ToString("u"),
							source.BetState
						},
						exception.Message
					}));

		await retryPolicy.ExecuteAsync(() => client.HandleAsync(
			source: source,
			cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}
}
