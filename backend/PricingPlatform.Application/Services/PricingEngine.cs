using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Application.Services;

public class PricingEngine
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IEnumerable<IPricingRule> _pricingRules;

    public PricingEngine(IRuleRepository ruleRepository, IEnumerable<IPricingRule> pricingRules)
    {
        _ruleRepository = ruleRepository;
        _pricingRules = pricingRules;
    }

    public async Task<QuoteResult> CalculateAsync(QuoteRequestDto request)
    {
        var result = new QuoteResult();

        // Base price = declared value as starting point
        var basePrice = request.DeclaredValue > 0 ? request.DeclaredValue : request.WeightKg * 10;
        result.BasePrice = basePrice;

        var currentPrice = basePrice;
        var appliedRules = new List<string>();

        // Get all active rules sorted by priority (lower number = higher priority)
        var rules = await _ruleRepository.GetAllAsync();
        var activeRules = rules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in activeRules)
        {
            if (rule.MaxWeightKg.HasValue && 
            request.WeightKg >= rule.MaxWeightKg) continue;
            // Find the matching strategy handler for this rule type
            var handler = _pricingRules.FirstOrDefault(h => h.IsApplicable(rule, request));
            if (handler != null)
            {
                currentPrice = handler.Apply(rule, request, currentPrice);
                appliedRules.Add($"{rule.Name} (Priority: {rule.Priority})");
            }
        }

        result.FinalPrice = Math.Round(currentPrice, 2);
        result.AppliedRules = appliedRules;
        return result;
    }
}