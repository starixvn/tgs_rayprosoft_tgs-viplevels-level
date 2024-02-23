using Tgs.VipLevels.Level.Core.JobApplicationServices;

namespace Tgs.VipLevels.Level.Core;

public interface IHangfireUtensil
{
	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
