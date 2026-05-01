using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Adjudication.Rules;

public class HydraMultiplierRule : IModifierRule
{
    public ModificationResult Apply(Claim claim)
    {
        if (claim.Patient.Species == Species.Hydra)
            return new ModificationResult { QuantityMultiplier = claim.Patient.HeadCount };

        return new ModificationResult();
    }
}
