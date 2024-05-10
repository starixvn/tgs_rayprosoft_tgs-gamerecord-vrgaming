using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.VRGaming.Common;

public record VRGamingGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string Version { get; init; } = default!;
	[Required]
	public string MerchantId { get; init; } = default!;
	[Required]
	public string Deskey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; }
}
