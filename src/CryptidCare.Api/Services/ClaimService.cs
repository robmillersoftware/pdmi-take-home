using CryptidCare.Api.Adjudication;
using CryptidCare.Api.Data;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;
using CryptidCare.Api.DTOs;

namespace CryptidCare.Api.Services;

public class ClaimService(CryptidCareDbContext db, AdjudicationEngine engine) : IClaimService
{
    public async Task<ClaimResponse> SubmitClaimAsync(ClaimRequest request)
    {
        var claim = await BuildClaimAsync(request);

        var result = engine.Execute(claim);
        claim.Status = result.IsRejected ? ClaimStatus.Rejected : ClaimStatus.Approved;
        claim.RejectionReason = result.RejectionReason;

        db.Claims.Add(claim);
        await db.SaveChangesAsync();

        return new ClaimResponse(claim.Id, claim.Status, claim.Quantity, claim.TotalCost, claim.RejectionReason);
    }

    public async Task<ClaimCheckResponse> CheckClaimAsync(ClaimRequest request)
    {
        var claim = await BuildClaimAsync(request);

        var result = engine.Execute(claim);
        var status = result.IsRejected ? ClaimStatus.Rejected : ClaimStatus.Approved;

        return new ClaimCheckResponse(status, claim.Quantity, claim.TotalCost, result.RejectionReason);
    }

    private async Task<Claim> BuildClaimAsync(ClaimRequest request)
    {
        var patient = await db.Patients.FindAsync(request.PatientId)
            ?? throw new KeyNotFoundException($"Patient {request.PatientId} not found.");

        var medicine = await db.Medicines.FindAsync(request.MedicineId)
            ?? throw new KeyNotFoundException($"Medicine {request.MedicineId} not found.");

        return new Claim
        {
            PatientId = request.PatientId,
            Patient = patient,
            MedicineId = request.MedicineId,
            Medicine = medicine,
            Quantity = request.Quantity,
            Status = ClaimStatus.Pending
        };
    }
}
