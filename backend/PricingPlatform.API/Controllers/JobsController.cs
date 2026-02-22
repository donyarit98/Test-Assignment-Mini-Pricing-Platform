using Microsoft.AspNetCore.Mvc;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.API.Controllers;

[ApiController]
[Route("jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobRepository jobRepository, ILogger<JobsController> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    /// <summary>Track job status and retrieve results</summary>
    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJob(string jobId)
    {
        _logger.LogInformation("Fetching job {JobId}", jobId);

        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            return NotFound(new { message = $"Job {jobId} not found" });

        return Ok(new
        {
            job.Id,
            Status = job.Status.ToString(),
            job.CreatedAt,
            job.CompletedAt,
            job.TotalItems,
            job.ProcessedItems,
            job.Results,
            job.ErrorMessage
        });
    }
}