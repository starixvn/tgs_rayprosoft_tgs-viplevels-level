using Microsoft.AspNetCore.Mvc;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Controllers;

[Route("[controller]")]
[ApiController]
public class CheckController : ControllerBase
{
	/// <summary>
	/// Executes the asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="playerCode">The player code.</param>
	/// <returns></returns>
	[HttpPost("{playerCode}", Name = "Check_Execute")]
	public ValueTask ExecuteAsync(
		[FromServices] IMessageSender messageSender,
		string playerCode)
	{
		var publish = new VipLevelsLevelMessage
		{
			PlayerCode = playerCode
		};

		return messageSender.PublishAsync(
			VipLevelsLevelNatsConst.Check,
			data: publish,
			cancellationToken: HttpContext.RequestAborted);
	}
}
