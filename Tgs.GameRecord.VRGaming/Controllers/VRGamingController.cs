using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.VRGaming.Common;
using Tgs.GameRecord.VRGaming.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.VRGaming.Controllers;

[ApiController]
[Route("VR")]
public class VRGamingController : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///         {
    ///             "apiUrl": "https://fe.vrbetdemo.com/",
    ///             "version": "1.0",
    ///             "merchantid": "PEGA",
    ///             "deskey": "8VF408JBV2B406488F2VH2X684T6R6DV",
    ///             "startTime": "2023-12-25T00:00:00+08:00",
    ///             "endTime": "2023-12-26T00:10:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public ValueTask GameRecordAsync(
        [FromServices] IMessageSender messageSender,
        PullRecordViewModel source)
    {
        var parameters = new VRGamingGameRecordMessage
        {
            ApiUrl = source.ApiUrl,
            Version = source.Version,
            MerchantId = source.MerchantId,
            Deskey = source.Deskey,
            StartTime = source.StartTime,
            EndTime = source.EndTime
        };

        return messageSender.PublishAsync(
            subject: VRGamingNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: HttpContext.RequestAborted);
    }
}
