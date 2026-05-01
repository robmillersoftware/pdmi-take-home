# Cryptid-Care Prescription Processor

A .NET 10 Web API that adjudicates pharmacy claims for mythical beings.

## Running Locally

**1. Start the database**

```bash
docker compose up -d
```

**2. Apply migrations**

```bash
dotnet ef database update --project src/CryptidCare.Api --startup-project src/CryptidCare.Api
```

**3. Run the API**

```bash
dotnet run --project src/CryptidCare.Api
```

The API listens on `http://localhost:5043`.

**4. Run the tests**

```bash
dotnet test
```

---

## API Endpoints

### `POST /api/claims`

Submit a claim for adjudication. The claim is persisted and the final outcome returned.

**Request**
```json
{
  "patientId": 1,
  "medicineId": 2,
  "quantity": 3
}
```

**Response — Approved `200 OK`**
```json
{
  "id": 42,
  "status": "Approved",
  "quantity": 9,
  "totalCost": 270.00,
  "rejectionReason": null
}
```

**Response — Rejected `400 Bad Request`**
```json
{
  "id": 43,
  "status": "Rejected",
  "quantity": 3,
  "totalCost": 0,
  "rejectionReason": "Werewolves cannot be prescribed silver-based medications."
}
```

---

### `POST /api/claims/check` *(Wildcard Feature)*

Dry-run adjudication without persisting anything. Useful for pharmacies to validate a claim before formally submitting it. Always returns `200 OK` — the outcome is in the response body.

**Request** — same shape as `/api/claims`

**Response**
```json
{
  "status": "Rejected",
  "quantity": 3,
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
├── Domain/
│   ├── Entities/           # Patient, Medicine, Claim
│   └── Enums/              # Species, ClaimStatus
├── DTOs/                   # ClaimRequest, ClaimResponse, ClaimCheckResponse
└── Services/
    └── ClaimService.cs

tests/CryptidCare.Api.Tests/
├── Adjudication/           # Per-rule and engine tests
├── Controllers/            # Controller tests with mocked service
└── Services/               # ClaimService tests with EF Core in-memory database
```
