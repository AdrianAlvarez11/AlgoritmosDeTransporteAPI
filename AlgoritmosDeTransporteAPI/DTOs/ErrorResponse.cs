namespace AlgoritmosDeTransporteAPI.DTOs;

public sealed class ErrorResponse
{
    public IReadOnlyList<string> Errores { get; init; } = [];
}
