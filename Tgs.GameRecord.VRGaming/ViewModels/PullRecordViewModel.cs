using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.VRGaming.ViewModels;

public record PullRecordViewModel
{
    [Required]
    public Uri ApiRecordUrl { get; init; } = default!;
    [Required]
    public string Deskey { get; init; } = default!;
    [Required]
    public string Md5key { get; init; } = default!;
    [Required]
    public string AgentId { get; init; } = default!;
    [Required]
    public DateTimeOffset StartTime { get; init; }
    [Required]
    public DateTimeOffset EndTime { get; init; }
}
