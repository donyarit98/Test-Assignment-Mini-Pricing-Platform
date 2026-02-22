using System.Text.Json;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.Infrastructure.Repositories;

public class JsonRuleRepository : IRuleRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonRuleRepository(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // Create file with empty array if it doesn't exist
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<PricingRule>> ReadAllAsync()
    {
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<PricingRule>>(json, _jsonOptions) ?? new();
    }

    private async Task WriteAllAsync(List<PricingRule> rules)
    {
        var json = JsonSerializer.Serialize(rules, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<List<PricingRule>> GetAllAsync() => await ReadAllAsync();

    public async Task<PricingRule?> GetByIdAsync(string id)
    {
        var rules = await ReadAllAsync();
        return rules.FirstOrDefault(r => r.Id == id);
    }

    public async Task<PricingRule> CreateAsync(PricingRule rule)
    {
        var rules = await ReadAllAsync();
        rules.Add(rule);
        await WriteAllAsync(rules);
        return rule;
    }

    public async Task<PricingRule?> UpdateAsync(string id, PricingRule updated)
    {
        var rules = await ReadAllAsync();
        var index = rules.FindIndex(r => r.Id == id);
        if (index == -1) return null;

        updated.Id = id;
        rules[index] = updated;
        await WriteAllAsync(rules);
        return updated;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var rules = await ReadAllAsync();
        var rule = rules.FirstOrDefault(r => r.Id == id);
        if (rule == null) return false;

        rules.Remove(rule);
        await WriteAllAsync(rules);
        return true;
    }
}