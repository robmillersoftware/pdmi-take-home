using CryptidCare.Api.Controllers;
using CryptidCare.Api.Domain.Enums;
using CryptidCare.Api.DTOs;
using CryptidCare.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CryptidCare.Api.Tests.Controllers;

public class ClaimsControllerTests
{
    private readonly IClaimService _claimService = Substitute.For<IClaimService>();
    private readonly ClaimsController _sut;
    private readonly ClaimRequest _request = new(PatientId: 1, MedicineId: 1, Quantity: 2);

    public ClaimsControllerTests()
    {
        _sut = new ClaimsController(_claimService);
    }

    [Fact]
    public async Task Submit_WhenClaimApproved_ReturnsOkWithResponse()
    {
        var response = new ClaimResponse(1, ClaimStatus.Approved, 2, 20m, null);
        _claimService.SubmitClaimAsync(_request).Returns(response);

        var result = await _sut.Submit(_request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Submit_WhenClaimRejected_ReturnsBadRequestWithResponse()
    {
        var response = new ClaimResponse(1, ClaimStatus.Rejected, 2, 0m, "Werewolves cannot be prescribed silver-based medications.");
        _claimService.SubmitClaimAsync(_request).Returns(response);

        var result = await _sut.Submit(_request);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(response, bad.Value);
    }

    [Fact]
    public async Task Submit_WhenPatientOrMedicineNotFound_ReturnsNotFound()
    {
        _claimService.SubmitClaimAsync(_request).Throws(new KeyNotFoundException("Patient 1 not found."));

        var result = await _sut.Submit(_request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Check_WhenClaimWouldBeApproved_ReturnsOkWithResponse()
    {
        var response = new ClaimCheckResponse(ClaimStatus.Approved, 2, 20m, null);
        _claimService.CheckClaimAsync(_request).Returns(response);

        var result = await _sut.Check(_request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Check_WhenClaimWouldBeRejected_ReturnsOkWithResponse()
    {
        var response = new ClaimCheckResponse(ClaimStatus.Rejected, 2, 0m, "Werewolves cannot be prescribed silver-based medications.");
        _claimService.CheckClaimAsync(_request).Returns(response);

        var result = await _sut.Check(_request);

        // Check always returns 200 — the status is in the response body
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Check_WhenPatientOrMedicineNotFound_ReturnsNotFound()
    {
        _claimService.CheckClaimAsync(_request).Throws(new KeyNotFoundException("Medicine 1 not found."));

        var result = await _sut.Check(_request);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
