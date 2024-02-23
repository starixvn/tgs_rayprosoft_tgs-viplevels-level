using Microsoft.AspNetCore.Mvc;
using Tgs.VipLevels.Level.Core;

namespace Tgs.VipLevels.Level.Controllers;

[Route("[controller]")]
[ApiController]
public class TriggerController : ControllerBase
{
	/// <summary>
	/// Executes the asynchronous.
	/// </summary>
	/// <param name="repository">The repository.</param>
	/// <param name="timeProvider">The time provider.</param>
	/// <param name="triggerTimeUtnsil">The trigger time utnsil.</param>
	/// <param name="playerCode">The player code.</param>
	/// <returns></returns>
	[HttpPost("{playerCode}", Name = "Trigger_Execute")]
	public ValueTask ExecuteAsync(
		[FromServices] IPlayerVipLevelBacklogRepository repository,
		[FromServices] TimeProvider timeProvider,
		[FromServices] ITriggerTimeUtnsil triggerTimeUtnsil,
		string playerCode)
	{
		var currentTime = timeProvider.GetUtcNow();
		var triggerAt = triggerTimeUtnsil.GetTriggerAt(currentTime);

		return repository.AddAsync(
			playerCode: playerCode,
			triggerAt: triggerAt,
			cancellationToken: HttpContext.RequestAborted);
	}
}
