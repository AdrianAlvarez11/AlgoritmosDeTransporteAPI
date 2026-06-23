using AlgoritmosDeTransporteAPI.DTOs;
using AlgoritmosDeTransporteAPI.Domain;

namespace AlgoritmosDeTransporteAPI.Validators;

public interface IResolverProblemaRequestValidator
{
    ValidationResult Validate(ResolverProblemaRequest request, out MetodoTransporte metodo);
}

public sealed class ResolverProblemaRequestValidator : IResolverProblemaRequestValidator
{
    public ValidationResult Validate(ResolverProblemaRequest request, out MetodoTransporte metodo)
    {
        var result = new ValidationResult();

        if (!TryParseMetodo(request.Metodo, out metodo))
        {
            result.Errors.Add("El metodo debe ser 'Esquina noroeste', 'Costo minimo' o 'Vogel'.");
        }

        if (request.Costos.Length < 2)
        {
            result.Errors.Add("La matriz de costos debe tener al menos 2 filas.");
            return result;
        }

        var columnas = request.Costos[0].Length;
        if (columnas < 2)
        {
            result.Errors.Add("La matriz de costos debe tener al menos 2 columnas.");
        }

        for (var i = 0; i < request.Costos.Length; i++)
        {
            if (request.Costos[i].Length != columnas)
            {
                result.Errors.Add("Todas las filas de la matriz de costos deben tener la misma cantidad de columnas.");
                break;
            }

            for (var j = 0; j < request.Costos[i].Length; j++)
            {
                if (request.Costos[i][j] < 0)
                {
                    result.Errors.Add($"El costo en la posicion ({i + 1}, {j + 1}) no puede ser negativo.");
                }
            }
        }

        if (request.Ofertas.Length != request.Costos.Length)
        {
            result.Errors.Add("La cantidad de ofertas debe coincidir con el numero de filas.");
        }

        if (request.Demandas.Length != columnas)
        {
            result.Errors.Add("La cantidad de demandas debe coincidir con el numero de columnas.");
        }

        AddQuantityErrors(request.Ofertas, "oferta", result);
        AddQuantityErrors(request.Demandas, "demanda", result);

        return result;
    }

    private static void AddQuantityErrors(double[] values, string label, ValidationResult result)
    {
        if (values.Length == 0)
        {
            result.Errors.Add($"Debe indicar al menos una {label}.");
            return;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (double.IsNaN(values[i]) || double.IsInfinity(values[i]) || values[i] < 0)
            {
                result.Errors.Add($"La {label} {i + 1} debe ser un numero finito mayor o igual a cero.");
            }
        }

        if (values.Sum() <= 0)
        {
            result.Errors.Add($"La suma de {label}s debe ser mayor que cero.");
        }
    }

    public static bool TryParseMetodo(string value, out MetodoTransporte metodo)
    {
        var normalized = Normalize(value);
        metodo = normalized switch
        {
            "esquinanoroeste" or "noroeste" => MetodoTransporte.EsquinaNoroeste,
            "costominimo" or "minimo" => MetodoTransporte.CostoMinimo,
            "vogel" or "voguel" => MetodoTransporte.Vogel,
            _ => default
        };

        return normalized is "esquinanoroeste" or "noroeste" or "costominimo" or "minimo" or "vogel" or "voguel";
    }

    private static string Normalize(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u");
    }
}
