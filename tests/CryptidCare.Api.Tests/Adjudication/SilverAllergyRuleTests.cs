using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Tests.Adjudication;

public class SilverAllergyRuleTests
{
    private readonly SilverAllergyRule _sut = new();

    [Fact]
    public void Apply_WerewolfWithSilverMedicine_RejectsWithReason()
    {
        var claim = BuildClaim(Species.Werewolf, containsSilver: true);

        var result = _sut.Apply(claim);

        Assert.True(result.IsRejected);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void Apply_WerewolfWithNonSilverMedicine_Passes()
    {
        var claim = BuildClaim(Species.Werewolf, containsSilver: false);

        var result = _sut.Apply(claim);

        Assert.False(result.IsRejected);
    }

    [Fact]
    public void Apply_NonWerewolfWithSilverMedicine_Passes()
    {
        var claim = BuildClaim(Species.Hydra, containsSilver: true);

        var result = _sut.Apply(claim);

        Assert.False(result.IsRejected);
    }

    private static Claim BuildClaim(Species species, bool containsSilver) => new()
    {
        Patient = new Patient { Species = species, HeadCount = 1 },
        Medicine = new Medicine { ContainsSilver = containsSilver, BaseCost = 10m },
        Quantity = 1
    };
}
