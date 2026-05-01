using CryptidCare.Api.Adjudication;
using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Data;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;
using CryptidCare.Api.DTOs;
using CryptidCare.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CryptidCare.Api.Tests.Services;

public class ClaimServiceTests : IDisposable
{
    private readonly CryptidCareDbContext _db;
    private readonly AdjudicationEngine _engine;
    private readonly ClaimService _claimService;

    private readonly Patient _werewolf  = new() { Id = 1, Name = "Lupin",  Species = Species.Werewolf, HeadCount = 1, IsActive = true };
    private readonly Patient _hydra    = new() { Id = 2, Name = "Lernie", Species = Species.Hydra,    HeadCount = 3, IsActive = true };
    private readonly Patient _inactive = new() { Id = 3, Name = "Ghost",  Species = Species.Phoenix,  HeadCount = 1, IsActive = false };
    private readonly Medicine _safe   = new() { Id = 1, Name = "Wolfsbane Tonic",  ContainsSilver = false, BaseCost = 10m };
    private readonly Medicine _silver = new() { Id = 2, Name = "Silvadene Cream",  ContainsSilver = true,  BaseCost = 20m };

    public ClaimServiceTests()
    {
        var options = new DbContextOptionsBuilder<CryptidCareDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new CryptidCareDbContext(options);
        _db.Patients.AddRange(_werewolf, _hydra, _inactive);
        _db.Medicines.AddRange(_safe, _silver);
        _db.SaveChanges();

        _engine = new AdjudicationEngine(
            [new SilverAllergyRule()],
            [new HydraMultiplierRule()]);

        _claimService = new ClaimService(_db, _engine);
    }

    // --- SubmitClaimAsync ---

    [Fact]
    public async Task SubmitClaimAsync_ValidClaim_PersistsClaimToDatabase()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 1, Quantity: 2);

        await _claimService.SubmitClaimAsync(request);

        Assert.Single(_db.Claims);
    }

    [Fact]
    public async Task SubmitClaimAsync_ApprovedClaim_ReturnsApprovedResponse()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 1, Quantity: 2);

        var response = await _claimService.SubmitClaimAsync(request);

        Assert.Equal(ClaimStatus.Approved, response.Status);
        Assert.Null(response.RejectionReason);
    }

    [Fact]
    public async Task SubmitClaimAsync_RejectedClaim_ReturnsRejectedResponseWithReason()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 2, Quantity: 2);

        var response = await _claimService.SubmitClaimAsync(request);

        Assert.Equal(ClaimStatus.Rejected, response.Status);
        Assert.NotNull(response.RejectionReason);
    }

    [Fact]
    public async Task SubmitClaimAsync_HydraClaim_PersistsAdjustedQuantityAndCost()
    {
        var request = new ClaimRequest(PatientId: 2, MedicineId: 1, Quantity: 2);

        var response = await _claimService.SubmitClaimAsync(request);

        Assert.Equal(6, response.Quantity);        // 2 × 3 heads
        Assert.Equal(60m, response.TotalCost);     // 6 × £10
    }

    [Fact]
    public async Task SubmitClaimAsync_UnknownPatient_ThrowsKeyNotFoundException()
    {
        var request = new ClaimRequest(PatientId: 99, MedicineId: 1, Quantity: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _claimService.SubmitClaimAsync(request));
    }

    [Fact]
    public async Task SubmitClaimAsync_UnknownMedicine_ThrowsKeyNotFoundException()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 99, Quantity: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _claimService.SubmitClaimAsync(request));
    }

    [Fact]
    public async Task SubmitClaimAsync_InactivePatient_ThrowsInvalidOperationException()
    {
        var request = new ClaimRequest(PatientId: 3, MedicineId: 1, Quantity: 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _claimService.SubmitClaimAsync(request));
    }

    // --- CheckClaimAsync ---

    [Fact]
    public async Task CheckClaimAsync_DoesNotPersistAnything()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 1, Quantity: 2);

        await _claimService.CheckClaimAsync(request);

        Assert.Empty(_db.Claims);
    }

    [Fact]
    public async Task CheckClaimAsync_WouldBeRejected_ReturnsRejectedWithReason()
    {
        var request = new ClaimRequest(PatientId: 1, MedicineId: 2, Quantity: 2);

        var response = await _claimService.CheckClaimAsync(request);

        Assert.Equal(ClaimStatus.Rejected, response.Status);
        Assert.NotNull(response.RejectionReason);
    }

    [Fact]
    public async Task CheckClaimAsync_HydraClaim_ReturnsAdjustedQuantityAndCost()
    {
        var request = new ClaimRequest(PatientId: 2, MedicineId: 1, Quantity: 2);

        var response = await _claimService.CheckClaimAsync(request);

        Assert.Equal(6, response.Quantity);
        Assert.Equal(60m, response.TotalCost);
    }

    public void Dispose() => _db.Dispose();
}
