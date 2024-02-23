using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

public class TriggerGameWagerMessageHandler(
	ILogger<TriggerGameWagerMessageHandler> logger,
	ICommandService<TriggerGameWagerCommand> service) : IMessageHandler<GameWagerPublish>
{
	public ValueTask HandleAsync(string subject, GameWagerPublish data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var loggerInfo = new
		{
			data.GameProviderCode,
			data.SubGameProviderCode,
			data.GameId,
			data.GameCategory,
			data.GameType,
			data.ResellerCode,
			data.LicenseeCode,
			data.BrandCode,
			data.PlayerCode,
			data.Username,
			data.CurrencyCode,
			data.ExternalRoundId,
			data.ExternalWagerId
		};

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			loggerInfo);

		return service.ExecuteAsync(
			command: new TriggerGameWagerCommand(
				PlayerCode: data.PlayerCode),
			cancellationToken: cancellationToken);
	}
}
