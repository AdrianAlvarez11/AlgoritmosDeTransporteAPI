namespace AlgoritmosDeTransporteAPI.Models;

public sealed class PasoResolucion
{
    public int Numero { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public IReadOnlyList<IReadOnlyList<Celda>> Matriz { get; init; } = [];
    public IReadOnlyList<double> OfertasRestantes { get; init; } = [];
    public IReadOnlyList<double> DemandasRestantes { get; init; } = [];
    public IReadOnlyList<double>? PenalizacionesFilas { get; init; }
    public IReadOnlyList<double>? PenalizacionesColumnas { get; init; }
    public int? FilaSeleccionada { get; init; }
    public int? ColumnaSeleccionada { get; init; }
    public bool OfertaAgotada { get; init; }
    public bool DemandaAgotada { get; init; }
    public bool EsFinal { get; init; }
}
