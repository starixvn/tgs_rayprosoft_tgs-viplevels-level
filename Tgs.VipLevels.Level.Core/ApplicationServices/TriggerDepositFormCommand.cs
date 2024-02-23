using Tgs.Payment.Clients;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

public record TriggerDepositFormCommand(
	Guid FormId,
	Guid BrandId,
	PaymentFlowState Status);
