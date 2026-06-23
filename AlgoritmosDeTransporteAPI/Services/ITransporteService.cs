using AlgoritmosDeTransporteAPI.Domain;
using AlgoritmosDeTransporteAPI.DTOs;

namespace AlgoritmosDeTransporteAPI.Services;

public interface ITransporteService
{
    Problema Resolver(ResolverProblemaRequest request, MetodoTransporte metodo);
}
