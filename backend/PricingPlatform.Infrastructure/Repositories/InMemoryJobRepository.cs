using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Interfaces;
using System.Collections.Concurrent;

namespace PricingPlatform.Infrastructure.Repositories;

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<string, Job> _jobs = new();

    public Task<Job> CreateAsync(Job job)
    {
        _jobs[job.Id] = job;
        return Task.FromResult(job);
    }

    public Task<Job?> GetByIdAsync(string id)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<Job> UpdateAsync(Job job)
    {
        _jobs[job.Id] = job;
        return Task.FromResult(job);
    }
}
