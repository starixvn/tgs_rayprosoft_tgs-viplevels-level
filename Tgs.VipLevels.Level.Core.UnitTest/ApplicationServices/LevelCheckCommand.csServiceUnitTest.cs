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
	/// <summary>
	/// 正常
	/// </summary>
	[Fact]
	public async Task Normal_Process()
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// 此玩家尚未達到等級
	/// </summary>
	[Fact]
	public async Task This_player_has_not_yet_reached_the_level()
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// 此玩家等級關閉自動升級功能
	/// </summary>
	[Fact]
	public async Task This_player_leveling_feature_has_been_disabled()
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// 此玩家已經最高等
	/// </summary>
	[Fact]
	public async Task This_player_is_already_the_highest_level()
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// 此玩家存款達標但有效投注未達標
	/// </summary>
	[Fact]
	public async Task This_player_is_deposit_reaches_the_limit_but_valid_bets_do_not_reach_the_limit()
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
			AccumDeposit: 10000,
			AccumTurnover: 10998,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// 此玩家有效投注達標但存款未達標
	/// </summary>
	[Fact]
	public async Task This_player_is_valid_bets_have_reached_the_limit_but_the_deposit_has_not_reached_the_limit()
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
			AccumDeposit: 9998,
			AccumTurnover: 11000,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
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
			subject: Arg.Is(VipLevelsLevelNatsConst.VipLevelsLevelUpgrade),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
		cancellationToken: Arg.Any<CancellationToken>());
	}
}
