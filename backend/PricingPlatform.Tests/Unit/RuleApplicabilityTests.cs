using FluentAssertions;
using PricingPlatform.Application.Rules;
using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Enums;

namespace PricingPlatform.Tests.Unit;

public class RuleApplicabilityTests
{
    // ─── WeightTier Applicability ──────────────────────────
    [Theory]
    [InlineData(10, 50, 10, true)]   // exactly at min boundary
    [InlineData(10, 50, 50, true)]   // exactly at max boundary
    [InlineData(10, 50, 30, true)]   // in range
    [InlineData(10, 50, 5, false)]   // below range
    [InlineData(10, 50, 60, false)]  // above range
    public void WeightTierRule_Applicability_BasedOnWeightRange(
        decimal weightFrom, decimal weightTo, decimal requestWeight, bool expected)
    {
        var rule = new WeightTierRule();
        var pricingRule = new PricingRule
        {
            Type = RuleType.WeightTier, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            WeightFrom = weightFrom, WeightTo = weightTo
        };

        var request = new QuoteRequestDto
        {
            WeightKg = requestWeight,
            ShipmentDate = DateTime.UtcNow
        };

        rule.IsApplicable(pricingRule, request).Should().Be(expected);
    }

    // ─── RemoteArea Applicability ──────────────────────────
    [Theory]
    [InlineData("90210", true)]    // in remote list
    [InlineData("10001", true)]    // in remote list
    [InlineData("99999", false)]   // not in remote list
    [InlineData("00000", false)]   // not in remote list
    public void RemoteAreaRule_Applicability_BasedOnZipCode(
        string zipCode, bool expected)
    {
        var rule = new RemoteAreaSurchargeRule();
        var pricingRule = new PricingRule
        {
            Type = RuleType.RemoteAreaSurcharge, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            RemoteZipCodes = new List<string> { "90210", "10001" }
        };

        var request = new QuoteRequestDto
        {
            DestinationZipCode = zipCode,
            ShipmentDate = DateTime.UtcNow
        };

        rule.IsApplicable(pricingRule, request).Should().Be(expected);
    }

    // ─── TimeWindow Applicability ──────────────────────────
    [Theory]
    [InlineData(6, 0, 9, 0, 7, 30, true)]    // 7:30 is within 6:00-9:00
    [InlineData(6, 0, 9, 0, 6, 0, true)]     // exactly at start
    [InlineData(6, 0, 9, 0, 9, 0, true)]     // exactly at end
    [InlineData(6, 0, 9, 0, 5, 59, false)]   // just before window
    [InlineData(6, 0, 9, 0, 9, 1, false)]    // just after window
    public void TimeWindowRule_Applicability_BasedOnTimeOfDay(
        int startHour, int startMin, int endHour, int endMin,
        int requestHour, int requestMin, bool expected)
    {
        var rule = new TimeWindowPromotionRule();
        var pricingRule = new PricingRule
        {
            Type = RuleType.TimeWindowPromotion, IsActive = true,
            EffectiveFrom = DateTime.UtcNow.AddDays(-1),
            EffectiveTo = DateTime.UtcNow.AddDays(1),
            WindowStart = new TimeSpan(startHour, startMin, 0),
            WindowEnd = new TimeSpan(endHour, endMin, 0)
        };

        var today = DateTime.UtcNow.Date;
        var request = new QuoteRequestDto
        {
            ShipmentDate = today.AddHours(requestHour).AddMinutes(requestMin)
        };

        rule.IsApplicable(pricingRule, request).Should().Be(expected);
    }
}