using Microsoft.AspNetCore.Mvc;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.ViewModels;
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
	/// <param name="source">The source.</param>
	/// <returns></returns>
	/// <remarks>
	/// <b>Example</b><br />
	///
	///     Post
	///         {
	///             "PlayerCode": "4um1qf1p0v3z"
	///         }
	/// </remarks>
	[HttpPost(Name = "Check_Execute")]
	public ValueTask ExecuteAsync(
		[FromServices] IMessageSender messageSender,
		[FromBody] VipLevelsLevelViewModel source)
	{
		var publish = new VipLevelsLevelMessage
		{
			PlayerCode = source.PlayerCode
		};

		return messageSender.PublishAsync(
			VipLevelsLevelNatsConst.Check,
			data: publish,
			cancellationToken: HttpContext.RequestAborted);
	}
}