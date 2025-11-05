namespace Clinic.Models;

public class Consultation
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime DateTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Notes { get; set; } = null!;
    
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}