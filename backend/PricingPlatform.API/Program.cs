using PricingPlatform.Application.Rules;
using PricingPlatform.Application.Services;
using PricingPlatform.Core.Interfaces;
using PricingPlatform.Infrastructure.BackgroundServices;
using PricingPlatform.Infrastructure.Repositories;
using PricingPlatform.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PricingPlatform.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers ───────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─── Swagger ───────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Mini Pricing Platform API",
        Version = "v1",
        Description = "A configurable pricing engine with rules management"
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});
// ─── Rules File Path ──────────────────────────────────────
var rulesFileName = builder.Configuration["RulesFilePath"] ?? "Data/rules.json";
var rulesFilePath = Path.IsPathRooted(rulesFileName)
    ? rulesFileName
    : Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "PricingPlatform.Infrastructure", "Data", "rules.json");
Directory.CreateDirectory(Path.GetDirectoryName(rulesFilePath)!);

// ─── CORS ─────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IRuleRepository>(_ => new JsonRuleRepository(rulesFilePath));
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();

// ─── Pricing Rules (Strategy Pattern) ─────────────────────
builder.Services.AddScoped<IPricingRule, TimeWindowPromotionRule>();
builder.Services.AddScoped<IPricingRule, RemoteAreaSurchargeRule>();
builder.Services.AddScoped<IPricingRule, WeightTierRule>();

// ─── Application Services ──────────────────────────────────
builder.Services.AddScoped<PricingEngine>();
builder.Services.AddScoped<IQuoteService, QuoteService>();

// ─── Background Services ───────────────────────────────────
builder.Services.AddHostedService<BulkJobProcessor>();
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();

// ─── Logging ───────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ─── Rate Limiting (Built-in .NET 8) ──────────────────────
builder.Services.AddRateLimiter(options =>
{
    // /quotes/price — 30 ครั้ง/นาที
    options.AddFixedWindowLimiter("quotes-price", o =>
    {
        o.PermitLimit = 30;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // /quotes/bulk — 5 ครั้ง/นาที
    options.AddFixedWindowLimiter("quotes-bulk", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // return 429 เมื่อเกิน limit
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", token);
    };
});

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini Pricing Platform v1");
        c.RoutePrefix = "swagger";
    });
// }

app.UseRateLimiter();                          // ← Rate Limiting
app.UseMiddleware<CorrelationIdMiddleware>();   // ← Correlation ID
app.UseCors("AllowFrontend");                  // ← CORS
app.UseAuthorization();                        // ← Auth
app.MapControllers();                          // ← Controllers

// ─── Seed ─────────────────────────────────────────────────
await SeedSampleRules(app);

app.Run();

// ─── Seed Method ──────────────────────────────────────────
static async Task SeedSampleRules(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var ruleRepo = scope.ServiceProvider.GetRequiredService<IRuleRepository>();
    var existing = await ruleRepo.GetAllAsync();
    if (existing.Any()) return;

    var sampleRules = new List<PricingPlatform.Core.Entities.PricingRule>
    {
        new()
        {
            Id = "rule-001",
            Name = "Morning Promotion",
            Type = PricingPlatform.Core.Enums.RuleType.TimeWindowPromotion,
            Priority = 1,
            EffectiveFrom = new DateTime(2024, 1, 1),
            EffectiveTo = new DateTime(2099, 12, 31),
            IsActive = true,
            WindowStart = new TimeSpan(6, 0, 0),
            WindowEnd = new TimeSpan(9, 0, 0),
            DiscountPercent = 10
        },
        new()
        {
            Id = "rule-002",
            Name = "Remote Area Surcharge",
            Type = PricingPlatform.Core.Enums.RuleType.RemoteAreaSurcharge,
            Priority = 2,
            EffectiveFrom = new DateTime(2024, 1, 1),
            EffectiveTo = new DateTime(2099, 12, 31),
            IsActive = true,
            SurchargeAmount = 50,
            RemoteZipCodes = new List<string> { "90210", "10001", "77001" }
        },
        new()
        {
            Id = "rule-003",
            Name = "Heavy Package Tier",
            Type = PricingPlatform.Core.Enums.RuleType.WeightTier,
            Priority = 3,
            EffectiveFrom = new DateTime(2024, 1, 1),
            EffectiveTo = new DateTime(2099, 12, 31),
            IsActive = true,
            WeightFrom = 10,
            WeightTo = 50,
            PricePerKg = 15
        }
    };

    foreach (var rule in sampleRules)
        await ruleRepo.CreateAsync(rule);
}

// needed for integration tests
public partial class Program { }