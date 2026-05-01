using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Tests.Adjudication;

public class HydraMultiplierRuleTests
{
    private readonly HydraMultiplierRule _sut = new();

    [Fact]
    public void Apply_Hydra_ReturnsHeadCountAsMultiplier()
    {
        var claim = BuildClaim(Species.Hydra, headCount: 5);

        var result = _sut.Apply(claim);

        Assert.Equal(5, result.QuantityMultiplier);
    }

    [Fact]
    public void Apply_NonHydra_ReturnsMultiplierOfOne()
    {
        var claim = BuildClaim(Species.Werewolf, headCount: 1);

        var result = _sut.Apply(claim);

        Assert.Equal(1, result.QuantityMultiplier);
    }

    private static Claim BuildClaim(Species species, int headCount) => new()
    {
        Patient = new Patient { Species = species, HeadCount = headCount },
        Medicine = new Medicine { BaseCost = 10m },
        Quantity = 3
    };
}
