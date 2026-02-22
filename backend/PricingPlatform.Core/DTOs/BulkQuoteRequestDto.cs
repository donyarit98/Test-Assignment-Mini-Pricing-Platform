namespace PricingPlatform.Core.DTOs;

public class BulkQuoteRequestDto
{
    public List<QuoteRequestDto> Quotes { get; set; } = new();
}
