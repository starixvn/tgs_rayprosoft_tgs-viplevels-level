using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.ApplicationServices;
using Tgs.VipLevels.Upgrade.Common;
using Tgs.VipLevels.Upgrade.Common.Protos;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using Tgs.VipLevelService.Common.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.UnitTest.ApplicationServices;

public class LevelUpgradeCommandServiceUnitTest
{
	[Fact]
	public async void Normal_Process()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelUpgradeCommand
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
			AccumDepositId: Guid.NewGuid(),
			AccumDeposit: 4989,
			AccumTurnoverId: Guid.NewGuid(),
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

		var current = new VipLevelInfoViewModel(
			Id: playerVipLevelInfo.VipLevelId,
			Level: 2,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 199.0m,
			PromotionBenefit: 188.0m,
			QuarterlyBenefit: 166.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var higher = new VipLevelInfoViewModel(
			Id: Guid.NewGuid(),
			Level: 3,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 299.0m,
			PromotionBenefit: 288.0m,
			QuarterlyBenefit: 266.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var lower = new VipLevelInfoViewModel(
			Id: Guid.NewGuid(),
			Level: 1,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 1999,
			LevelUpAccumulateDeposit: 999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 99.0m,
			PromotionBenefit: 88.0m,
			QuarterlyBenefit: 66.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		_ = fakeVipLevelUtensils.GetConditionsAsync(
			brandId: Arg.Is(playerVipLevelInfo.BrandId),
			level: Arg.Is(playerVipLevelInfo.VipLevel),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)>((current, higher, lower));

		decimal currentDeposit = 5001m;
		decimal CurrentTurnover = 6001m;
		_ = fakeVipLevelUtensils.GetCurrentExpAsync(
			accumDepositId: Arg.Is(playerVipLevelInfo.AccumDepositId),
			accumTurnoverId: Arg.Is(playerVipLevelInfo.AccumTurnoverId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns<(decimal CurrentDeposit, decimal CurrentTurnover)>((currentDeposit, CurrentTurnover));

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(1)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(VipLevelsUpgradeNatsConst.Create),
			data: Arg.Is(new VipLevelsUpgradeMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode,
				VipLevel = higher.Level,
			}),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Check),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_has_not_yet_reached_the_level()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelUpgradeCommand
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
			AccumDepositId: Guid.NewGuid(),
			AccumDeposit: 4989,
			AccumTurnoverId: Guid.NewGuid(),
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			false,
			nextVipLevelId,
			nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var current = new VipLevelInfoViewModel(
			Id: playerVipLevelInfo.VipLevelId,
			Level: 2,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 199.0m,
			PromotionBenefit: 188.0m,
			QuarterlyBenefit: 166.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var higher = new VipLevelInfoViewModel(
			Id: Guid.NewGuid(),
			Level: 3,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 299.0m,
			PromotionBenefit: 288.0m,
			QuarterlyBenefit: 266.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var lower = new VipLevelInfoViewModel(
			Id: Guid.NewGuid(),
			Level: 1,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 1999,
			LevelUpAccumulateDeposit: 999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 99.0m,
			PromotionBenefit: 88.0m,
			QuarterlyBenefit: 66.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		_ = fakeVipLevelUtensils.GetConditionsAsync(
			brandId: Arg.Is(playerVipLevelInfo.BrandId),
			level: Arg.Is(playerVipLevelInfo.VipLevel),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)>((current, higher, lower));

		decimal currentDeposit = 4998m;
		decimal CurrentTurnover = 6001m;
		_ = fakeVipLevelUtensils.GetCurrentExpAsync(
			accumDepositId: Arg.Is(playerVipLevelInfo.AccumDepositId),
			accumTurnoverId: Arg.Is(playerVipLevelInfo.AccumTurnoverId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns<(decimal CurrentDeposit, decimal CurrentTurnover)>((currentDeposit, CurrentTurnover));

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsUpgradeNatsConst.Create),
			data: Arg.Is(new VipLevelsUpgradeMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode,
				VipLevel = higher.Level,
			}),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Check),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_leveling_feature_has_been_disabled()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelUpgradeCommand
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
			AccumDepositId: Guid.NewGuid(),
			AccumDeposit: 4989,
			AccumTurnoverId: Guid.NewGuid(),
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			false,
			nextVipLevelId,
			nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: Arg.Any<int>(),
			vipLevelId: Arg.Any<Guid>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsUpgradeNatsConst.Create),
			data: Arg.Any<VipLevelsUpgradeMessage>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Check),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async void This_player_is_already_the_highest_level()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new LevelUpgradeCommand
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
			VipLevelId: nextVipLevelId,
			VipLevel: nextVipLevel,
			AutoState: PlayerVipLevelAutoState.Enable,
			AccumDepositId: Guid.NewGuid(),
			AccumDeposit: 4989,
			AccumTurnoverId: Guid.NewGuid(),
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			FullLevel: true,
			NextVipLevelId: nextVipLevelId,
			NextVipLevel: nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var current = new VipLevelInfoViewModel(
			Id: nextVipLevelId,
			Level: nextVipLevel,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 299.0m,
			PromotionBenefit: 288.0m,
			QuarterlyBenefit: 266.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var higher = new VipLevelInfoViewModel(
			Id: nextVipLevelId,
			Level: nextVipLevel,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 299.0m,
			PromotionBenefit: 288.0m,
			QuarterlyBenefit: 266.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		var lower = new VipLevelInfoViewModel(
			Id: Guid.NewGuid(),
			Level: 2,
			BrandId: playerVipLevelInfo.BrandId,
			LevelUpAccumulateBetAmount: 5999,
			LevelUpAccumulateDeposit: 4999,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 199.0m,
			PromotionBenefit: 188.0m,
			QuarterlyBenefit: 166.0m,
			LiveDealerRebate: 0.0m,
			SlotRebate: 0.0m,
			LotteryRebate: 0.0m,
			CardGameRebate: 0.0m,
			SportsRebate: 0.0m,
			VirtualSportsRebate: 0.0m,
			WithdrawTransactionsPerDay: -1,
			WithdrawTotalAmountPerDay: -1);
		_ = fakeVipLevelUtensils.GetConditionsAsync(
			brandId: Arg.Is(playerVipLevelInfo.BrandId),
			level: Arg.Is(playerVipLevelInfo.VipLevel),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns<(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower)>((current, higher, lower));

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsUpgradeNatsConst.Create),
			data: Arg.Any<VipLevelsUpgradeMessage>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(VipLevelsLevelNatsConst.Check),
			data: Arg.Is(new VipLevelsLevelMessage
			{
				PlayerCode = playerVipLevelInfo.PlayerCode
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}