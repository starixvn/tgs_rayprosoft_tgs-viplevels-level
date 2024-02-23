using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class TriggerGameWagerCommandService(
	ILogger<TriggerGameWagerCommandService> logger,
	TimeProvider timeProvider,
	ITriggerTimeUtnsil triggerTimeUtnsil,
	IPlayerVipLevelBacklogRepository repository) : ICommandService<TriggerGameWagerCommand>
{
	public ValueTask ExecuteAsync(TriggerGameWagerCommand command, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Mesaage:This command information.({command})",
			DateTime.Now.ToString("o"),
			command);

		var currentTime = timeProvider.GetUtcNow();
		var triggerAt = triggerTimeUtnsil.GetTriggerAt(currentTime);

		return repository.AddAsync(
			playerCode: command.PlayerCode,
			triggerAt: triggerAt,
			cancellationToken: cancellationToken);
	}
}
