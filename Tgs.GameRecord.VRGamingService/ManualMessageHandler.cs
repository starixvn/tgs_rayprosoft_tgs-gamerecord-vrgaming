using System.Text;
using System.Text.Json;
using Tgs.GameRecord.VRGamingService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly IGetService<PullRecordQuery, PullRecordResult> _service;

    public ManualMessageHandler(IGetService<PullRecordQuery, PullRecordResult> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(json)!;

        var result = await _service.GetAsync(new PullRecordQuery()
        {
            BrandCode = request.BrandCode,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        }, cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse()
        {
            Success = result.Success,
            LastGameRecordTime = result.LastGameRecordTime,
        };

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}
