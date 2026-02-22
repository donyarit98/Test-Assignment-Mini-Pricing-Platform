using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Interfaces;  // ← ครอบคลุม IJobRepository และ IJobQueue

namespace PricingPlatform.Application.Services;

public class QuoteService : IQuoteService
{
    private readonly PricingEngine _pricingEngine;
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public QuoteService(
        PricingEngine pricingEngine,
        IJobRepository jobRepository,
        IJobQueue jobQueue)
    {
        _pricingEngine = pricingEngine;
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    public async Task<QuoteResult> CalculateAsync(QuoteRequestDto request)
    {
        return await _pricingEngine.CalculateAsync(request);
    }

    public async Task<string> SubmitBulkAsync(BulkQuoteRequestDto request)
    {
        var job = new Job
        {
            TotalItems = request.Quotes.Count,
            Status = Core.Enums.JobStatus.Pending
        };

        await _jobRepository.CreateAsync(job);
        _jobQueue.Enqueue(job.Id, request);
        return job.Id;
    }
}