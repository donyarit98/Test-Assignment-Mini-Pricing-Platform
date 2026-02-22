using PricingPlatform.Core.Enums;

namespace PricingPlatform.Core.Entities;

public class Job
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public List<QuoteResult> Results { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
