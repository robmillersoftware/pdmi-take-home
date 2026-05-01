using CryptidCare.Api.DTOs;

namespace CryptidCare.Api.Services;

public interface IClaimService
{
    Task<ClaimResponse> SubmitClaimAsync(ClaimRequest request);
    Task<ClaimCheckResponse> CheckClaimAsync(ClaimRequest request);
}
