namespace NotificationService.Events;

public class ConsultationCreatedEvent
{
    public int ConsultationId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime DateTime { get; set; }
}