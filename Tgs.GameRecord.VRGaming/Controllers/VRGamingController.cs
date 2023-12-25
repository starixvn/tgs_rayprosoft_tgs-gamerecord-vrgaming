using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.GameRecord.VRGaming.ViewModels;
using Valhalla.MessageQueue;
using System.Text;
using System.Threading;
using Tgs.GameRecord.VRGaming.Common;
using System.Text.Json;

namespace Tgs.GameRecord.VRGaming.Controllers;

[ApiController]
[Route("VR")]
public class VRGamingController : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
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
    ///             "startTime": "2023-12-25T11:00:00+08:00",
    ///             "endTime": "2023-12-25T11:10:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public ValueTask GameRecordAsync(
        [FromServices] IMessageSender messageSender,
        GameRecordQueryViewModel source)
    {
        var parameters = new GameRecordMessage
        {
            ApiUrl = source.ApiUrl,
            Version = source.Version,
            MerchantId = source.MerchantId,
            Deskey = source.Deskey,
            StartTime = source.StartTime,
            EndTime = source.EndTime
        };

        return messageSender.PublishAsync(
            subject: "gp.gamerecord.vr",
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: HttpContext.RequestAborted);
    }
}
