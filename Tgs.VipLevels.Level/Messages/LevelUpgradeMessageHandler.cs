using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

internal class LevelUpgradeMessageHandler(
	ILogger<LevelUpgradeMessageHandler> logger,
	ILevelUpgradeDispatcher dispatcher) : IMessageHandler<VipLevelsLevelMessage>
{
	public ValueTask HandleAsync(string subject, VipLevelsLevelMessage data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return dispatcher.PushCommandAsync(
			playerCode: data.PlayerCode,
			command: new LevelUpgradeCommand(
				PlayerCode: data.PlayerCode),
			cancellationToken: cancellationToken);
	}
}
