using CryptidCare.Api.Domain.Entities;

namespace CryptidCare.Api.Adjudication;

public class AdjudicationEngine(
    IEnumerable<IValidationRule> validationRules,
    IEnumerable<IModifierRule> modifierRules)
{
    public AdjudicationResult Execute(Claim claim)
    {
        foreach (var rule in validationRules)
        {
            var result = rule.Apply(claim);
            if (result.IsRejected) return result;
        }

        var finalQuantity = claim.Quantity;
        foreach (var modifier in modifierRules)
        {
            var mod = modifier.Apply(claim);
            finalQuantity *= mod.QuantityMultiplier;
        }

        claim.Quantity = finalQuantity;
        claim.TotalCost = finalQuantity * claim.Medicine.BaseCost;

        return AdjudicationResult.Pass();
    }
}
