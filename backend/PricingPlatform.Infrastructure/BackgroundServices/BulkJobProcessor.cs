using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PricingPlatform.Core.Enums;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Infrastructure.BackgroundServices;

public class BulkJobProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BulkJobProcessor> _logger;
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public BulkJobProcessor(
        IServiceProvider serviceProvider,
        ILogger<BulkJobProcessor> logger,
        IJobRepository jobRepository,
        IJobQueue jobQueue)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BulkJobProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_jobQueue.TryDequeue(out var item))
            {
                _logger.LogInformation("Found job {JobId} in queue", item.JobId);
                await ProcessJobAsync(item.JobId, item.Request);
            }
            else
            {
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(string jobId, Core.DTOs.BulkQuoteRequestDto request)
    {
        using var scope = _serviceProvider.CreateScope();
        var quoteService = scope.ServiceProvider.GetRequiredService<IQuoteService>();

        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            _logger.LogWarning("Job {JobId} not found!", jobId);
            return;
        }

        job.Status = JobStatus.Processing;
        await _jobRepository.UpdateAsync(job);

        try
        {
            foreach (var quoteRequest in request.Quotes)
            {
                var result = await quoteService.CalculateAsync(quoteRequest);
                job.Results.Add(result);
                job.ProcessedItems++;
                await _jobRepository.UpdateAsync(job);
                //  await Task.Delay(10);
            }

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Job {JobId} completed with {Count} results",
                jobId, job.Results.Count);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Job {JobId} failed", jobId);
        }

        await _jobRepository.UpdateAsync(job);
    }
}