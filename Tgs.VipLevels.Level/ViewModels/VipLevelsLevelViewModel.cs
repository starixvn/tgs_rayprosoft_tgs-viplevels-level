using System.ComponentModel.DataAnnotations;

namespace Tgs.VipLevels.Level.ViewModels;

public record VipLevelsLevelViewModel
{
	/// <summary>
	/// The player code/
	/// </summary>
	[Required]
	public string PlayerCode { get; init; } = default!;
}