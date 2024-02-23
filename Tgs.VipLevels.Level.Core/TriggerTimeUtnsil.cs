using Tgs.VipLevels.Level.Core;

namespace Tgs.VipLevels.Level.Core;

internal class TriggerTimeUtnsil : ITriggerTimeUtnsil
{
	public DateTimeOffset GetTriggerAt(DateTimeOffset source)
	{
		var utc = source.UtcDateTime;
		var mins = utc.Minute % 5;
		var temp = utc.AddMinutes(-mins);
		return new DateTimeOffset(temp.Year, temp.Month, temp.Day, temp.Hour, temp.Minute, 00, TimeSpan.Zero).AddMinutes(6);
	}
}
