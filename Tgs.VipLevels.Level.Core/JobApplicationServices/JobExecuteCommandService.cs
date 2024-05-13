using System.Text;
using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Valhalla.MessageQueue;
using Tgs.VipLevels.Level.Core.JobApplicationServices;

namespace Tgs.VipLevels.Level.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	TimeProvider timeProvider,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{

		var message = new VipLevelsLevelTriggerMessage
		{
			TriggerAt = timeProvider.GetUtcNow().ToTimestamp()
		};

		logger.LogInformation("LogOn:{logOn} Publish Message:{message}", DateTime.Now.ToString("u"), message);

		return messageSender.PublishAsync<VipLevelsLevelTriggerMessage>(
			subject: VipLevelsLevelNatsConst.VipLevelsLevelTrigger,
			data: message,
			cancellationToken: cancellationToken);
	}
}
