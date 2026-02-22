using System.Collections.Concurrent;
using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Infrastructure.Repositories;

public class InMemoryJobQueue : IJobQueue
{
    private readonly ConcurrentQueue<(string JobId, BulkQuoteRequestDto Request)> _queue = new();

    public void Enqueue(string jobId, BulkQuoteRequestDto request)
        => _queue.Enqueue((jobId, request));

    public bool TryDequeue(out (string JobId, BulkQuoteRequestDto Request) item)
        => _queue.TryDequeue(out item);
}