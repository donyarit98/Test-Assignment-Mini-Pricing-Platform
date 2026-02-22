using PricingPlatform.Core.Entities;

namespace PricingPlatform.Core.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateAsync(Job job);
    Task<Job?> GetByIdAsync(string id);
    Task<Job> UpdateAsync(Job job);
}
