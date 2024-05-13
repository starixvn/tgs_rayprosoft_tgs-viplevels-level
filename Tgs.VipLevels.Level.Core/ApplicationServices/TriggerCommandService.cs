using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.Models;
using Valhalla.MessageQueue;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class TriggerCommandService(
	ILogger<TriggerCommandService> logger,
	IPlayerVipLevelBacklogRepository repository,
	IMessageSender messageSender) : ICommandService<TriggerCommand>
{
	public async ValueTask ExecuteAsync(TriggerCommand command, CancellationToken cancellationToken = default)
	{
		IEnumerable<PlayerVipLevelBacklogInfo>? infos;
		var offset = 0;
		var limit = 100;

		do
		{
			infos = await repository.QueryAsync(
				endOn: command.TriggerAt,
				offset: offset,
				limit: limit,
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<PlayerVipLevelBacklogInfo>();

			logger.LogInformation("LogOn:{logOn} Mesaage:This gets the VipLevel pending player.({infos})",
				DateTime.Now.ToString("o"),
				infos);

			foreach (var info in infos)
			{
				await messageSender.PublishAsync(
					VipLevelsLevelNatsConst.VipLevelsLevelCheck,
					data: new VipLevelsLevelMessage
					{
						PlayerCode = info.PlayerCode
					},
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				await repository.DeleteAsync(
					playerCode: info.PlayerCode,
					triggerAt: info.TriggerAt,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			offset += 1;
		} while (infos.Any());
	}
}
