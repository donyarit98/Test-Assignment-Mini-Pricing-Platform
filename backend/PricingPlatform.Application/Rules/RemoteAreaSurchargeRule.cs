using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Enums;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Application.Rules;

public class RemoteAreaSurchargeRule : IPricingRule
{
    public bool IsApplicable(PricingRule rule, QuoteRequestDto request)
    {
        if (rule.Type != RuleType.RemoteAreaSurcharge) return false;
        if (!rule.IsActive) return false;
        if (rule.RemoteZipCodes == null || !rule.RemoteZipCodes.Any()) return false;

        var now = request.ShipmentDate;
        if (now < rule.EffectiveFrom || now > rule.EffectiveTo) return false;

        return rule.RemoteZipCodes.Contains(request.DestinationZipCode);
    }

    public decimal Apply(PricingRule rule, QuoteRequestDto request, decimal currentPrice)
    {
        return currentPrice + (rule.SurchargeAmount ?? 0);
    }
}