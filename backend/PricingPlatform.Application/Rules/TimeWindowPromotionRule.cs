using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Enums;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Application.Rules;

public class TimeWindowPromotionRule : IPricingRule
{
    public bool IsApplicable(PricingRule rule, QuoteRequestDto request)
    {
        if (rule.Type != RuleType.TimeWindowPromotion) return false;
        if (!rule.IsActive) return false;
        if (rule.WindowStart == null || rule.WindowEnd == null) return false;

        var now = request.ShipmentDate;
        if (now < rule.EffectiveFrom || now > rule.EffectiveTo) return false;

        var timeOfDay = now.TimeOfDay;
        return timeOfDay >= rule.WindowStart && timeOfDay <= rule.WindowEnd;
    }

    public decimal Apply(PricingRule rule, QuoteRequestDto request, decimal currentPrice)
    {
        if (rule.DiscountPercent == null) return currentPrice;
        var discount = currentPrice * (rule.DiscountPercent.Value / 100);
        return currentPrice - discount;
    }
}