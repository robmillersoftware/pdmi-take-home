using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Tests.Adjudication;

public class HydraMultiplierRuleTests
{
    private readonly HydraMultiplierRule _sut = new();

    [Fact]
    public void Apply_Hydra_ReturnsDeltaToScaleByHeadCount()
    {
        // quantity=3, headCount=5 → final should be 15 → delta = 3 * (5-1) = 12
        var claim = BuildClaim(Species.Hydra, headCount: 5);

        var result = _sut.Apply(claim);

        Assert.Equal(12, result.QuantityDelta);
    }

    [Fact]
    public void Apply_NonHydra_ReturnsZeroDelta()
    {
        var claim = BuildClaim(Species.Werewolf, headCount: 1);

        var result = _sut.Apply(claim);

        Assert.Equal(0, result.QuantityDelta);
    }

    private static Claim BuildClaim(Species species, int headCount) => new()
    {
        Patient = new Patient { Species = species, HeadCount = headCount },
        Medicine = new Medicine { BaseCost = 10m },
        Quantity = 3
    };
}
