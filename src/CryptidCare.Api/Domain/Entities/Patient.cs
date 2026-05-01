using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Domain.Entities;

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Species Species { get; set; }
    public int HeadCount { get; set; }
    public bool IsActive { get; set; }
}