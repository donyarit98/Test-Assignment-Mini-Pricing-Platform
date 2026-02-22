using FluentAssertions;
using Moq;
using PricingPlatform.Application.Rules;
using PricingPlatform.Application.Services;
using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Enums;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Tests.Unit;

public class PricingEngineTests
{
    private readonly Mock<IRuleRepository> _ruleRepoMock;
    private readonly List<IPricingRule> _pricingRules;

    public PricingEngineTests()
    {
        _ruleRepoMock = new Mock<IRuleRepository>();
        _pricingRules = new List<IPricingRule>
        {
            new TimeWindowPromotionRule(),
            new RemoteAreaSurchargeRule(),
            new WeightTierRule()
        };
    }

    private PricingEngine CreateEngine() =>
        new PricingEngine(_ruleRepoMock.Object, _pricingRules);

    [Fact]
    public async Task Calculate_NoRules_ReturnsBasePrice()
    {
        _ruleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PricingRule>());
        var engine = CreateEngine();

        var result = await engine.CalculateAsync(new QuoteRequestDto
        {
            WeightKg = 5,
            DeclaredValue = 100,
            DestinationZipCode = "12345",
            ShipmentDate = DateTime.UtcNow
        });

        result.BasePrice.Should().Be(100);
        result.FinalPrice.Should().Be(100);
        result.AppliedRules.Should().BeEmpty();
    }

    // ─── WeightTier Tests ──────────────────────────────────
    [Theory]
    [InlineData(10, 15, 150)]   // 10kg * 15/kg = 150
    [InlineData(20, 15, 300)]   // 20kg * 15/kg = 300
    [InlineData(50, 10, 500)]   // 50kg * 10/kg = 500
    public async Task Calculate_WeightTierRule_CorrectlyMultipliesWeightByRate(
        decimal weightKg, decimal pricePerKg, decimal expectedPrice)
    {
        var rule = new PricingRule
        {
            Id = "r1", Name = "Weight Tier",
            Type = RuleType.WeightTier, Priority = 1, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            WeightFrom = 1, WeightTo = 100,
            PricePerKg = pricePerKg
        };

        _ruleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PricingRule> { rule });

        var result = await CreateEngine().CalculateAsync(new QuoteRequestDto
        {
            WeightKg = weightKg,
            DeclaredValue = 100,
            DestinationZipCode = "12345",
            ShipmentDate = DateTime.UtcNow
        });

        result.FinalPrice.Should().Be(expectedPrice);
        result.AppliedRules.Should().HaveCount(1);
    }

    // ─── RemoteArea Tests ──────────────────────────────────
    [Theory]
    [InlineData("90210", 50, 150)]   // remote zip, 100 base + 50 surcharge
    [InlineData("10001", 100, 200)]  // remote zip, 100 base + 100 surcharge
    [InlineData("99999", 50, 100)]   // non-remote zip, no surcharge
    public async Task Calculate_RemoteAreaSurcharge_CorrectlyAppliesToRemoteZips(
        string zipCode, decimal surcharge, decimal expectedPrice)
    {
        var rule = new PricingRule
        {
            Id = "r2", Name = "Remote Surcharge",
            Type = RuleType.RemoteAreaSurcharge, Priority = 1, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            RemoteZipCodes = new List<string> { "90210", "10001" },
            SurchargeAmount = surcharge
        };

        _ruleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PricingRule> { rule });

        var result = await CreateEngine().CalculateAsync(new QuoteRequestDto
        {
            WeightKg = 5,
            DeclaredValue = 100,
            DestinationZipCode = zipCode,
            ShipmentDate = DateTime.UtcNow
        });

        result.FinalPrice.Should().Be(expectedPrice);
    }

    // ─── TimeWindow Discount Tests ─────────────────────────
    [Theory]
    [InlineData(10, 200, 180)]   // 10% off 200 = 180
    [InlineData(20, 200, 160)]   // 20% off 200 = 160
    [InlineData(50, 200, 100)]   // 50% off 200 = 100
    public async Task Calculate_TimeWindowPromotion_CorrectlyAppliesDiscount(
        decimal discountPercent, decimal declaredValue, decimal expectedPrice)
    {
        var rule = new PricingRule
        {
            Id = "r3", Name = "Promo",
            Type = RuleType.TimeWindowPromotion, Priority = 1, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            WindowStart = new TimeSpan(0, 0, 0),
            WindowEnd = new TimeSpan(23, 59, 59),
            DiscountPercent = discountPercent
        };

        _ruleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PricingRule> { rule });

        var result = await CreateEngine().CalculateAsync(new QuoteRequestDto
        {
            WeightKg = 5,
            DeclaredValue = declaredValue,
            DestinationZipCode = "12345",
            ShipmentDate = DateTime.UtcNow
        });

        result.FinalPrice.Should().Be(expectedPrice);
    }

    // ─── Rule State Tests ──────────────────────────────────
    [Theory]
    [InlineData(false, true, 100)]   // inactive rule - not applied
    [InlineData(true, false, 100)]   // expired rule - not applied
    [InlineData(true, true, 150)]    // active + valid - applied
    public async Task Calculate_RuleState_OnlyAppliesWhenActiveAndValid(
        bool isActive, bool isValid, decimal expectedPrice)
    {
        var rule = new PricingRule
        {
            Id = "r4", Name = "Surcharge",
            Type = RuleType.RemoteAreaSurcharge, Priority = 1,
            IsActive = isActive,
            EffectiveFrom = isValid ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1),
            EffectiveTo = isValid ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow.AddDays(2),
            RemoteZipCodes = new List<string> { "90210" },
            SurchargeAmount = 50
        };

        _ruleRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PricingRule> { rule });

        var result = await CreateEngine().CalculateAsync(new QuoteRequestDto
        {
            WeightKg = 5,
            DeclaredValue = 100,
            DestinationZipCode = "90210",
            ShipmentDate = DateTime.UtcNow
        });

        result.FinalPrice.Should().Be(expectedPrice);
    }
}