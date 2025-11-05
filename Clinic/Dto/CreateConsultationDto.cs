namespace Clinic.Dto;

public class CreateConsultationDto
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime DateTime { get; set; }
}