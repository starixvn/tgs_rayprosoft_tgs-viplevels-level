using ExpectedObjects;
using NSubstitute;
using Tgs.Calculate.Clients;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace Tgs.VipLevels.Level.Core.UnitTest;

public class VipLevelUtensilsUnitTest
{
	[Fact]
	public async void GetConditions_Normal_Process()
	{
		Guid brandId = Guid.NewGuid();
		int level = 2;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeCalculateClient = Substitute.For<ICalculateClient>();

		var snapshotId = Guid.NewGuid();
		_ = fakeVipLevelServiceClient.VipLevelSnapshotCurrentSnapshotIdAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(snapshotId);

		var vipLevelInfos = new VipLevelInfoViewModel[] {
			new(
				Id: Guid.NewGuid(),
				Level: 1,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 2,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 3,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
		};
		fakeVipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: Arg.Is(snapshotId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient,
			fakeCalculateClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Lower);
	}

	[Fact]
	public async void GetConditions_Normal_Process_Lower()
	{
		Guid brandId = Guid.NewGuid();
		int level = 1;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeCalculateClient = Substitute.For<ICalculateClient>();

		var snapshotId = Guid.NewGuid();
		_ = fakeVipLevelServiceClient.VipLevelSnapshotCurrentSnapshotIdAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(snapshotId);

		var vipLevelInfos = new VipLevelInfoViewModel[] {
			new(
				Id: Guid.NewGuid(),
				Level: 1,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 2,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 3,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
		};
		fakeVipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: Arg.Is(snapshotId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient,
			fakeCalculateClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Lower);
	}

	[Fact]
	public async void GetConditions_Normal_Process_Higher()
	{
		Guid brandId = Guid.NewGuid();
		int level = 3;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeCalculateClient = Substitute.For<ICalculateClient>();

		var snapshotId = Guid.NewGuid();
		_ = fakeVipLevelServiceClient.VipLevelSnapshotCurrentSnapshotIdAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(snapshotId);

		var vipLevelInfos = new VipLevelInfoViewModel[] {
			new(
				Id: Guid.NewGuid(),
				Level: 1,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 2,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 3,
				BrandId: brandId,
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
		};
		fakeVipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: Arg.Is(snapshotId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient,
			fakeCalculateClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Lower);
	}

	[Fact]
	public async void GetVipLevelInfo_Normal_Process()
	{
		Guid snapshotId = Guid.NewGuid();
		int level = 2;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeCalculateClient = Substitute.For<ICalculateClient>();

		var vipLevelInfos = new VipLevelInfoViewModel[] {
			new(
				Id: Guid.NewGuid(),
				Level: 1,
				BrandId: Guid.NewGuid(),
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 2,
				BrandId: Guid.NewGuid(),
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
			new(
				Id: Guid.NewGuid(),
				Level: 3,
				BrandId: Guid.NewGuid(),
				LevelUpAccumulateBetAmount: 0,
				LevelUpAccumulateDeposit: 0,
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
				WithdrawTotalAmountPerDay: -1),
		};
		fakeVipLevelServiceClient.VipLevelSnapshotQueryVipLevelAsync(
			id: Arg.Is(snapshotId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient,
			fakeCalculateClient);

		var actual = await sut.GetVipLevelInfoAsync(snapshotId, level);

		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(actual);
	}

	[Fact]
	public async void GetCurrentExp_Normal_Process()
	{
		Guid accumDepositId = Guid.NewGuid();
		Guid accumTurnoverId = Guid.NewGuid();

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();
		var fakeCalculateClient = Substitute.For<ICalculateClient>();

		_ = fakeCalculateClient
			.GetBalanceAsync(
			calculateId: Arg.Is(accumDepositId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(new ExchangeResult(
				Id: accumDepositId,
				PostBalance: 99.0m
			));

		_ = fakeCalculateClient
			.GetBalanceAsync(
			calculateId: Arg.Is(accumTurnoverId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(new ExchangeResult(
				Id: accumDepositId,
		PostBalance: 88.0m
		));

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient,
			fakeCalculateClient);

		(decimal CurrentDeposit, decimal CurrentTurnover) = await sut.GetCurrentExpAsync(accumDepositId, accumTurnoverId);

		Assert.Equal(99.0m, CurrentDeposit);
		Assert.Equal(88.0m, CurrentTurnover);
	}
}