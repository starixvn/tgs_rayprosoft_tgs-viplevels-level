using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using Tgs.VipLevelService.Common.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class UpgradeCheckCommandService(
	ILogger<UpgradeCheckCommandService> logger,
	IVipLevelServiceClient vipLevelServiceClient,
	IVipLevelUtensils vipLevelUtensils,
	IMessageSender messageSender) : ICommandService<LevelCheckCommand>
{
	public async ValueTask ExecuteAsync(LevelCheckCommand command, CancellationToken cancellationToken = default)
	{
		var playerVipLevelInfo = await vipLevelServiceClient.PlayerVipLevelGetAsync(
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

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, _) = await vipLevelUtensils.GetConditionsAsync(
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

		(decimal CurrentDeposit, decimal CurrentTurnover) = await vipLevelUtensils.GetCurrentExpAsync(
			accumDepositId: playerVipLevelInfo.AccumDepositId,
			accumTurnoverId: playerVipLevelInfo.AccumTurnoverId,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (Current.LevelUpAccumulateDeposit <= CurrentDeposit &&
			Current.LevelUpAccumulateBetAmount <= CurrentTurnover)
		{
			var message = new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			};

			await messageSender.PublishAsync(
				subject: VipLevelsLevelNatsConst.Upgrade,
				data: message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			logger.LogInformation("LogOn:{logOn} Subject:{subject} Message:This message has been published.({message})",
				DateTime.Now.ToString("u"),
				VipLevelsLevelNatsConst.Upgrade,
				message);
		}
	}
}