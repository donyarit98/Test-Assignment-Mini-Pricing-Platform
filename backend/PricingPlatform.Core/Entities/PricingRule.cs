using PricingPlatform.Core.Enums;

namespace PricingPlatform.Core.Entities;

public class PricingRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public RuleType Type { get; set; }
    public int Priority { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public bool IsActive { get; set; }

    // TimeWindowPromotion fields
    public TimeSpan? WindowStart { get; set; }
    public TimeSpan? WindowEnd { get; set; }
    public decimal? DiscountPercent { get; set; }

    // RemoteAreaSurcharge fields
    public List<string> RemoteZipCodes { get; set; } = new();
    public decimal? SurchargeAmount { get; set; }

    // WeightTier fields
    public decimal? WeightFrom { get; set; }
    public decimal? WeightTo { get; set; }
    public decimal? PricePerKg { get; set; }
    public decimal? MaxWeightKg { get; set; }
    
}
