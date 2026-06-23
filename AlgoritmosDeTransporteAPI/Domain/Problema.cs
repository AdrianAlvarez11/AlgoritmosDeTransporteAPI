namespace AlgoritmosDeTransporteAPI.Domain;

public sealed class Problema
{
    public MetodoTransporte Metodo { get; init; }
    public IReadOnlyList<IReadOnlyList<Celda>> Matriz { get; init; } = [];
    public IReadOnlyList<double> Ofertas { get; init; } = [];
    public IReadOnlyList<double> Demandas { get; init; } = [];
    public decimal CostoTotal { get; init; }
    public bool EstaBalanceado { get; init; }
    public string? BalanceAgregado { get; init; }
    public IReadOnlyList<PasoResolucion> Pasos { get; init; } = [];
}
