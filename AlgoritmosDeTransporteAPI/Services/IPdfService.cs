using AlgoritmosDeTransporteAPI.Domain;

namespace AlgoritmosDeTransporteAPI.Services;

public interface IPdfService
{
    byte[] GenerarReporte(Problema problema);
}
