using Tgs.GameBets.First.Worker.Common.Protos;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

public class GameBetsFirstWorkerVipLevelWorkerMessageHandler(
	ILogger<GameBetsFirstWorkerVipLevelWorkerMessageHandler> logger,
	ICommandService<TriggerGameWagerCommand> service) : IMessageHandler<GameBetsFirstWorkerVipLevelMessage>
{
	public ValueTask HandleAsync(string subject, GameBetsFirstWorkerVipLevelMessage data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			new
			{
				GameProviderId = data.GameProviderId.ToGuid(),
				data.GameId,
				data.PlayerCode,
				WalletId = data.WalletId.ToGuid(),
				data.ExternalRoundId,
				data.ExternalBetId,
				ExternalCreatedOn = data.ExternalCreatedOn.ToDateTimeOffset(),
				ClosedOn = data.ClosedOn.ToDateTimeOffset()
			});

		return service.ExecuteAsync(
			command: new TriggerGameWagerCommand(
				PlayerCode: data.PlayerCode),
			cancellationToken: cancellationToken);
	}
}
