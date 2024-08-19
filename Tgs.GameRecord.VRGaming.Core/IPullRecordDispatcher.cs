using Tgs.GameRecord.VRGaming.Core.ApplicationServices;

namespace Tgs.GameRecord.VRGaming.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}
