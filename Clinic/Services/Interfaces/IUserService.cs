using Clinic.Models;

namespace Clinic.Services.Implementations;

public interface IUserService
{
    Task<Doctor?> GetDoctorByIdAsync(int doctorId);
    Task<Patient?> GetPatientByIdAsync(int patientId);
    Task<Doctor> CreateDoctorAsync(Doctor doctor);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task<List<Doctor>> GetAllDoctorsAsync();
    Task<List<Doctor>> GetVerifiedDoctorsAsync();
}