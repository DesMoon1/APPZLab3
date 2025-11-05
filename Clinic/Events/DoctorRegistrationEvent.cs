namespace Clinic.Events;

public class DoctorRegistrationEvent
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;

    public string Specialization { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
}