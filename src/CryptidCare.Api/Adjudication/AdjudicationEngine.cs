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

        // Apply modifier rules. They all return a delta to add to the quantity or 0
        var totalDelta = modifierRules.Sum(modifier => modifier.Apply(claim).QuantityDelta);
        var finalQuantity = claim.Quantity + totalDelta;

        claim.Quantity = finalQuantity;
        claim.TotalCost = finalQuantity * claim.Medicine.BaseCost;

        return AdjudicationResult.Pass();
    }
}
