using PricingPlatform.Core.DTOs;

namespace PricingPlatform.Core.Interfaces;

public interface IJobQueue
{
    void Enqueue(string jobId, BulkQuoteRequestDto request);
    bool TryDequeue(out (string JobId, BulkQuoteRequestDto Request) item);
}