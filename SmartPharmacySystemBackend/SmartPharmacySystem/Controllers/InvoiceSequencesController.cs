using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.InvoiceSequences;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceSequencesController : ControllerBase
{
    private readonly IInvoiceSequenceService _sequenceService;
    public InvoiceSequencesController(IInvoiceSequenceService sequenceService) => _sequenceService = sequenceService;

    [HttpGet("next/{prefix}")]
    public async Task<IActionResult> GetNextNumber(string prefix, [FromQuery] int year)
    {
        var result = await _sequenceService.GetNextNumberAsync(prefix, year);
        return Ok(ApiResponse<InvoiceSequenceDto>.Succeeded(result, "تم إنشاء الرقم التالي"));
    }
}
