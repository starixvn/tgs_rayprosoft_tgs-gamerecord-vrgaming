using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.VRGaming.Protos;
using Tgs.GameRecord.VRGaming.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.VRGaming.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Loss()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "VR",
			GameProviderConfiguration = "VRConfiguration",
			HangfirePrefix = "gamerecord-vr-hangfire:",
			HangfireLocking = "gamerecord-vr-locking:",
			IntervalMinutes = 3,
			IntervalMinutesMax = 15,
			IntervalMinutesMinimum = 1
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			new GameWagerInfo
			{
				PlayerCode = "4uml0083b59p",
				ExternalRoundId = "20240324",
				ExternalWagerId = "10324190827706073646",
				ExternalGameId = "105",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-03-24T11:08:27Z").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-03-24T11:33:40Z").ToTimestamp(),
				IsClosed = true,
				IsCanceled = false,
				IsResettle = false,
				Bet = 500.000000m,
				Settle = 0.000000m,
				WinLoss = -500.000000m,
				AdditionalData = "{\"ip\":\"27.64.65.203\",\"cost\":500.0,\"note\":\"\",\"odds\":\"16.83\",\"unit\":1.0,\"count\":1,\"state\":2,\"number\":\"03,24\",\"multiple\":500,\"position\":\"Hàng chục,Hàng đơn vị\",\"subState\":0,\"channelId\":105,\"lossPrize\":0.0,\"createTime\":\"2024-03-24T11:08:27Z\",\"playerName\":\"4uml0083b59p\",\"playerOdds\":1980,\"updateTime\":\"2024-03-24T11:33:40Z\",\"betTypeCode\":22013,\"betTypeName\":\"Lô xiên 2\",\"channelCode\":\"HANOIVN\",\"channelName\":\"Hà Nội (miền Bắc)\",\"issueNumber\":\"20240324\",\"playerPrize\":0.0,\"prizeDetail\":[],\"merchantCode\":\"MOVN2\",\"serialNumber\":\"10324190827706073646\",\"merchantPrize\":0.0,\"winningNumber\":\"39,32,09,29,633,713,935,7942,1490,3454,5874,2189,3069,9805,0115,5952,9003,33461,75312,98348,75341,89897,04071,57784,53350,06693,88274\"}"
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 500.000000m,
				ValidBet = 500.000000m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 500.000000m,
				WinLoss = -500.000000m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "VR",
			GameProviderConfiguration = "VRConfiguration",
			HangfirePrefix = "gamerecord-vr-hangfire:",
			HangfireLocking = "gamerecord-vr-locking:",
			IntervalMinutes = 3,
			IntervalMinutesMax = 15,
			IntervalMinutesMinimum = 1
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(
			new GameWagerInfo
			{
				PlayerCode = "4uml0083b59p",
				ExternalRoundId = "20240324",
				ExternalWagerId = "10324190743288681884",
				ExternalGameId = "105",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-03-24T11:07:43Z").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-03-24T11:33:40Z").ToTimestamp(),
				IsClosed = true,
				IsCanceled = false,
				IsResettle = false,
				Bet = 2700.000000m,
				Settle = 4850.000000m,
				WinLoss = 2150.000000m,
				AdditionalData = "{\"ip\":\"27.64.65.203\",\"cost\":2700.0,\"note\":\"\",\"odds\":\"97\",\"unit\":1.0,\"count\":54,\"state\":3,\"number\":\"03,24\",\"multiple\":50,\"position\":\"Hàng chục,Hàng đơn vị\",\"subState\":0,\"channelId\":105,\"lossPrize\":0.0,\"createTime\":\"2024-03-24T11:07:43Z\",\"playerName\":\"4uml0083b59p\",\"playerOdds\":1980,\"updateTime\":\"2024-03-24T11:33:40Z\",\"betTypeCode\":22010,\"betTypeName\":\"2D kiểu đơn-Bao lô\",\"channelCode\":\"HANOIVN\",\"channelName\":\"Hà Nội (miền Bắc)\",\"issueNumber\":\"20240324\",\"playerPrize\":4850.0,\"prizeDetail\":[{\"count\":1,\"number\":\"03\",\"awardName\":\"2D Bao Lô\"}],\"merchantCode\":\"MOVN2\",\"serialNumber\":\"10324190743288681884\",\"merchantPrize\":4850.0,\"winningNumber\":\"39,32,09,29,633,713,935,7942,1490,3454,5874,2189,3069,9805,0115,5952,9003,33461,75312,98348,75341,89897,04071,57784,53350,06693,88274\"}"
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2700.000000m,
				ValidBet = 2700.000000m,
				Settle = 4850.000000m,
				Commission = 0.0M,
				Turnover = 2700.000000m,
				WinLoss = 2150.000000m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
