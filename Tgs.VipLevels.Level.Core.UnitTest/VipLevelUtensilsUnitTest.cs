using ExpectedObjects;
using NSubstitute;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common.ViewModels;

namespace Tgs.VipLevels.Level.Core.UnitTest;

public class VipLevelUtensilsUnitTest
{
	[Fact]
	public async Task GetConditions_Normal_Process()
	{
		Guid brandId = Guid.NewGuid();
		int level = 2;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

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
		_ = fakeVipLevelServiceClient.VipLevelQueryAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Lower);
	}

	[Fact]
	public async Task GetConditions_Normal_Process_Lower()
	{
		Guid brandId = Guid.NewGuid();
		int level = 1;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

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
		fakeVipLevelServiceClient.VipLevelQueryAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(0).ToExpectedObject().ShouldEqual(Lower);
	}

	[Fact]
	public async Task GetConditions_Normal_Process_Higher()
	{
		Guid brandId = Guid.NewGuid();
		int level = 3;

		var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

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
		fakeVipLevelServiceClient.VipLevelQueryAsync(
			brandId: Arg.Is(brandId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(vipLevelInfos.ToAsyncEnumerable());

		var sut = new VipLevelUtensils(
			fakeVipLevelServiceClient);

		(VipLevelInfoViewModel Current, VipLevelInfoViewModel Higher, VipLevelInfoViewModel Lower) = await sut.GetConditionsAsync(brandId, level);

		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Current);
		vipLevelInfos.ElementAt(2).ToExpectedObject().ShouldEqual(Higher);
		vipLevelInfos.ElementAt(1).ToExpectedObject().ShouldEqual(Lower);
	}
}
