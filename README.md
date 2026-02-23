# Mini Pricing Platform

A configurable pricing engine built with .NET 8 Web API and React TypeScript frontend. Supports dynamic pricing rules, bulk quote processing, and real-time job tracking.

---

## Architecture

This project follows **Clean Architecture** principles with 4 layers:

```
frontend/client/          # React + TypeScript + Vite + TailwindCSS
backend/
├── PricingPlatform.Core          # Entities, Interfaces, DTOs, Enums
├── PricingPlatform.Application   # Business Logic, PricingEngine, Strategy Pattern
├── PricingPlatform.Infrastructure # Repositories, Background Services
├── PricingPlatform.API           # Controllers, Program.cs, Swagger
└── PricingPlatform.Tests         # Unit + Integration Tests
```

### Key Design Patterns

- **Clean Architecture** — dependency flows inward toward Core, no layer knows about outer layers
- **Strategy Pattern** — each pricing rule type is an independent strategy (`IPricingRule`)
- **Repository Pattern** — storage abstraction via `IRuleRepository`, `IJobRepository`
- **Background Service** — async bulk job processing via `BulkJobProcessor`
- **Dependency Injection** — all services wired via `Program.cs`, no tight coupling

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 8 Web API |
| Frontend | React 18 + TypeScript + Vite + TailwindCSS |
| Storage | JSON file (rules), In-memory (jobs) |
| Testing | xUnit + FluentAssertions + Moq |
| Docs | Swagger / OpenAPI |
| Container | Docker + Docker Compose |
| CI | GitHub Actions |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)

---

### Run with Docker

```bash
# Clone the repo
git clone <repo-url>
cd mini-pricing-platform

# Start the API
docker-compose up -d

# API runs at http://localhost:8080
# Swagger at http://localhost:8080/swagger
```

---

### Run Locally (Backend)

```bash
cd mini-pricing-platform

# Restore dependencies
dotnet restore

# Run the API
dotnet run --project backend/PricingPlatform.API

# API runs at http://localhost:5295
# Swagger at http://localhost:5295/swagger
```

### Run Locally (Frontend)

```bash
cd frontend/client

# Install dependencies
npm install

# Start dev server
npm run dev

# Frontend runs at http://localhost:5173
```

---

### Run Tests

```bash
dotnet test --verbosity normal
```

```
Total tests: 38
Passed: 38
```

---

## CI Pipeline

GitHub Actions runs automatically on every push to `main`:

| Job | What it does |
|-----|-------------|
| `test` | dotnet restore → build → run 38 tests |
| `lint-frontend` | npm install → ESLint check TypeScript |

---

## Environment Configuration

### Backend

| File | Used When |
|------|-----------|
| `appsettings.json` | Base config (all environments) |
| `appsettings.Development.json` | Local development (`dotnet run`) |
| `appsettings.Production.json` | Production (`docker-compose up`) |

> **Note:** `appsettings.Production.json` is excluded from git via `.gitignore`.  
> Docker uses `ASPNETCORE_ENVIRONMENT=Production` set in `docker-compose.yml`.

Key config values:

```json
{
  "RulesFilePath": "Data/rules.json",
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

Override via environment variables (Docker):
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - RulesFilePath=/app/Data/rules.json
  - Cors__AllowedOrigins__0=http://localhost:8080
```

### Frontend

| File | Used When |
|------|-----------|
| `.env` | Local development (`npm run dev`) |
| `.env.production` | Production build (`npm run build`) |

```env
# .env
VITE_API_URL=http://localhost:5295

# .env.production
VITE_API_URL=http://localhost:8080
```

---

## Rate Limiting

Built-in .NET 8 rate limiting (no external package required):

| Endpoint | Limit |
|----------|-------|
| `POST /quotes/price` | 30 requests/minute |
| `POST /quotes/bulk` | 5 requests/minute |

When exceeded → `429 Too Many Requests`

---

## Correlation ID

Every request is assigned a unique `X-Correlation-ID` header for request tracing:

```
Request  → X-Correlation-ID: abc-123 (client sends or server generates)
Response → X-Correlation-ID: abc-123 (echoed back)
```

Use this ID to trace logs across services or report issues to support.

---

## Pricing Rules

Rules are stored in `rules.json` and loaded at runtime. Three rule types are supported:

### 1. Time Window Promotion
Applies a percentage discount during a specific time window.  
Only applies to shipments with weight **≤ MaxWeightKg**.

```json
{
  "Type": "TimeWindowPromotion",
  "Priority": 1,
  "MaxWeightKg": 10,
  "WindowStart": "06:00:00",
  "WindowEnd": "09:00:00",
  "DiscountPercent": 10
}
```

### 2. Remote Area Surcharge
Adds a flat surcharge for shipments to remote zip codes.  
Only applies to shipments with weight **≤ MaxWeightKg**.

```json
{
  "Type": "RemoteAreaSurcharge",
  "Priority": 2,
  "MaxWeightKg": 10,
  "SurchargeAmount": 50,
  "RemoteZipCodes": ["90210", "10001", "77001"]
}
```

### 3. Weight Tier (Heavy Package)
Calculates price per kg for heavy shipments.  
**No MaxWeightKg** — applies to all packages within the weight range.

