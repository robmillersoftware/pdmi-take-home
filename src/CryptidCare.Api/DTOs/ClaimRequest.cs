namespace CryptidCare.Api.DTOs;

public record ClaimRequest(int PatientId, int MedicineId, int Quantity);
