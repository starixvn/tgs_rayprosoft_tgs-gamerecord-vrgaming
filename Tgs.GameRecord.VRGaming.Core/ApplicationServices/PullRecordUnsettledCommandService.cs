using Microsoft.Extensions.Logging;
using Tgs.GameInterface.VRGaming.Client;
using Tgs.GameInterface.VRGaming.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGaming.Core.ApplicationServices;

internal class PullRecordUnsettledCommandService(
    ILogger<PullRecordUnsettledCommandService> logger,
    IGameInterfaceVRGamingClient client,
    ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordUnsettledCommand>
{
    public async ValueTask ExecuteAsync(PullRecordUnsettledCommand command, CancellationToken cancellationToken = default)
    {
        var pageNumber = 1;
        var pageSize = 1000;
        try
        {
            var infos = Enumerable.Empty<GameWagerInfo>();

            do
            {
                infos = await client.GameWagerUnsettledAsync(
                    apiUrl: command.ApiUrl,
                    version: command.Version,
                    merchantId: command.MerchantId,
                    deskey: command.Deskey,
                    startTime: command.StartTime,
                    endTime: command.EndTime,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    cancellationToken: cancellationToken)
                    .ToArrayAsync(
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false)
                    ?? Enumerable.Empty<GameWagerInfo>();

                foreach (var info in infos)
                {
                    try
                    {
                        await service.ExecuteAsync(
                            command: new HandleRecordCommand(info),
                            cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({info})", DateTime.Now.ToString("o"), info);
                    }
                }

                pageNumber++;
            } while (infos.Any());

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogOn:{logOn} Message: An unknown error occurred.({command})", DateTime.Now.ToString("o"), command);
        }
    }
}
