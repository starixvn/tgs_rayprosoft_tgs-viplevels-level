using Tgs.VipLevelService.Common.ViewModels;

namespace Tgs.VipLevels.Level.Core;

public interface IVipLevelUtensils
{
	ValueTask<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)> GetConditionsAsync(Guid brandId, int level, CancellationToken cancellationToken = default);

	ValueTask<VipLevelInfoViewModel> GetVipLevelInfoAsync(Guid snapshotId, int level, CancellationToken cancellationToken = default);

	ValueTask<(decimal CurrentDeposit, decimal CurrentTurnover)> GetCurrentExpAsync(Guid accumDepositId, Guid accumTurnoverId, CancellationToken cancellationToken = default);
}