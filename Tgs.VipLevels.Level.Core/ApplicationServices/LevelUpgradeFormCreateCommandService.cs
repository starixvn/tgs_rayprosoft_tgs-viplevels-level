using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class LevelUpgradeFormCreateCommandService(
	ILogger<LevelUpgradeFormCreateCommandService> logger,
	IVipLevelServiceClient vipLevelServiceClient) : ICommandService<LevelUpgradeFormCreateCommand>
{
	public async ValueTask ExecuteAsync(LevelUpgradeFormCreateCommand command, CancellationToken cancellationToken = default)
	{
		if (command.VipLevel == 1)
		{
			logger.LogWarning("LogOn:{logOn} Message:This player ({playerCode}) VIP level 1 will not distribute benefits.",
				DateTime.Now.ToString("u"),
				command.PlayerCode);
			return;
		}

		if (await vipLevelServiceClient.PlayerVipLevelUpgradeFormExistAsync(
			playerCode: command.PlayerCode,
			vipLevel: command.VipLevel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false))
		{
			logger.LogWarning("LogOn:{logOn} Message:This player ({playerCode})VIP level({vipLevel}) benefits have been distributed.",
				DateTime.Now.ToString("u"),
				command.PlayerCode,
				command.VipLevel);
			return;
		}

		var id = await vipLevelServiceClient.PlayerVipLevelUpgradeFormCreateAsync(
			playerCode: command.PlayerCode,
			vipLevel: command.VipLevel,
			operatorUserId: Guid.Empty,
			operatorUsername: "system",
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn}  Message:This player VIP level form was created successfully.({id})",
			DateTime.Now.ToString("u"),
			id);
	}
}
