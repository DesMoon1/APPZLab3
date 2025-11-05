using Clinic.Dto;
using Clinic.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Controllers;


[ApiController]
[Route("api/consultations")]
public class ConsultationController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateConsultation([FromBody] CreateConsultationDto dto)
    {
        try
        {
            var consultation = await _consultationService.CreateConsultationAsync(dto.DoctorId, dto.PatientId, dto.DateTime);
            return Ok(consultation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}