```json
{
  "Type": "WeightTier",
  "Priority": 3,
  "WeightFrom": 10,
  "WeightTo": 50,
  "PricePerKg": 15
}
```

Rules are applied in **Priority** order (lowest number first).  
`MaxWeightKg` acts as a hard cap — if shipment weight exceeds this value, the rule is skipped entirely.

### Rule Application Examples

| weightKg | zipCode | Time | Applied Rules |
|----------|---------|------|---------------|
| 5 | 90210 | 07:30 | Morning Promotion + Remote Area Surcharge |
| 5 | 12345 | 12:00 | None |
| 15 | 90210 | 07:30 | Heavy Package Tier (Rule 1, 2 skipped — weight > MaxWeightKg) |
| 15 | 12345 | 12:00 | Heavy Package Tier |

---

## API Reference

### Quotes

#### Calculate Single Quote
```
POST /quotes/price
```
```json
{
  "weightKg": 15,
  "destinationZipCode": "90210",
  "shipmentDate": "2026-02-19T07:30:00",
  "declaredValue": 500
}
```
Response:
```json
{
  "basePrice": 500,
  "finalPrice": 225,
  "appliedRules": ["Morning Promotion (Priority: 1)", "Heavy Package Tier (Priority: 3)"]
}
```

#### Submit Bulk Quotes
```
POST /quotes/bulk
```
```json
{
  "quotes": [
    { "weightKg": 15, "destinationZipCode": "90210", "shipmentDate": "2026-02-19T07:30:00", "declaredValue": 0 },
    { "weightKg": 5, "destinationZipCode": "12345", "shipmentDate": "2026-02-19T12:00:00", "declaredValue": 200 }
  ]
}
```
Response:
```json
{
  "job_id": "39721c8a-2f39-4df8-8366-56d966f9169f",
  "message": "Bulk job submitted successfully"
}
```

### Jobs

#### Get Job Status
```
GET /jobs/{jobId}
```
Response:
```json
{
  "id": "39721c8a-...",
  "status": "Completed",
  "totalItems": 2,
  "processedItems": 2,
  "results": [...]
}
```

Job Status values: `Pending` → `Processing` → `Completed` / `Failed`

### Rules

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/rules` | List all rules |
| GET | `/rules/{id}` | Get single rule |
| POST | `/rules` | Create new rule |
| PUT | `/rules/{id}` | Update rule |
| DELETE | `/rules/{id}` | Delete rule |

### Health

```
GET /health
```

---

## How Bulk Processing Works

```
Client POST /quotes/bulk
    ↓
QuoteService creates Job → adds to Queue → returns job_id immediately
    ↓
BulkJobProcessor (Background Service) picks up job from Queue
    ↓
Processes each quote → updates Job.status and Job.processedItems
    ↓
Client polls GET /jobs/{id} to track progress
```

---

## Sample Data

Sample files provided in the repository:

- `backend/PricingPlatform.Infrastructure/Data/rules.json` — 3 sample pricing rules
- `backend/sample_data/bulk_quotes.csv` — 1000 sample bulk quotes (CSV)
- `backend/sample_data/bulk_quotes.json` — 1000 sample bulk quotes (JSON)

---

## Project Structure

```
mini-pricing-platform/
├── .github/workflows/
│   └── ci.yml
├── docker-compose.yml
├── README.md
├── PricingPlatform.sln
├── backend/
│   ├── Dockerfile
│   ├── sample_data/
│   │   ├── bulk_quotes.csv
│   │   └── bulk_quotes.json
│   ├── PricingPlatform.API/
│   │   ├── Controllers/
│   │   │   ├── QuotesController.cs
│   │   │   ├── JobsController.cs
│   │   │   ├── RulesController.cs
│   │   │   └── HealthController.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   └── Program.cs
│   ├── PricingPlatform.Core/
│   │   ├── Entities/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Enums/
│   ├── PricingPlatform.Application/
│   │   ├── Rules/
│   │   │   ├── TimeWindowPromotionRule.cs
│   │   │   ├── RemoteAreaSurchargeRule.cs
│   │   │   └── WeightTierRule.cs
│   │   └── Services/
│   │       ├── PricingEngine.cs
│   │       └── QuoteService.cs
│   ├── PricingPlatform.Infrastructure/
│   │   ├── Data/
│   │   │   └── rules.json
│   │   ├── Repositories/
│   │   │   ├── JsonRuleRepository.cs
│   │   │   ├── InMemoryJobRepository.cs
│   │   │   └── InMemoryJobQueue.cs
│   │   └── BackgroundServices/
│   │       └── BulkJobProcessor.cs
│   └── PricingPlatform.Tests/
│       ├── Unit/
│       │   ├── PricingEngineTests.cs
│       │   └── RuleApplicabilityTests.cs
│       └── Integration/
│           └── QuotesApiTests.cs
└── frontend/
    └── client/
        └── src/
            ├── pages/
            │   ├── QuotePage.tsx
            │   ├── RulesPage.tsx
            │   ├── BulkQuotePage.tsx
            │   └── JobsPage.tsx
            ├── services/
            │   └── api.ts
            └── types/
                └── index.ts
```