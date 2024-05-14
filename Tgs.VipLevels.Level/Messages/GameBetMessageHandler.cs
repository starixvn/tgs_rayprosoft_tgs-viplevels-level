using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

public class GameBetMessageHandler(
	ILogger<GameBetMessageHandler> logger,
	ICommandService<GameBetCommand> service) : IMessageHandler<GameBets.Worker.Common.Protos.GameBetsWorkerPublish>
{
	public ValueTask HandleAsync(string subject, GameBets.Worker.Common.Protos.GameBetsWorkerPublish data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			new
			{
				GameBetId = data.GameBetId.ToGuid(),
				GameProviderId = data.GameProviderId.ToGuid(),
				data.GameId,
				ResellerId = data.ResellerId.ToGuid(),
				LicenseeId = data.LicenseeId.ToGuid(),
				BrandId = data.BrandId.ToGuid(),
				data.PlayerCode,
				data.CurrencyCode,
				WalletId = data.WalletId.ToGuid(),
				data.ExternalRoundId,
				data.ExternalBetId,
				ExternalCreatedOn = data.ExternalCreatedOn.ToDateTimeOffset()
			});

		return service.ExecuteAsync(
			command: new GameBetCommand(
				PlayerCode: data.PlayerCode),
			cancellationToken: cancellationToken);
	}
}
