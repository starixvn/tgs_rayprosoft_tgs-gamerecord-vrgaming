namespace Tgs.GameRecord.VRGamingService.Core
{
    public interface ISystemClock
    {
        DateTimeOffset CurrentTime { get; }
    }
}
