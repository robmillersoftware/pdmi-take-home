using CryptidCare.Api.Domain.Enums;

namespace CryptidCare.Api.DTOs;

public record ClaimCheckResponse(
    ClaimStatus Status,
    int Quantity,
    decimal TotalCost,
    string? RejectionReason);
