namespace PricingPlatform.Core.Entities;

public class QuoteResult
{
    public string QuoteId { get; set; } = Guid.NewGuid().ToString();
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public List<string> AppliedRules { get; set; } = new();
    public string? Error { get; set; }
}
