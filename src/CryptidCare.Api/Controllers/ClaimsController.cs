using Microsoft.AspNetCore.Mvc;                                                                                      
using CryptidCare.Api.Services;                                                                                      
                                                                                                                       
namespace CryptidCare.Api.Controllers;                                                                             
                                                                                                                       
[ApiController]
[Route("api/[controller]")]                                                                                          
public class ClaimsController : ControllerBase                             
{                                           
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)                                                              
    {
        _claimService = claimService;                                                                                
    }                                                                                                              

    [HttpPost]                                                                                                       
    public IActionResult Submit()
    {                                                                                                                
        var result = _claimService.SubmitClaim();                                                                  
        return result ? Ok() : BadRequest();
    }                                       
} 
