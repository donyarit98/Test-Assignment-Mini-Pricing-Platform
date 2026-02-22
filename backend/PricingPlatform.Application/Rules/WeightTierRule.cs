using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Enums;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Application.Rules;

public class WeightTierRule : IPricingRule
{
    public bool IsApplicable(PricingRule rule, QuoteRequestDto request)
    {
        if (rule.Type != RuleType.WeightTier) return false;
        if (!rule.IsActive) return false;
        if (rule.WeightFrom == null || rule.WeightTo == null) return false;

        var now = request.ShipmentDate;
        if (now < rule.EffectiveFrom || now > rule.EffectiveTo) return false;

        return request.WeightKg >= rule.WeightFrom && request.WeightKg <= rule.WeightTo;
    }

    public decimal Apply(PricingRule rule, QuoteRequestDto request, decimal currentPrice)
    {
        if (rule.PricePerKg == null) return currentPrice;
        return request.WeightKg * rule.PricePerKg.Value;
    }
}