using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.Models;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

internal class TriggerCommandService(
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

			foreach (var info in infos)
			{
				await messageSender.PublishAsync(
					VipLevelsLevelNatsConst.Check,
					data: new VipLevelsLevelMessage
					{
						PlayerCode = info.PlayerCode
					},
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				await repository.UpdateExpireOnAsync(
					playerCode: info.PlayerCode,
					triggerAt: info.TriggerAt,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			offset += 1;
		} while (infos.Any());
	}
}
