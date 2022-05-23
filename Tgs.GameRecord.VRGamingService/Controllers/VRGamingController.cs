using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.VRGamingService.Core.ApplicationServices;
using Tgs.GameRecord.VRGamingService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.VRGamingService.Controllers
{
    [ApiController]
    [Route("VR")]
    public class VRGamingController : ControllerBase
    {
        [HttpPost]
        public async ValueTask<GameRecordResultViewModel> GameRecordAsync(
            [FromServices] IGetService<GameRecordQuery, GameRecordResult> service,
            GameRecordQueryViewModel model)
        {
            var result = await service.GetAsync(new GameRecordQuery()
            {
                BrandCode = model.BrandCode,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            }).ConfigureAwait(false);

            return new GameRecordResultViewModel()
            {
                Success = result.Success,
                LastGameRecordTime = result.LastGameRecordTime,
                SuccessCount = result.SuccessCount,
                FailCount = result.FailCount,
                Count = result.Count,
            };
        }
    }
}
