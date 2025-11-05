using Clinic.Data;
using Clinic.Models;
using Microsoft.EntityFrameworkCore;
using Clinic.Events;
using RabbitMQ;

namespace Clinic.Services.Implementations;

public class UserService : IUserService
{
    private readonly ClinicDbContext _context;
    private readonly RabbitMQPublisher _publisher;

    public UserService(ClinicDbContext context, RabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        var d = _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var evt = new DoctorRegistrationEvent
        {
            DoctorId = d.Entity.Id,
            FullName = doctor.FullName,
            Certificate = doctor.Certificates,
            Specialization = doctor.Specialization,
            RegistrationDate = DateTime.UtcNow
        };


        await _publisher.PublishAsync("doctor_registration", evt);

        return doctor;
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        patient.DateOfBirth = patient.DateOfBirth.ToUniversalTime();

        var p = _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return p.Entity;
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
    {
        return await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public async Task<List<Doctor>> GetAllDoctorsAsync()
    {
        return await _context.Doctors.ToListAsync();
    }

    public async Task<List<Doctor>> GetVerifiedDoctorsAsync()
    {
        return await _context.Doctors.Where(d => d.Verified).ToListAsync();
    }
}