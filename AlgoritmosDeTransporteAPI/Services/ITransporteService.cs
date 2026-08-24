using AlgoritmosDeTransporteAPI.DTOs;
using AlgoritmosDeTransporteAPI.Models;

namespace AlgoritmosDeTransporteAPI.Services;

public interface ITransporteService
{
    Problema Resolver(ResolverProblemaRequest request, MetodoTransporte metodo);
}
