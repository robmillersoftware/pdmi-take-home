using CryptidCare.Api.Domain.Enums;
using CryptidCare.Api.DTOs;
using CryptidCare.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptidCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(ClaimRequest request)
    {
        try
        {
            var response = await claimService.SubmitClaimAsync(request);

            return response.Status == ClaimStatus.Rejected
                ? BadRequest(response)
                : Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("check")]
    public async Task<IActionResult> Check(ClaimRequest request)
    {
        try
        {
            var response = await claimService.CheckClaimAsync(request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
