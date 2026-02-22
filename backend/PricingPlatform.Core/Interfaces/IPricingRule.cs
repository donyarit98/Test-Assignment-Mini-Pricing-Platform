using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;

namespace PricingPlatform.Core.Interfaces;

public interface IPricingRule
{
    bool IsApplicable(PricingRule rule, QuoteRequestDto request);
    decimal Apply(PricingRule rule, QuoteRequestDto request, decimal currentPrice);
}
