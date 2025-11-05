namespace VerificationService.Event;

public class DoctorRegistrationEvent
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
}