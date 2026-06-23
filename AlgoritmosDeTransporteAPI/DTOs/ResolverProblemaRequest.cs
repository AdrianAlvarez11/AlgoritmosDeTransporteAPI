namespace AlgoritmosDeTransporteAPI.DTOs;

public sealed class ResolverProblemaRequest
{
    public string Metodo { get; init; } = string.Empty;
    public decimal[][] Costos { get; init; } = [];
    public double[] Ofertas { get; init; } = [];
    public double[] Demandas { get; init; } = [];
    public bool IncluirPasos { get; init; } = true;
}
