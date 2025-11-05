namespace VerificationService.Model;

public class DoctorVerification
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public DateTime RegisteredAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}