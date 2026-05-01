using CryptidCare.Api.Adjudication;
using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Tests.Adjudication;

public class AdjudicationEngineTests
{
    [Fact]
    public void Execute_WerewolfWithSilverMedicine_RejectsClaim()
    {
        var engine = BuildEngine();
        var claim = BuildClaim(Species.Werewolf, containsSilver: true, quantity: 2, headCount: 1);

        var result = engine.Execute(claim);

        Assert.True(result.IsRejected);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void Execute_HydraWithValidMedicine_MultipliesQuantityByHeadCount()
    {
        var engine = BuildEngine();
        var claim = BuildClaim(Species.Hydra, containsSilver: false, quantity: 2, headCount: 3);

        var result = engine.Execute(claim);

        Assert.False(result.IsRejected);
        Assert.Equal(6, claim.Quantity);
    }

    [Fact]
    public void Execute_HydraWithValidMedicine_CalculatesTotalCostFromAdjustedQuantity()
    {
        var engine = BuildEngine();
        var claim = BuildClaim(Species.Hydra, containsSilver: false, quantity: 2, headCount: 3, baseCost: 10m);

        engine.Execute(claim);

        Assert.Equal(60m, claim.TotalCost);
    }

    [Fact]
    public void Execute_WerewolfWithValidMedicine_ApprovesWithUnmodifiedQuantity()
    {
        var engine = BuildEngine();
        var claim = BuildClaim(Species.Werewolf, containsSilver: false, quantity: 4, headCount: 1);

        var result = engine.Execute(claim);

        Assert.False(result.IsRejected);
        Assert.Equal(4, claim.Quantity);
    }

    private static AdjudicationEngine BuildEngine() => new(
        [new SilverAllergyRule()],
        [new HydraMultiplierRule()]);

    private static Claim BuildClaim(
        Species species, bool containsSilver, int quantity, int headCount, decimal baseCost = 10m) => new()
    {
        Patient = new Patient { Species = species, HeadCount = headCount },
        Medicine = new Medicine { ContainsSilver = containsSilver, BaseCost = baseCost },
        Quantity = quantity
    };
}
