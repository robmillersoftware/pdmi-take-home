using CryptidCare.Api.Domain.Entities;

namespace CryptidCare.Api.Adjudication;

public interface IValidationRule
{
    AdjudicationResult Apply(Claim claim);
}

public interface IModifierRule
{
    ModificationResult Apply(Claim claim);
}
