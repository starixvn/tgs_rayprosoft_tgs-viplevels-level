using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.VipLevels.Level.Core.UnitTest;
public class TriggerTimeUtnsilUnitTest
{
	[Fact]
	public void DateTimeOffset_24_02_22_11_49_59_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 11, 49, 59, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 11, 51, 00, TimeSpan.FromHours(+8)), actual);
	}
	[Fact]
	public void DateTimeOffset_24_02_22_11_50_59_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 11, 50, 59, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 11, 56, 00, TimeSpan.FromHours(+8)), actual);
	}

	[Fact]
	public void DateTimeOffset_24_02_22_11_54_59_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 11, 54, 59, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 11, 56, 00, TimeSpan.FromHours(+8)), actual);
	}

	[Fact]
	public void DateTimeOffset_24_02_22_11_55_00_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 11, 55, 00, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 12, 01, 00, TimeSpan.FromHours(+8)), actual);
	}

	[Fact]
	public void DateTimeOffset_24_02_22_11_59_59_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 11, 59, 59, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 12, 01, 00, TimeSpan.FromHours(+8)), actual);
	}

	[Fact]
	public void DateTimeOffset_24_02_22_12_00_00_08()
	{
		var source = new DateTimeOffset(2024, 02, 22, 12, 00, 00, TimeSpan.FromHours(+8));

		var utnsil = new TriggerTimeUtnsil();

		var actual = utnsil.GetTriggerAt(source);

		Assert.Equal(new DateTimeOffset(2024, 02, 22, 12, 06, 00, TimeSpan.FromHours(+8)), actual);
	}
}
