using Clinic.Models;

namespace Clinic.Services.Implementations;

public interface IConsultationService
{
    Task<Consultation> CreateConsultationAsync(int doctorId, int patientId, DateTime dateTime);
}