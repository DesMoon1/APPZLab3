namespace Clinic.Events;

public class DoctorVerifiedEvent
{
    public int DoctorId { get; set; }
    public bool IsVerified { get; set; }
    public DateTime VerificationDate { get; set; }
}