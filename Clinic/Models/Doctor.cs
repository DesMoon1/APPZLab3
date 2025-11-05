namespace Clinic.Models;

public class Doctor
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string Certificates { get; set; } = null!;
    public bool Verified { get; set; } = false;
}