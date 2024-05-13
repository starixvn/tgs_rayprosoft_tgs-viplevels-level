using Tgs.Payment.Clients;

namespace Tgs.VipLevels.Level.Core.ApplicationServices;

public record DepositFormCommand(
	Guid FormId,
	Guid BrandId,
	PaymentFlowState Status);
