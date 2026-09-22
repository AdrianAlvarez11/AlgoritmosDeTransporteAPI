using AlgoritmosDeTransporteAPI.Models;

namespace AlgoritmosDeTransporteAPI.Services;

public interface IPdfService
{
    byte[] GenerarReporte(Problema problema);
}
