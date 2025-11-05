using Clinic.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;
}