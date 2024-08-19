using Tgs.GameRecord.VRGaming.Core.JobWorkerServices;

namespace Tgs.GameRecord.VRGaming.Core;

public interface IHangfireUtensilRepository
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
