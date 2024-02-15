using Tgs.Calculate.Clients;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace Tgs.VipLevels.Level.Core;

internal class VipLevelUtensils(
	IVipLevelServiceClient vipLevelServiceClient,
	ICalculateClient calculateClient) : IVipLevelUtensils
{
	public async ValueTask<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)> GetConditionsAsync(Guid brandId, int level, CancellationToken cancellationToken = default)
	{
		var snapshotId = await vipLevelServiceClient.VipLevelSnapshotCurrentSnapshotIdAsync(
			brandId: brandId,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var vipLevelInfos = await vipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: snapshotId,
			cancellationToken: cancellationToken)
			.ToArrayAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<VipLevelInfoViewModel>();

		var current = vipLevelInfos.FirstOrDefault(predicate: vl => vl.Level == level)
			?? throw new NullReferenceException();
		var lower = vipLevelInfos.OrderBy(keySelector: vl => vl.Level).First();
		var highest = vipLevelInfos.OrderByDescending(keySelector: vl => vl.Level).First();

		highest = vipLevelInfos.Where(predicate: vl => vl.Level > level).FirstOrDefault() ?? highest;
		lower = vipLevelInfos.Where(predicate: vl => vl.Level < level).LastOrDefault() ?? lower;

		return new(current, highest, lower);
	}

	public async ValueTask<(decimal CurrentDeposit, decimal CurrentTurnover)> GetCurrentExpAsync(Guid accumDepositId, Guid accumTurnoverId, CancellationToken cancellationToken = default)
	{
		var currentDeposit = await calculateClient.GetBalanceAsync(
			calculateId: accumDepositId,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var currentTurnover = await calculateClient.GetBalanceAsync(
			calculateId: accumTurnoverId,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new(currentDeposit.PostBalance, currentTurnover.PostBalance);
	}

	public async ValueTask<VipLevelInfoViewModel> GetVipLevelInfoAsync(Guid snapshotId, int level, CancellationToken cancellationToken = default)
	{
		var vipLevelInfos = await vipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: snapshotId,
			cancellationToken: cancellationToken)
			.ToArrayAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<VipLevelInfoViewModel>();

		return vipLevelInfos.FirstOrDefault(predicate: vl => vl.Level == level) ?? throw new NullReferenceException();
	}
}