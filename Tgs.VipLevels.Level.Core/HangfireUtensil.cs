using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core.JobApplicationServices;

namespace Tgs.VipLevels.Level.Core;

internal class HangfireUtensil(
	ICommandService<JobExecuteCommand> jobExecuteService,
	ICommandService<JobInitCommand> jobInitservice) : IHangfireUtensil
{
	public Task JobExecuteAsync(JobExecuteCommand source)
		=> jobExecuteService.ExecuteAsync(source).AsTask();

	public Task JobInitAsync()
		=> jobInitservice.ExecuteAsync(new()).AsTask();
}
