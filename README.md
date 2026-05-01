# Cryptid-Care Prescription Processor

A .NET 10 Web API that adjudicates pharmacy claims for mythical beings.

---

## Getting Started

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
docker compose up --build
```

This builds the API image, starts SQL Server, applies migrations, seeds test data, and listens on `http://localhost:5043`.

---

## Seed Data

The API seeds the following records automatically on first startup:

| Id | Patient        | Species  | Heads | Active |
|----|----------------|----------|-------|--------|
| 1  | Gary Lupine    | Werewolf | 1     | Yes    |
| 2  | Harriet Hydra  | Hydra    | 3     | Yes    |
| 3  | Felix Ash      | Phoenix  | 1     | Yes    |

| Id | Medicine         | Contains Silver | Base Cost |
|----|------------------|-----------------|-----------|
| 1  | Wolfsbane Tonic  | No              | $30.00    |
| 2  | Silver Salve     | Yes             | $90.00    |

---

## Authentication

All requests require an `X-Api-Key` header:

```
X-Api-Key: cryptid-care-dev-key
```

Requests without a valid key receive `401 Unauthorized`.

---

## API Reference

### `POST /api/claims`

Submit a claim for adjudication. The claim is persisted and the final outcome returned.

**Request**
```json
{
  "patientId": 1,
  "medicineId": 1,
  "quantity": 5
}
```

**Approved — `200 OK`**
```bash
curl -s -X POST http://localhost:5043/api/claims \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: cryptid-care-dev-key" \
  -d '{"patientId": 1, "medicineId": 1, "quantity": 5}'
```
```json
{
  "id": 1,
  "status": "Approved",
  "quantity": 5,
  "totalCost": 150.00,
  "rejectionReason": null
}
```

**Rejected — `400 Bad Request`** *(Werewolf + silver medicine)*
```bash
curl -s -X POST http://localhost:5043/api/claims \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: cryptid-care-dev-key" \
  -d '{"patientId": 1, "medicineId": 2, "quantity": 5}'
```
```json
{
  "id": 2,
  "status": "Rejected",
  "quantity": 5,
  "totalCost": 0,
  "rejectionReason": "Werewolves cannot be prescribed silver-based medications."
}
```

**Hydra patient — quantity multiplied by head count** *(3 heads × 4 = 12)*
```bash
curl -s -X POST http://localhost:5043/api/claims \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: cryptid-care-dev-key" \
  -d '{"patientId": 2, "medicineId": 1, "quantity": 4}'
```
```json
{
  "id": 3,
  "status": "Approved",
  "quantity": 12,
  "totalCost": 360.00,
  "rejectionReason": null
}
```

**Other responses**

| Status | Condition |
|--------|-----------|
| `404 Not Found` | `patientId` or `medicineId` does not exist |
| `400 Bad Request` | Patient exists but is inactive |
| `401 Unauthorized` | Missing or invalid `X-Api-Key` |

---

### `POST /api/claims/check` *(Wildcard Feature)*

Dry-run adjudication without persisting anything. Always returns `200 OK` — the outcome is in the response body.

**Request** — same shape as `/api/claims`

```bash
curl -s -X POST http://localhost:5043/api/claims/check \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: cryptid-care-dev-key" \
  -d '{"patientId": 1, "medicineId": 2, "quantity": 5}'
```
```json
{
  "status": "Rejected",
  "quantity": 5,
  "totalCost": 0,
  "rejectionReason": "Werewolves cannot be prescribed silver-based medications."
}
```

---

## Architecture

### Request Flow

```
POST /api/claims
      │
      ▼
ClaimsController          (thin — validates, delegates, maps HTTP status)
      │
      ▼
ClaimService              (orchestrates: load → adjudicate → persist)
      │
      ▼
AdjudicationEngine        (two-phase: validate → modify)
      │
      ├── IValidationRule[]   (short-circuit on first rejection)
      └── IModifierRule[]     (pipeline: each modifier sees the quantity left by the previous one)
```

### Adjudication Engine

The engine is the core of the system. It runs in two phases:

1. **Validation** — each `IValidationRule` inspects the claim and either passes or rejects with a reason. The engine short-circuits on the first rejection.
2. **Modification** — each `IModifierRule` returns a `ModificationResult` with a `QuantityDelta`. The engine applies modifiers in sequence as a pipeline — each rule sees the quantity as left by the previous one.

After modification, the engine calculates `TotalCost = FinalQuantity × Medicine.BaseCost`.

### Extensibility

Adding a new rule requires:
1. Create a class implementing `IValidationRule` or `IModifierRule`
2. Register it in `Program.cs` with `AddScoped<IValidationRule, MyNewRule>()`

No existing code changes. The engine, service, and controller are untouched.

### Rules Implemented

| Rule | Type | Behaviour |
|------|------|-----------|
| `SilverAllergyRule` | `IValidationRule` | Rejects claims where the patient is a Werewolf and the medicine contains silver |
| `HydraMultiplierRule` | `IModifierRule` | Multiplies quantity by `Patient.HeadCount` for Hydra patients |

### Project Structure

```
src/CryptidCare.Api/
├── Adjudication/
│   ├── Rules/              # One file per rule
│   ├── AdjudicationEngine.cs
│   ├── IAdjudicationRule.cs   # IValidationRule + IModifierRule
│   ├── AdjudicationResult.cs
│   └── ModificationResult.cs
├── Controllers/
│   └── ClaimsController.cs
├── Data/
│   └── CryptidCareDbContext.cs
├── Middleware/
│   └── ApiKeyMiddleware.cs
├── Domain/
│   ├── Entities/           # Patient, Medicine, Claim
│   └── Enums/              # Species, ClaimStatus
├── DTOs/                   # ClaimRequest, ClaimResponse, ClaimCheckResponse
└── Services/
    └── ClaimService.cs

tests/CryptidCare.Api.Tests/
├── Adjudication/           # Per-rule and engine tests
├── Controllers/            # Controller tests with mocked service
├── Middleware/             # ApiKeyMiddleware tests
└── Services/               # ClaimService tests with EF Core in-memory database
```

---

## Running Locally

For development without Docker for the API itself. Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**1. Install the EF Core CLI tool**

```bash
dotnet tool install --global dotnet-ef
```

**2. Start the database**

```bash
docker compose up -d db
```

**3. Apply migrations**

```bash
dotnet ef database update --project src/CryptidCare.Api --startup-project src/CryptidCare.Api
```

**4. Run the API**

```bash
dotnet run --project src/CryptidCare.Api
```

**5. Run the tests**

```bash
dotnet test
```
