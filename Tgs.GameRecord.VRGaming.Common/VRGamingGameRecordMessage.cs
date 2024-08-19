using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.VRGaming.Common;

public record VRGamingGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://fe.vrbetdemo.com/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "1.0")]
	public string Version { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "PEGA")]
	public string MerchantId { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "8VF408JBV2B406488F2VH2X684T6R6DV")]
	public string Deskey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-12-25T00:00:00+08:00")]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-12-26T00:10:00+08:00")]
	public DateTimeOffset EndTime { get; init; }
}
