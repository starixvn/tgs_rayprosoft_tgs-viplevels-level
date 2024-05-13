using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

internal class LevelTriggerMessageHandler(
	ILogger<LevelTriggerMessageHandler> logger,
	ICommandService<LevelTriggerCommand> service) : IMessageHandler<VipLevelsLevelTriggerMessage>
{
	public ValueTask HandleAsync(string subject, VipLevelsLevelTriggerMessage data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new LevelTriggerCommand(
				TriggerAt: data.TriggerAt.ToDateTimeOffset()),
			cancellationToken: cancellationToken);
	}
}
