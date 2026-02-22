using Microsoft.AspNetCore.Mvc;
using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Interfaces;
using Microsoft.AspNetCore.RateLimiting;


namespace PricingPlatform.API.Controllers;

[ApiController]
[Route("quotes")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly ILogger<QuotesController> _logger;

    public QuotesController(IQuoteService quoteService, ILogger<QuotesController> logger)
    {
        _quoteService = quoteService;
        _logger = logger;
    }

    /// <summary>Calculate price immediately from payload</summary>
    [HttpPost("price")]
    [EnableRateLimiting("quotes-price")]
    public async Task<IActionResult> CalculatePrice([FromBody] QuoteRequestDto request)
    {
        if (request.WeightKg <= 0)
            return BadRequest("WeightKg must be greater than 0");

        if (string.IsNullOrWhiteSpace(request.DestinationZipCode))
            return BadRequest("DestinationZipCode is required");

        _logger.LogInformation("Calculating price for {ZipCode}, weight {Weight}kg",
            request.DestinationZipCode, request.WeightKg);

        var result = await _quoteService.CalculateAsync(request);
        return Ok(result);
    }

    /// <summary>Submit multiple quote requests, returns job_id</summary>
    [HttpPost("bulk")]
    [EnableRateLimiting("quotes-bulk")]
    public async Task<IActionResult> SubmitBulk([FromBody] BulkQuoteRequestDto request)
    {
        if (request.Quotes == null || !request.Quotes.Any())
            return BadRequest("At least one quote is required");

        _logger.LogInformation("Submitting bulk job with {Count} quotes", request.Quotes.Count);

        var jobId = await _quoteService.SubmitBulkAsync(request);
        return Accepted(new { job_id = jobId, message = "Bulk job submitted successfully" });
    }
}