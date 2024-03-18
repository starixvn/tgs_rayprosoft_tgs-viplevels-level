using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace Tgs.VipLevels.Level.Core;

internal class VipLevelUtensils(
	IVipLevelServiceClient vipLevelServiceClient) : IVipLevelUtensils
{
	public async ValueTask<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)> GetConditionsAsync(Guid brandId, int level, CancellationToken cancellationToken = default)
	{
		var vipLevelInfos = await vipLevelServiceClient.VipLevelQueryAsync(
			brandId: brandId,
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
}
