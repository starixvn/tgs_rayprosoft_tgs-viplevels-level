using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using Tgs.VipLevelService.Common.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class LevelUpgradeCommandService(
	ILogger<LevelUpgradeCommandService> logger,
	IVipLevelServiceClient client,
	IVipLevelUtensils utensils,
	ICommandService<LevelUpgradeFormCreateCommand> service,
	IMessageSender messageSender) : ICommandService<LevelUpgradeCommand>
{
	public async ValueTask ExecuteAsync(LevelUpgradeCommand command, CancellationToken cancellationToken = default)
	{
		var playerVipLevelInfo = await client.PlayerVipLevelGetAsync(
			playerCode: command.PlayerCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (playerVipLevelInfo.AutoState == PlayerVipLevelAutoState.Disable)
		{
			logger.LogWarning("LogOn:{logOn}  Message:This player leveling feature has been disabled.({playerCode}|{autoState})",
				DateTime.Now.ToString("u"),
				playerVipLevelInfo.PlayerCode,
				playerVipLevelInfo.AutoState);
			return;
		}

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, _) = await utensils.GetConditionsAsync(
			playerVipLevelInfo.BrandId,
			playerVipLevelInfo.VipLevel,
			cancellationToken)
			.ConfigureAwait(false);

		if (Current.Level == Higher.Level)
		{
			logger.LogWarning("LogOn:{logOn}  Message:This player is already the highest level.({playerCode}|{level})",
				DateTime.Now.ToString("u"),
				playerVipLevelInfo.PlayerCode,
				playerVipLevelInfo.VipLevel);
			return;
		}

		if (Current.LevelUpAccumulateDeposit <= playerVipLevelInfo.AccumDeposit &&
			Current.LevelUpAccumulateBetAmount <= playerVipLevelInfo.AccumTurnover)
		{
			await client.PlayerVipLevelSystemAsync(
				playerCode: playerVipLevelInfo.PlayerCode,
				Higher.Level,
				Higher.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			await service.ExecuteAsync(
				command: new LevelUpgradeFormCreateCommand(
					PlayerCode: playerVipLevelInfo.PlayerCode,
					VipLevel: Higher.Level),
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			var message = new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			};

			await messageSender.PublishAsync(
				subject: VipLevelsLevelNatsConst.Check,
				data: message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			logger.LogInformation("LogOn:{logOn} Subject:{subject} Message:This message has been published.({message})",
				DateTime.Now.ToString("u"),
				VipLevelsLevelNatsConst.Check,
				message);
		}
	}
}
