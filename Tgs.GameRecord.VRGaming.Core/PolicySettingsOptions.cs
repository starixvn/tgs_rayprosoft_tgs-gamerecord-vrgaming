namespace Tgs.GameRecord.VRGaming.Core;
public record PolicySettingsOptions
{
	public int RetryCount { get; init; } = 3;
	public int RetrySeconds { get; init; } = 2;
}
