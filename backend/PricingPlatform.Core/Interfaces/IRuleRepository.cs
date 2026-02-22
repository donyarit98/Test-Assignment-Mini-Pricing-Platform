using PricingPlatform.Core.Entities;

namespace PricingPlatform.Core.Interfaces;

public interface IRuleRepository
{
    Task<List<PricingRule>> GetAllAsync();
    Task<PricingRule?> GetByIdAsync(string id);
    Task<PricingRule> CreateAsync(PricingRule rule);
    Task<PricingRule?> UpdateAsync(string id, PricingRule rule);
    Task<bool> DeleteAsync(string id);
}
