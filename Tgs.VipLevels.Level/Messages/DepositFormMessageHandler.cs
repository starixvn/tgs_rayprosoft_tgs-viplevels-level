using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Messages;

internal class DepositFormMessageHandler(
	ILogger<DepositFormMessageHandler> logger,
	ICommandService<DepositFormCommand> service) : IMessageHandler<FormEventEx>
{
	public ValueTask HandleAsync(string subject, FormEventEx data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new DepositFormCommand(
				FormId: data.FormId,
				BrandId: data.BrandId,
				Status: (PaymentFlowState)Enum.Parse(typeof(PaymentFlowState), data.Status)),
			cancellationToken: cancellationToken);
	}
}
