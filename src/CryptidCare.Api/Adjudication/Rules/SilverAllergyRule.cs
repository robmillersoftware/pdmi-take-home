using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Adjudication.Rules;

public class SilverAllergyRule : IValidationRule
{
    public AdjudicationResult Apply(Claim claim)
    {
        if (claim.Patient.Species == Species.Werewolf && claim.Medicine.ContainsSilver)
            return AdjudicationResult.Reject("Werewolves cannot be prescribed silver-based medications.");

        return AdjudicationResult.Pass();
    }
}
