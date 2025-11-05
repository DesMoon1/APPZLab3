using System;
using Clinic.Data;
using Clinic.Models;
using Clinic.Events;
using RabbitMQ;

namespace Clinic.Services.Implementations;

public class ConsultationService : IConsultationService
{
    private readonly ClinicDbContext _context;
    private readonly RabbitMQPublisher _publisher;

    public ConsultationService(ClinicDbContext context, RabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Consultation> CreateConsultationAsync(int doctorId, int patientId, DateTime dateTime)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        var patient = await _context.Patients.FindAsync(patientId);

        if (doctor == null || patient == null)
            throw new Exception("Doctor or patient not found");

        var consultation = new Consultation
        {
            DoctorId = doctorId,
            PatientId = patientId,
            DateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            Status = "Scheduled"
        };

        var c = _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        var evt = new ConsultationCreatedEvent
        {
            ConsultationId = c.Entity.Id,
            DoctorId = doctorId,
            PatientId = patientId,
            DateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        };

        await _publisher.PublishAsync("consultation_created", evt);

        return consultation;
    }
}