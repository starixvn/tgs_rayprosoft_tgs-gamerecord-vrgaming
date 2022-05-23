using Microsoft.Extensions.Logging;
using Tgs.GameRecord.VRGamingService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService.Core.JobWorkerServices
{
    internal class JobWorkerCommandService : ICommandService<JobWorkerCommand>
    {
        private readonly IGameRecordSchedule _gameRecordScheduleService;
        private readonly IGetService<GameRecordQuery, GameRecordResult> _getService;
        private readonly ILogger<JobWorkerCommandService> _logger;
        private readonly ISystemClock _systemClock;
        private readonly IUtensil _utensil;

        public JobWorkerCommandService(
            IUtensil utensil,
            IGameRecordSchedule gameRecordScheduleService,
            ISystemClock systemClock,
            ILogger<JobWorkerCommandService> logger,
            IGetService<GameRecordQuery, GameRecordResult> getService)
        {
            _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
            _gameRecordScheduleService = gameRecordScheduleService ?? throw new ArgumentNullException(nameof(gameRecordScheduleService));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getService = getService ?? throw new ArgumentNullException(nameof(getService));
        }

        public async ValueTask ExecuteAsync(JobWorkerCommand command, CancellationToken cancellationToken = default)
        {
            var configuration = await _utensil.GetSettingConfigurationAsync().ConfigureAwait(false);

            var gameProviderId = await _utensil.GetGameProviderIdAsync().ConfigureAwait(false);

            var brandMerchants = configuration.BrandMerchants.Where(x => x.Status);
            foreach (var brand in brandMerchants)
            {
                try
                {
                    var sysTime = _systemClock.CurrentTime;

                    var gameProviderSchedule =
                        await _gameRecordScheduleService.GetAsync(
                            brand.BrandCode,
                            brand.Merchantid,
                            cancellationToken).ConfigureAwait(false)
                            ?? await _gameRecordScheduleService.InitAsync(
                                gameProviderId,
                                brand.BrandCode,
                                brand.Merchantid,
                                cancellationToken).ConfigureAwait(false);

                    while (gameProviderSchedule != null)
                    {
                        var config = _gameRecordScheduleService.GetConfig(gameProviderSchedule);
                        if (config.EndTime >= sysTime)
                        {
                            break;
                        }

                        var result = await _getService.GetAsync(new GameRecordQuery()
                        {
                            BrandCode = brand.BrandCode,
                            StartTime = config.StartTime,
                            EndTime = config.EndTime
                        }, cancellationToken).ConfigureAwait(false);
                        if (result.Success)
                        {
                            gameProviderSchedule = await _gameRecordScheduleService.UpdateAsync(gameProviderSchedule, result.LastGameRecordTime, sysTime, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"發生例外");
                }
            }
        }
    }
}
