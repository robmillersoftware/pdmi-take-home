using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.DTOs;

public record ClaimResponse(
    int Id,
    ClaimStatus Status,
    int Quantity,
    decimal TotalCost,
    string? RejectionReason);
