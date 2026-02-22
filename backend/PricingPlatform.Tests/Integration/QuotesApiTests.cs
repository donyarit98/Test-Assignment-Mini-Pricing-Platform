using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using PricingPlatform.Core.DTOs;

namespace PricingPlatform.Tests.Integration;

public class QuotesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public QuotesApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ─── Health Tests ──────────────────────────────────────
    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    // ─── Quote Price Tests ─────────────────────────────────
    [Theory]
    [InlineData(5, 100, "12345", true)]    // valid request
    [InlineData(10, 200, "90210", true)]   // valid remote zip
    [InlineData(1, 50, "00001", true)]     // min weight
    public async Task CalculatePrice_ValidRequests_ReturnOk(
        decimal weight, decimal value, string zip, bool shouldSucceed)
    {
        var request = new QuoteRequestDto
        {
            WeightKg = weight,
            DeclaredValue = value,
            DestinationZipCode = zip,
            ShipmentDate = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/quotes/price", request);

        if (shouldSucceed)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("finalPrice");
        }
    }

    [Theory]
    [InlineData(0, 100, "12345")]     // zero weight
    [InlineData(-1, 100, "12345")]    // negative weight
    [InlineData(5, 100, "")]          // empty zip
    public async Task CalculatePrice_InvalidRequests_ReturnBadRequest(
        decimal weight, decimal value, string zip)
    {
        var request = new QuoteRequestDto
        {
            WeightKg = weight,
            DeclaredValue = value,
            DestinationZipCode = zip,
            ShipmentDate = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/quotes/price", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── Bulk Job Tests ────────────────────────────────────
    [Theory]
    [InlineData(1)]    // single quote
    [InlineData(3)]    // multiple quotes
    [InlineData(5)]    // many quotes
    public async Task SubmitBulk_ValidRequests_ReturnAcceptedWithJobId(int quoteCount)
    {
        var quotes = Enumerable.Range(1, quoteCount).Select(_ => new QuoteRequestDto
        {
            WeightKg = 5,
            DeclaredValue = 100,
            DestinationZipCode = "12345",
            ShipmentDate = DateTime.UtcNow
        }).ToList();

        var response = await _client.PostAsJsonAsync("/quotes/bulk",
            new BulkQuoteRequestDto { Quotes = quotes });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("job_id");
    }

    [Fact]
    public async Task GetJob_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/jobs/non-existent-id");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}