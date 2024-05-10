namespace Tgs.GameRecord.VRGaming.Core.JobWorkerServices;

public record JobExecuteCommand(
	Uri ApiUrl,
	string Version,
	string MerchantId,
	string Deskey);
