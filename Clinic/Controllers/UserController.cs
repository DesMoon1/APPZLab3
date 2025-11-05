using Clinic.Models;
using Clinic.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("doctor")]
    public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
    {
        doctor.Verified = false;
        var created = await _userService.CreateDoctorAsync(doctor);
        return Ok(created);
    }

    [HttpPost("patient")]
    public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
    {
        var created = await _userService.CreatePatientAsync(patient);
        return Ok(created);
    }

    [HttpGet("doctor/{id}")]
    public async Task<IActionResult> GetDoctor(int id)
    {
        var doctor = await _userService.GetDoctorByIdAsync(id);
        return doctor == null ? NotFound() : Ok(doctor);
    }
    
    [HttpGet("doctor")]
    public async Task<IActionResult> GetAllDoctor()
    {
        var doctor = await _userService.GetAllDoctorsAsync();
        return doctor == null ? NotFound() : Ok(doctor);
    }

    [HttpGet("verified")]
    public async Task<IActionResult> GetVerifiedDoctors()
    {
        var doctors = await _userService.GetVerifiedDoctorsAsync();
        return doctors == null ? NotFound() : Ok(doctors);
    }

    [HttpGet("patient/{id}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await _userService.GetPatientByIdAsync(id);
        return patient == null ? NotFound() : Ok(patient);
    }
}