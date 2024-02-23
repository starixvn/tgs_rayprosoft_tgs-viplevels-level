using Tgs.VipLevels.Level.Core.Models;

namespace Tgs.VipLevels.Level.Core;

public interface IPlayerVipLevelBacklogRepository
{
	ValueTask AddAsync(string playerCode, DateTimeOffset triggerAt, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerVipLevelBacklogInfo> QueryAsync(DateTimeOffset endOn, int offset = 0, int limit = 20, CancellationToken cancellationToken = default);

	ValueTask UpdateExpireOnAsync(string playerCode, DateTimeOffset triggerAt, CancellationToken cancellationToken = default);
}
