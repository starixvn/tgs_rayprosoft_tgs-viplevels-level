using System.Text.Json;
using Microsoft.Extensions.Logging;

using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class DepositFormCommandService(
	ILogger<DepositFormCommandService> logger,
	TimeProvider timeProvider,
	ITriggerTimeUtnsil triggerTimeUtnsil,
	IPaymentServiceClient client,
	IPlayerVipLevelBacklogRepository repository) : ICommandService<DepositFormCommand>
{
	public async ValueTask ExecuteAsync(DepositFormCommand command, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Mesaage:This command information.({command})",
			DateTime.Now.ToString("o"),
			command);

		var depositFormInfo = await client.GetDepositFormAsync(
			command.FormId,
			cancellationToken)
			.ConfigureAwait(false)
			?? throw new NullReferenceException($"This deposit slip does not exist.({command.BrandId}|{command.FormId}|{command.Status})");

		logger.LogInformation("LogOn:{logOn} Mesaage:This deposit form information.({depositFormInfo})",
			DateTime.Now.ToString("o"),
			JsonSerializer.Serialize(depositFormInfo));

		var currentTime = timeProvider.GetUtcNow();
		var triggerAt = triggerTimeUtnsil.GetTriggerAt(currentTime);

		await repository.AddAsync(
			playerCode: depositFormInfo.PlayerCode,
			triggerAt: triggerAt,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}
