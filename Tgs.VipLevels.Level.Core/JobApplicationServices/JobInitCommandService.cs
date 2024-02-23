using System.Runtime.InteropServices;
using Hangfire;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Core;
using Tgs.VipLevels.Level.Core.JobApplicationServices;

namespace Tgs.VipLevels.Level.Core.JobApplicationServices;

internal class JobInitCommandService : ICommandService<JobInitCommand>
{
	private readonly ILogger<JobInitCommandService> _logger;
	private readonly RecurringJobOptions _recurringJobOptions;

	public JobInitCommandService(
		ILogger<JobInitCommandService> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_recurringJobOptions = new RecurringJobOptions()
		{
			TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
		};
	}

	public ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
	{
		try
		{

			var jobExecuteCommand = new JobExecuteCommand();

			RecurringJob.AddOrUpdate<IHangfireUtensil>(
				recurringJobId: "Trigger Level",
				methodCall: service => service.JobExecuteAsync(new JobExecuteCommand()),
				cronExpression: Cron.Minutely(),
				options: _recurringJobOptions);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "LogOn:{logOn} Message:初始化排程發生例外", DateTime.Now.ToString("o"));
		}

		return ValueTask.CompletedTask;
	}
}
