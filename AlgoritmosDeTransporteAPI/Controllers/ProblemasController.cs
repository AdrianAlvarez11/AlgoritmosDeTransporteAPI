using AlgoritmosDeTransporteAPI.DTOs;
using AlgoritmosDeTransporteAPI.Services;
using AlgoritmosDeTransporteAPI.Validators;
using Microsoft.AspNetCore.Mvc;

namespace AlgoritmosDeTransporteAPI.Controllers;

[ApiController]
[Route("api/problemas")]
public sealed class ProblemasController : ControllerBase
{
    private readonly ITransporteService _transporteService;
    private readonly IPdfService _pdfService;
    private readonly IResolverProblemaRequestValidator _validator;

    public ProblemasController(
        ITransporteService transporteService,
        IPdfService pdfService,
        IResolverProblemaRequestValidator validator)
    {
        _transporteService = transporteService;
        _pdfService = pdfService;
        _validator = validator;
    }

    [HttpPost("resolver")]
    public IActionResult Resolver([FromBody] ResolverProblemaRequest request)
    {
        var validation = _validator.Validate(request, out var metodo);
        if (!validation.IsValid)
        {
            return BadRequest(new ErrorResponse { Errores = validation.Errors });
        }

        return Ok(_transporteService.Resolver(request, metodo));
    }

    [HttpPost("reporte")]
    public IActionResult GenerarReporte([FromBody] ResolverProblemaRequest request)
    {
        var validation = _validator.Validate(request, out var metodo);
        if (!validation.IsValid)
        {
            return BadRequest(new ErrorResponse { Errores = validation.Errors });
        }

        var requestConPasos = new ResolverProblemaRequest
        {
            Metodo = request.Metodo,
            Costos = request.Costos,
            Ofertas = request.Ofertas,
            Demandas = request.Demandas,
            IncluirPasos = true
        };

        var problema = _transporteService.Resolver(requestConPasos, metodo);
        var pdf = _pdfService.GenerarReporte(problema);

        return File(pdf, "application/pdf", "solucion-transporte.pdf");
    }
}
