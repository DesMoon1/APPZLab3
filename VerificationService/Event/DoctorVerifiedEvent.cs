namespace VerificationService.Event;

public class DoctorVerifiedEvent
{
    public int DoctorId { get; set; }
    public bool IsVerified { get; set; }
    public DateTime VerificationDate { get; set; }
}