using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.ApplicationServices;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using Tgs.VipLevelService.Common.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.UnitTest.ApplicationServices;

public class UpgradeCheckCommandServiceUnitTest
{
	[Fact]
	public async void Normal_Process()
	{
		var fakeLog = NullLogger<UpgradeCheckCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelCheckCommand
		(
			PlayerCode: "4ucmqplxb029"
		);

		var nextVipLevelId = Guid.NewGuid();

		var nextVipLevel = 3;

		var playerVipLevelInfo = new PlayerVipLevelInfoViewModel(
			ResellerId: Guid.NewGuid(),
			ResellerCode: "Demo",
			LicenseeId: Guid.NewGuid(),
			LicenseeCode: "TGS",
			BrandId: Guid.NewGuid(),
			BrandCode: "GPI",
			PlayerCode: "4ucmqplxb029",
			Username: "test",
			VipLevelId: Guid.NewGuid(),
			VipLevel: 2,
			AutoState: PlayerVipLevelAutoState.Enable,
			AccumDeposit: 5000,
			AccumTurnover: 6000,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			false,
			NextVipLevelId: nextVipLevelId,
			NextVipLevel: nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new UpgradeCheckCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Upgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_has_not_yet_reached_the_level()
	{
		var fakeLog = NullLogger<UpgradeCheckCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelCheckCommand
		(
			PlayerCode: "4ucmqplxb029"
		);

		var nextVipLevelId = Guid.NewGuid();

		var nextVipLevel = 3;

		var playerVipLevelInfo = new PlayerVipLevelInfoViewModel(
			ResellerId: Guid.NewGuid(),
			ResellerCode: "Demo",
			LicenseeId: Guid.NewGuid(),
			LicenseeCode: "TGS",
			BrandId: Guid.NewGuid(),
			BrandCode: "GPI",
			PlayerCode: "4ucmqplxb029",
			Username: "test",
			VipLevelId: Guid.NewGuid(),
			VipLevel: 2,
			AutoState: PlayerVipLevelAutoState.Enable,
			AccumDeposit: 4989,
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			FullLevel: false,
			NextVipLevelId: nextVipLevelId,
			NextVipLevel: nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new UpgradeCheckCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Upgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_leveling_feature_has_been_disabled()
	{
		var fakeLog = NullLogger<UpgradeCheckCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelCheckCommand
		(
			PlayerCode: "4ucmqplxb029"
		);

		var nextVipLevelId = Guid.NewGuid();

		var nextVipLevel = 3;

		var playerVipLevelInfo = new PlayerVipLevelInfoViewModel(
			ResellerId: Guid.NewGuid(),
			ResellerCode: "Demo",
			LicenseeId: Guid.NewGuid(),
			LicenseeCode: "TGS",
			BrandId: Guid.NewGuid(),
			BrandCode: "GPI",
			PlayerCode: "4ucmqplxb029",
			Username: "test",
			VipLevelId: Guid.NewGuid(),
			VipLevel: 2,
			AutoState: PlayerVipLevelAutoState.Disable,
			AccumDeposit: 4989,
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			FullLevel: false,
			NextVipLevelId: nextVipLevelId,
			NextVipLevel: nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new UpgradeCheckCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Upgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_is_already_the_highest_level()
	{
		var fakeLog = NullLogger<UpgradeCheckCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelCheckCommand
		(
			PlayerCode: "4ucmqplxb029"
		);

		var vipLevelId = Guid.NewGuid();
		var vipLevel = 3;

		var playerVipLevelInfo = new PlayerVipLevelInfoViewModel(
			ResellerId: Guid.NewGuid(),
			ResellerCode: "Demo",
			LicenseeId: Guid.NewGuid(),
			LicenseeCode: "TGS",
			BrandId: Guid.NewGuid(),
			BrandCode: "GPI",
			PlayerCode: "4ucmqplxb029",
			Username: "test",
			VipLevelId: vipLevelId,
			VipLevel: vipLevel,
			AutoState: PlayerVipLevelAutoState.Enable,
			AccumDeposit: 4989,
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			FullLevel: true,
			NextVipLevelId: vipLevelId,
			NextVipLevel: vipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new UpgradeCheckCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Upgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}
}
