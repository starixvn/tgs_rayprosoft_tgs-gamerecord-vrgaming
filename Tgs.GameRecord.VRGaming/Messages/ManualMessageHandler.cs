using System.Text;
using System.Text.Json;
using Tgs.GameRecord.VRGaming.Common;
using Tgs.GameRecord.VRGaming.Core;
using Tgs.GameRecord.VRGaming.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.VRGaming.Messages;

public class ManualMessageHandler(
	ILogger<ManualMessageHandler> logger,
	IPullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<VRGamingGameRecordMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.Version,
					message.MerchantId,
					message.Deskey,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u")
				},
				Message = "This receive message handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new PullRecordCommand(
				ApiUrl: message.ApiUrl,
				Version: message.Version,
				MerchantId: message.MerchantId,
				Deskey: message.Deskey,
				StartTime: message.StartTime,
				EndTime: message.EndTime),
			cancellationToken: cancellationToken);
	}
}
