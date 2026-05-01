namespace CryptidCare.Api.Adjudication;

public record AdjudicationResult
{
    public bool IsRejected { get; init; }
    public string? RejectionReason { get; init; }

    public static AdjudicationResult Pass() => new();
    public static AdjudicationResult Reject(string reason) => new() { IsRejected = true, RejectionReason = reason };
}
