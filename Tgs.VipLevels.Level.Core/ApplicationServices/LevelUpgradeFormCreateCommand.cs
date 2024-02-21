namespace Tgs.VipLevels.Level.Core.ApplicationServices;
public record LevelUpgradeFormCreateCommand(
	string PlayerCode,
	int VipLevel);
