using Microsoft.AspNetCore.Mvc;
using VerificationService.Model;
using VerificationService.Services;

namespace VerificationService.Controller;

[ApiController]
[Route("api/verification")]
public class DoctorVerificationController : ControllerBase
{
    private readonly DoctorVerificationService _verificationService;

    public DoctorVerificationController(DoctorVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpGet("pending")]
    public ActionResult<List<DoctorVerification>> GetPendingVerifications()
    {
        var pending = _verificationService.GetPendingVerifications();
        return Ok(pending);
    }

    [HttpPost("verify/{doctorId}")]
    public async Task<ActionResult> VerifyDoctor(int doctorId, [FromQuery] bool isVerified = true)
    {
        await _verificationService.VerifyDoctorAsync(doctorId, isVerified);
        return Ok(new { DoctorId = doctorId, IsVerified = isVerified });
    }

    [HttpPost("deny/{doctorId}")]
    public async Task<ActionResult> DenyDoctor(int doctorId)
    {
        return Ok(new { DoctorId = doctorId, IsVerified = false });
    }
}