using Tgs.GameWagers.Stat.Worker.Common.Protos;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

public class TriggerGameWagerMessageHandler(
	ILogger<TriggerGameWagerMessageHandler> logger,
	ICommandService<TriggerGameWagerCommand> service) : IMessageHandler<GameWagersFirstsMessage>
{
	public ValueTask HandleAsync(string subject, GameWagersFirstsMessage data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new TriggerGameWagerCommand(
				PlayerCode: data.PlayerCode),
			cancellationToken: cancellationToken);
	}
}
