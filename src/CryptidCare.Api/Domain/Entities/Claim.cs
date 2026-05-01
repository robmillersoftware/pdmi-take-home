using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.Domain.Entities;

public class Claim
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public ClaimStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}