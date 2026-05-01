using CryptidCare.Api.Controllers;
using CryptidCare.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CryptidCare.Api.Tests.Controllers;

public class ClaimsControllerTests
{
    private readonly IClaimService _claimService = Substitute.For<IClaimService>();
    private readonly ClaimsController _sut;

    public ClaimsControllerTests()
    {
        _sut = new ClaimsController(_claimService);
    }

    [Fact]
    public void Submit_WhenServiceApprovesClaim_ReturnsOk()
    {
        _claimService.SubmitClaim().Returns(true);

        var result = _sut.Submit();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void Submit_WhenServiceRejectsClaim_ReturnsBadRequest()
    {
        _claimService.SubmitClaim().Returns(false);

        var result = _sut.Submit();

        Assert.IsType<BadRequestResult>(result);
    }
}
