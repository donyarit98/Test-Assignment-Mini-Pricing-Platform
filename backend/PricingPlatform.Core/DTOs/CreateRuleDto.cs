using PricingPlatform.Core.Enums;

namespace PricingPlatform.Core.DTOs;

public class CreateRuleDto
{
    public string Name { get; set; } = string.Empty;
    public RuleType Type { get; set; }
    public int Priority { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public bool IsActive { get; set; }

    // TimeWindowPromotion
    public TimeSpan? WindowStart { get; set; }
    public TimeSpan? WindowEnd { get; set; }
    public decimal? DiscountPercent { get; set; }

    // RemoteAreaSurcharge
    public List<string> RemoteZipCodes { get; set; } = new();
    public decimal? SurchargeAmount { get; set; }

    // WeightTier
    public decimal? WeightFrom { get; set; }
    public decimal? WeightTo { get; set; }
    public decimal? PricePerKg { get; set; }
}
