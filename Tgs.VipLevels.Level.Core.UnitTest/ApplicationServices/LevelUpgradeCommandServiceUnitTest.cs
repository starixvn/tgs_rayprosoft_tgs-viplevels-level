using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.ServiceUtility;
using Tgs.VipLevels.Level.Common;
using Tgs.VipLevels.Level.Common.Protos;
using Tgs.VipLevels.Level.Core.ApplicationServices;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using Tgs.VipLevelService.Common.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.VipLevels.Level.Core.UnitTest.ApplicationServices;

public class LevelUpgradeCommandServiceUnitTest
{
	/// <summary>
	/// 正常
	/// </summary>
	[Fact]
	public async void Normal_Process()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 10000,
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
			LevelUpAccumulateBetAmount: 6001m,
			LevelUpAccumulateDeposit: 5001m,
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

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(1)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(new LevelUpgradeFormCreateCommand(
				PlayerCode: playerVipLevelInfo.PlayerCode,
				VipLevel: higher.Level)),
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

	/// <summary>
	/// 此玩家尚未達到等級
	/// </summary>
	[Fact]
	public async void This_player_has_not_yet_reached_the_level()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 4989,
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
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
			LevelUpAccumulateBetAmount: 6001m,
			LevelUpAccumulateDeposit: 4998m,
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

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Is(new LevelUpgradeFormCreateCommand(
				PlayerCode: playerVipLevelInfo.PlayerCode,
				VipLevel: higher.Level)),
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

	/// <summary>
	/// 此玩家等級關閉自動升級功能
	/// </summary>
	[Fact]
	public async void This_player_leveling_feature_has_been_disabled()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 4989,
			AccumTurnover: 5989,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
			FullLevel: false,
			NextVipLevelId: nextVipLevelId,
			NextVipLevel: nextVipLevel);
		_ = fakeVipLevelServiceClient.PlayerVipLevelGetAsync(
			playerCode: Arg.Is(command.PlayerCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(playerVipLevelInfo);

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: Arg.Any<int>(),
			vipLevelId: Arg.Any<Guid>(),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Any<LevelUpgradeFormCreateCommand>(),
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

	/// <summary>
	/// 此玩家已經最高等
	/// </summary>
	[Fact]
	public async void This_player_is_already_the_highest_level()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 6001,
			AccumTurnover: 5001,
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
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Is(new LevelUpgradeFormCreateCommand(
				PlayerCode: playerVipLevelInfo.PlayerCode,
				VipLevel: higher.Level)),
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

	/// <summary>
	/// 此玩家存款達標但有效投注未達標
	/// </summary>
	[Fact]
	public async void This_player_is_deposit_reaches_the_limit_but_valid_bets_do_not_reach_the_limit()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 10000,
			AccumTurnover: 10998,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
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
			LevelUpAccumulateBetAmount: 0.0m,
			LevelUpAccumulateDeposit: 0.0m,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 99.0m,
			PromotionBenefit: 0.0m,
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

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Is(new LevelUpgradeFormCreateCommand(
				PlayerCode: playerVipLevelInfo.PlayerCode,
				VipLevel: higher.Level)),
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

	/// <summary>
	/// 此玩家有效投注達標但存款未達標
	/// </summary>
	[Fact]
	public async void This_player_is_valid_bets_have_reached_the_limit_but_the_deposit_has_not_reached_the_limit()
	{
		var fakeLog = NullLogger<LevelUpgradeCommandService>.Instance;
		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeVipLevelUtensils = Substitute.For<IVipLevelUtensils>();
		var fakeLevelUpgradeFormCreateService = Substitute.For<ICommandService<LevelUpgradeFormCreateCommand>>();
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
			AccumDeposit: 9998,
			AccumTurnover: 11000,
			LevelUpAccumulateBetAmount: 10999,
			LevelUpAccumulateDeposit: 9999,
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
			LevelUpAccumulateBetAmount: 0.0m,
			LevelUpAccumulateDeposit: 0.0m,
			LevelKeepAccumulateBetAmount: 0,
			LevelKeepAccumulateDeposit: 0,
			LevelKeepCondition: true,
			BirthBenefit: 99.0m,
			PromotionBenefit: 0.0m,
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

		var sut = new LevelUpgradeCommandService(
			fakeLog,
			fakeVipLevelServiceClient,
			fakeVipLevelUtensils,
			fakeLevelUpgradeFormCreateService,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeVipLevelServiceClient
			.Received(0)
			.PlayerVipLevelSystemAsync(
			playerCode: playerVipLevelInfo.PlayerCode,
			vipLevel: higher.Level,
			vipLevelId: higher.Id,
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeLevelUpgradeFormCreateService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Is(new LevelUpgradeFormCreateCommand(
				PlayerCode: playerVipLevelInfo.PlayerCode,
				VipLevel: higher.Level)),
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