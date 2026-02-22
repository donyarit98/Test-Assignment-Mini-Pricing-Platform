using Microsoft.AspNetCore.Mvc;
using PricingPlatform.Core.DTOs;
using PricingPlatform.Core.Entities;
using PricingPlatform.Core.Interfaces;

namespace PricingPlatform.API.Controllers;

[ApiController]
[Route("rules")]
public class RulesController : ControllerBase
{
    private readonly IRuleRepository _ruleRepository;
    private readonly ILogger<RulesController> _logger;

    public RulesController(IRuleRepository ruleRepository, ILogger<RulesController> logger)
    {
        _ruleRepository = ruleRepository;
        _logger = logger;
    }

    /// <summary>Get all pricing rules</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _ruleRepository.GetAllAsync();
        return Ok(rules);
    }

    /// <summary>Get a single rule by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var rule = await _ruleRepository.GetByIdAsync(id);
        if (rule == null)
            return NotFound(new { message = $"Rule {id} not found" });

        return Ok(rule);
    }

    /// <summary>Create a new pricing rule</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRuleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Rule name is required");

        if (dto.EffectiveTo <= dto.EffectiveFrom)
            return BadRequest("EffectiveTo must be after EffectiveFrom");

        var rule = new PricingRule
        {
            Name = dto.Name,
            Type = dto.Type,
            Priority = dto.Priority,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsActive = dto.IsActive,
            WindowStart = dto.WindowStart,
            WindowEnd = dto.WindowEnd,
            DiscountPercent = dto.DiscountPercent,
            RemoteZipCodes = dto.RemoteZipCodes ?? new(),
            SurchargeAmount = dto.SurchargeAmount,
            WeightFrom = dto.WeightFrom,
            WeightTo = dto.WeightTo,
            PricePerKg = dto.PricePerKg
        };

        _logger.LogInformation("Creating rule {Name} of type {Type}", rule.Name, rule.Type);
        var created = await _ruleRepository.CreateAsync(rule);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing rule</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateRuleDto dto)
    {
        var rule = new PricingRule
        {
            Name = dto.Name,
            Type = dto.Type,
            Priority = dto.Priority,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsActive = dto.IsActive,
            WindowStart = dto.WindowStart,
            WindowEnd = dto.WindowEnd,
            DiscountPercent = dto.DiscountPercent,
            RemoteZipCodes = dto.RemoteZipCodes ?? new(),
            SurchargeAmount = dto.SurchargeAmount,
            WeightFrom = dto.WeightFrom,
            WeightTo = dto.WeightTo,
            PricePerKg = dto.PricePerKg
        };

        var updated = await _ruleRepository.UpdateAsync(id, rule);
        if (updated == null)
            return NotFound(new { message = $"Rule {id} not found" });

        return Ok(updated);
    }

    /// <summary>Delete a rule</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _ruleRepository.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Rule {id} not found" });

        return NoContent();
    }
}