# Cryptid-Care Prescription Processor

A .NET 10 Web API that adjudicates pharmacy claims for mythical beings.

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- `dotnet-ef` global tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Getting Started

**1. Start the database**

```bash
docker compose up -d
```

**2. Apply migrations**

```bash
dotnet ef database update --project src/CryptidCare.Api --startup-project src/CryptidCare.Api
```

**3. Seed test data**

There is no automatic seeder — insert test records directly. Connect to the database with any SQL client (e.g. Azure Data Studio, `sqlcmd`, or the Docker exec approach below) using:

- Host: `localhost,1433`
- User: `sa`
- Password: `CryptidCare_Dev1`

Or run SQL directly via Docker:

```bash
docker exec -i $(docker compose ps -q db) /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'CryptidCare_Dev1' -C -d CryptidCare -Q "
INSERT INTO Patients (Name, Species, HeadCount, IsActive) VALUES
  ('Gary Lupine',   0, 1, 1),
  ('Harriet Hydra', 2, 3, 1);

INSERT INTO Medicines (Name, ContainsSilver, BaseCost) VALUES
  ('Wolfsbane Tonic',    0, 30.00),
  ('Silver Salve',       1, 90.00);
"
```

This inserts:

| Id | Patient        | Species  | Heads | Active |
|----|----------------|----------|-------|--------|
| 1  | Gary Lupine    | Werewolf | 1     | Yes    |
| 2  | Harriet Hydra  | Hydra    | 3     | Yes    |

| Id | Medicine         | Contains Silver | Base Cost |
|----|------------------|-----------------|-----------|
| 1  | Wolfsbane Tonic  | No              | $30.00    |
| 2  | Silver Salve     | Yes             | $90.00    |

> **Species enum values:** Werewolf = 0, Phoenix = 1, Hydra = 2

**4. Run the API**

```bash
dotnet run --project src/CryptidCare.Api
```

The API listens on `http://localhost:5043`.

**5. Run the tests**

```bash
dotnet test
```

---

## Authentication

All requests require an `X-Api-Key` header. The dev key is pre-configured in `appsettings.Development.json`:

```
X-Api-Key: cryptid-care-dev-key
```

Requests without a valid key receive `401 Unauthorized`.

---

## End-to-End Examples

### Approved claim — Werewolf, non-silver medicine

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

---

### Rejected claim — Werewolf + silver medicine

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

---

### Hydra claim — quantity multiplied by head count (3 heads × 4 = 12)

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

---

### Dry-run check (wildcard feature) — no persistence

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

Always returns `200 OK` — the outcome is in the response body.

---

### Missing API key — 401

```bash
curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:5043/api/claims \
  -H "Content-Type: application/json" \
  -d '{"patientId": 1, "medicineId": 1, "quantity": 1}'
```

```
401
```

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

**Response — Approved `200 OK`**
```json
{
  "id": 1,
  "status": "Approved",
  "quantity": 5,
  "totalCost": 150.00,
  "rejectionReason": null
}
```

**Response — Rejected `400 Bad Request`**
```json
{
  "id": 2,
  "status": "Rejected",
  "quantity": 5,
  "totalCost": 0,
  "rejectionReason": "Werewolves cannot be prescribed silver-based medications."
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

Dry-run adjudication without persisting anything. Useful for pharmacies to validate a claim before formally submitting it. Always returns `200 OK` — the outcome is in the response body.

**Request** — same shape as `/api/claims`

**Response**
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
      └── IModifierRule[]     (collect modifications, engine applies them)
```

### Adjudication Engine

The engine is the core of the system. It runs in two phases:

1. **Validation** — each `IValidationRule` inspects the claim and either passes or rejects with a reason. The engine short-circuits on the first rejection.
2. **Modification** — each `IModifierRule` returns a `ModificationResult` describing adjustments (e.g. a quantity multiplier). The engine accumulates these and applies them — rules never mutate state directly.

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
