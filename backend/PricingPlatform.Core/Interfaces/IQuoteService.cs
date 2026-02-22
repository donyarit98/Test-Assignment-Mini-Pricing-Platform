using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;

namespace PricingPlatform.Core.Interfaces;

public interface IQuoteService
{
    Task<QuoteResult> CalculateAsync(QuoteRequestDto request);
    Task<string> SubmitBulkAsync(BulkQuoteRequestDto request);
}
