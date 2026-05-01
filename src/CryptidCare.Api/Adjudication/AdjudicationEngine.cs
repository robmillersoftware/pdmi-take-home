using CryptidCare.Api.Domain.Entities;

namespace CryptidCare.Api.Adjudication;

public class AdjudicationEngine(
    IEnumerable<IValidationRule> validationRules,
    IEnumerable<IModifierRule> modifierRules)
{
    public AdjudicationResult Execute(Claim claim)
    {
        // Apply validation rules first
        foreach (var rule in validationRules)
        {
            var result = rule.Apply(claim);
            if (result.IsRejected) return result;
        }

        // Pipeline: each modifier sees the quantity as left by the previous one
        foreach (var modifier in modifierRules)
        {
            claim.Quantity += modifier.Apply(claim).QuantityDelta;
        }

        claim.TotalCost = claim.Quantity * claim.Medicine.BaseCost;

        return AdjudicationResult.Pass();
    }
}
