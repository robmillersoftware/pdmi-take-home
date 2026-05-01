namespace CryptidCare.Api.Domain.Entities;

public class Medicine
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal BaseCost { get; set; }
    public bool ContainsSilver { get; set; }
}