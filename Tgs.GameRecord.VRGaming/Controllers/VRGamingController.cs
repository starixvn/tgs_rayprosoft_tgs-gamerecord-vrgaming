using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.VRGaming.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.VRGaming.Controllers;

[ApiController]
[Route("VR")]
public class VRGamingController : ControllerBase
{
	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		VRGamingGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: VRGamingNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		VRGamingGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: VRGamingNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
