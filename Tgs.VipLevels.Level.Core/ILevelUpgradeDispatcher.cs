using Tgs.VipLevels.Level.Core.ApplicationServices;

namespace Tgs.VipLevels.Level.Core;

public interface ILevelUpgradeDispatcher
{
	ValueTask PushCommandAsync(string playerCode, LevelUpgradeCommand command, CancellationToken cancellationToken = default);

	void ExtendExpiry(string playerCode);
}