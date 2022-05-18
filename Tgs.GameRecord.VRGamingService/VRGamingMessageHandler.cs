using System.Text;
using System.Text.Json;
using Tgs.GameRecord.VRGamingService.Core.JobWorkerServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService
{
    internal class VRGamingMessageHandler : IMessageHandler
    {
        private readonly ILogger<VRGamingMessageHandler> _logger;
        private readonly ICommandService<JobWorkerCommand> _service;

        public VRGamingMessageHandler(
            ILogger<VRGamingMessageHandler> logger,
            ICommandService<JobWorkerCommand> service
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var json = Encoding.UTF8.GetString(data.Span);
            var messageModel = JsonSerializer.Deserialize<VRGamingMessageModel>(json);

            if (messageModel == null)
            {
                _logger.LogError("Information Received:{LogInfo}", messageModel);
                return ValueTask.CompletedTask;
            }

            _logger.LogInformation("Information Received:{LogInfo}", messageModel);

            return _service.ExecuteAsync(new JobWorkerCommand()
            {
                GameProviderCode = messageModel.GameProviderCode,
                SendOn = messageModel.SendDateTime,
                ReceiveOn = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
    }
}
