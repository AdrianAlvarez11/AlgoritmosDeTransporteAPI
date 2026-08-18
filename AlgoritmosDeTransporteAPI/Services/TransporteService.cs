using AlgoritmosDeTransporteAPI.Domain;
using AlgoritmosDeTransporteAPI.DTOs;

namespace AlgoritmosDeTransporteAPI.Services;

public sealed class TransporteService : ITransporteService
{
    private const double Tolerance = 0.0000001;

    public Problema Resolver(ResolverProblemaRequest request, MetodoTransporte metodo)
    {
        var contexto = CrearContextoBalanceado(request, metodo);

        AgregarPaso(
            contexto,
            "balanceo",
            "Revision de balance",
            contexto.BalanceMessage,
            includePenalties: false);

        switch (metodo)
        {
            case MetodoTransporte.EsquinaNoroeste:
                ResolverPorEsquinaNoroeste(contexto);
                break;
            case MetodoTransporte.CostoMinimo:
                ResolverPorCostoMinimo(contexto);
                break;
            case MetodoTransporte.Vogel:
                ResolverPorVogel(contexto);
                break;
            default:
                throw new InvalidOperationException("Metodo no soportado.");
        }

        var costoTotal = CalcularCostoTotal(contexto);
        AgregarPaso(
            contexto,
            "resultado",
            "Resultado final",
            $"Solucion inicial obtenida con costo total {costoTotal:0.##}.",
            esFinal: true,
            includePenalties: metodo == MetodoTransporte.Vogel);

        return new Problema
        {
            Metodo = metodo,
            Matriz = CrearMatrizCeldas(contexto),
            Ofertas = contexto.OfertasOriginales,
            Demandas = contexto.DemandasOriginales,
            CostoTotal = costoTotal,
            EstaBalanceado = contexto.EstaBalanceado,
            BalanceAgregado = contexto.BalanceAgregado,
            Pasos = request.IncluirPasos ? contexto.Pasos : []
        };
    }

    private static ContextoResolucion CrearContextoBalanceado(ResolverProblemaRequest request, MetodoTransporte metodo)
    {
        var filasOriginales = request.Costos.Length;
        var columnasOriginales = request.Costos[0].Length;
        var totalOferta = request.Ofertas.Sum();
        var totalDemanda = request.Demandas.Sum();

        var filas = filasOriginales;
        var columnas = columnasOriginales;
        string? balanceAgregado = null;

        if (totalOferta > totalDemanda + Tolerance)
        {
            columnas++;
            balanceAgregado = $"Total de oferta: {totalOferta}\nTotal de demanda: {totalDemanda}.\nSe agrega demanda ficticia de {totalOferta - totalDemanda:0.##}";
        }
        else if (totalDemanda > totalOferta + Tolerance)
        {
            filas++;
            balanceAgregado = $"Total de oferta: {totalOferta}\nTotal de demanda: {totalDemanda}.\nSe agrega oferta ficticia de {totalDemanda - totalOferta:0.##}";
        }

        var costos = new decimal[filas, columnas];
        var esFicticia = new bool[filas, columnas];
        for (var i = 0; i < filasOriginales; i++)
        {
            for (var j = 0; j < columnasOriginales; j++)
            {
                costos[i, j] = request.Costos[i][j];
            }
        }

        var ofertas = request.Ofertas.ToList();
        var demandas = request.Demandas.ToList();

        if (totalOferta > totalDemanda + Tolerance)
        {
            var demandaFicticia = totalOferta - totalDemanda;
            demandas.Add(demandaFicticia);
            for (var i = 0; i < filas; i++)
            {
                esFicticia[i, columnas - 1] = true;
            }
        }
        else if (totalDemanda > totalOferta + Tolerance)
        {
            var ofertaFicticia = totalDemanda - totalOferta;
            ofertas.Add(ofertaFicticia);
            for (var j = 0; j < columnas; j++)
            {
                esFicticia[filas - 1, j] = true;
            }
        }

        var balanceMessage = balanceAgregado is null
            ? "La suma de ofertas y demandas ya coincide; se resuelve sin agregar origenes o destinos ficticios."
            : $"{balanceAgregado} agregada para balancear el problema. Sus costos son 0 y sus celdas se marcan como ficticias.";

        return new ContextoResolucion(
            metodo,
            costos,
            new double[filas, columnas],
            ofertas.ToArray(),
            demandas.ToArray(),
            (double[])request.Ofertas.Clone(),
            (double[])request.Demandas.Clone(),
            esFicticia,
            balanceAgregado is null,
            balanceAgregado,
            balanceMessage);
    }

    private static void ResolverPorEsquinaNoroeste(ContextoResolucion contexto)
    {
        var fila = 0;
        var columna = 0;

        while (fila < contexto.Filas && columna < contexto.Columnas)
        {
            if (contexto.OfertasRestantes[fila] <= Tolerance)
            {
                fila++;
                continue;
            }

            if (contexto.DemandasRestantes[columna] <= Tolerance)
            {
                columna++;
                continue;
            }

            var cantidad = Math.Min(contexto.OfertasRestantes[fila], contexto.DemandasRestantes[columna]);
            Asignar(contexto, fila, columna, cantidad);

            AgregarPasoAsignacion(
                contexto,
                "asignacion",
                "Asignacion por esquina noroeste",
                fila,
                columna,
                cantidad,
                $"Se toma la celda disponible mas al noroeste ({fila + 1}, {columna + 1}) y se asignan {cantidad:0.##} unidades.");

            if (contexto.OfertasRestantes[fila] <= Tolerance)
            {
                fila++;
            }

            if (contexto.DemandasRestantes[columna] <= Tolerance)
            {
                columna++;
            }
        }
    }

    private static void ResolverPorCostoMinimo(ContextoResolucion contexto)
    {
        while (HayPendientes(contexto))
        {
            var seleccion = SeleccionarCeldaCostoMinimo(contexto);
            if (seleccion is null)
            {
                break;
            }

            var (fila, columna) = seleccion.Value;
            var cantidad = Math.Min(contexto.OfertasRestantes[fila], contexto.DemandasRestantes[columna]);
            Asignar(contexto, fila, columna, cantidad);

            AgregarPasoAsignacion(
                contexto,
                "asignacion",
                "Asignacion por costo minimo",
                fila,
                columna,
                cantidad,
                $"Entre las celdas disponibles se elige el menor costo ({contexto.Costos[fila, columna]:0.##}) en ({fila + 1}, {columna + 1}) y se asignan {cantidad:0.##} unidades.");
        }
    }

    private static void ResolverPorVogel(ContextoResolucion contexto)
    {
        while (HayPendientes(contexto))
        {
            contexto.PenalizacionesFilas = CalcularPenalizacionesFilas(contexto);
            contexto.PenalizacionesColumnas = CalcularPenalizacionesColumnas(contexto);

            AgregarPaso(
                contexto,
                "penalizaciones",
                "Penalizaciones de Vogel",
                "Se calculan las penalizaciones con la diferencia entre los dos costos menores disponibles de cada fila y columna.",
                includePenalties: true);

            var seleccion = SeleccionarCeldaVogel(contexto);
            if (seleccion is null)
            {
                break;
            }

            var (fila, columna, desdeFila) = seleccion.Value;
            var cantidad = Math.Min(contexto.OfertasRestantes[fila], contexto.DemandasRestantes[columna]);
            Asignar(contexto, fila, columna, cantidad);

            var origen = desdeFila ? $"la fila {fila + 1}" : $"la columna {columna + 1}";
            AgregarPasoAsignacion(
                contexto,
                "asignacion",
                "Asignacion por Vogel",
                fila,
                columna,
                cantidad,
                $"La mayor penalizacion apunta a {origen}. Dentro de esa linea se elige la celda de menor costo ({contexto.Costos[fila, columna]:0.##}) y se asignan {cantidad:0.##} unidades.",
                includePenalties: true);
        }
    }

    private static (int Fila, int Columna)? SeleccionarCeldaCostoMinimo(ContextoResolucion
        
        contexto)
    {
        (int Fila, int Columna)? seleccion = null;
        var menorCosto = decimal.MaxValue;
        var mayorAsignable = double.MinValue;

        for (var i = 0; i < contexto.Filas; i++)
        {
            if (contexto.OfertasRestantes[i] <= Tolerance)
            {
                continue;
            }

            for (var j = 0; j < contexto.Columnas; j++)
            {
                if (contexto.DemandasRestantes[j] <= Tolerance)
                {
                    continue;
                }

                var asignable = Math.Min(contexto.OfertasRestantes[i], contexto.DemandasRestantes[j]);
                if (contexto.Costos[i, j] < menorCosto ||
                    (contexto.Costos[i, j] == menorCosto && asignable > mayorAsignable))
                {
                    menorCosto = contexto.Costos[i, j];
                    mayorAsignable = asignable;
                    seleccion = (i, j);
                }
            }
        }

        return seleccion;
    }

    private static (int Fila, int Columna, bool DesdeFila)? SeleccionarCeldaVogel(ContextoResolucion contexto)
    {
        var mejorFila = SeleccionarLinea(contexto.PenalizacionesFilas!, true, contexto);
        var mejorColumna = SeleccionarLinea(contexto.PenalizacionesColumnas!, false, contexto);

        if (mejorFila is null && mejorColumna is null)
        {
            return null;
        }

        if (mejorColumna is null ||
            (mejorFila is not null && CompararLineas(mejorFila, mejorColumna) <= 0))
        {
            var fila = mejorFila!.Indice;
            var columna = SeleccionarMenorCostoEnFila(contexto, fila);
            return columna is null ? null : (fila, columna.Value, true);
        }

        var col = mejorColumna.Indice;
        var renglon = SeleccionarMenorCostoEnColumna(contexto, col);
        return renglon is null ? null : (renglon.Value, col, false);
    }

    private static LineaCandidata? SeleccionarLinea(IReadOnlyList<double> penalizaciones, bool esFila, ContextoResolucion contexto)
    {
        LineaCandidata? mejor = null;

        for (var indice = 0; indice < penalizaciones.Count; indice++)
        {
            if (esFila && contexto.OfertasRestantes[indice] <= Tolerance)
            {
                continue;
            }

            if (!esFila && contexto.DemandasRestantes[indice] <= Tolerance)
            {
                continue;
            }

            var menorCosto = esFila
                ? MenorCostoFila(contexto, indice)
                : MenorCostoColumna(contexto, indice);

            if (menorCosto is null)
            {
                continue;
            }

            var candidata = new LineaCandidata(indice, penalizaciones[indice], menorCosto.Value);
            if (mejor is null || CompararLineas(candidata, mejor) < 0)
            {
                mejor = candidata;
            }
        }

        return mejor;
    }

    private static int CompararLineas(LineaCandidata left, LineaCandidata right)
    {
        var penaltyCompare = right.Penalizacion.CompareTo(left.Penalizacion);
        if (penaltyCompare != 0)
        {
            return penaltyCompare;
        }

        var costCompare = left.MenorCosto.CompareTo(right.MenorCosto);
        return costCompare != 0 ? costCompare : left.Indice.CompareTo(right.Indice);
    }

    private static int? SeleccionarMenorCostoEnFila(ContextoResolucion contexto, int fila)
    {
        int? columna = null;
        var menorCosto = decimal.MaxValue;
        var mayorAsignable = double.MinValue;

        for (var j = 0; j < contexto.Columnas; j++)
        {
            if (contexto.DemandasRestantes[j] <= Tolerance)
            {
                continue;
            }

            var asignable = Math.Min(contexto.OfertasRestantes[fila], contexto.DemandasRestantes[j]);
            if (contexto.Costos[fila, j] < menorCosto ||
                (contexto.Costos[fila, j] == menorCosto && asignable > mayorAsignable))
            {
                menorCosto = contexto.Costos[fila, j];
                mayorAsignable = asignable;
                columna = j;
            }
        }

        return columna;
    }

    private static int? SeleccionarMenorCostoEnColumna(ContextoResolucion contexto, int columna)
    {
        int? fila = null;
        var menorCosto = decimal.MaxValue;
        var mayorAsignable = double.MinValue;

        for (var i = 0; i < contexto.Filas; i++)
        {
            if (contexto.OfertasRestantes[i] <= Tolerance)
            {
                continue;
            }

            var asignable = Math.Min(contexto.OfertasRestantes[i], contexto.DemandasRestantes[columna]);
            if (contexto.Costos[i, columna] < menorCosto ||
                (contexto.Costos[i, columna] == menorCosto && asignable > mayorAsignable))
            {
                menorCosto = contexto.Costos[i, columna];
                mayorAsignable = asignable;
                fila = i;
            }
        }

        return fila;
    }

    private static decimal? MenorCostoFila(ContextoResolucion contexto, int fila)
    {
        decimal? menor = null;
        for (var j = 0; j < contexto.Columnas; j++)
        {
            if (contexto.DemandasRestantes[j] <= Tolerance)
            {
                continue;
            }

            menor = menor is null ? contexto.Costos[fila, j] : Math.Min(menor.Value, contexto.Costos[fila, j]);
        }

        return menor;
    }

    private static decimal? MenorCostoColumna(ContextoResolucion contexto, int columna)
    {
        decimal? menor = null;
        for (var i = 0; i < contexto.Filas; i++)
        {
            if (contexto.OfertasRestantes[i] <= Tolerance)
            {
                continue;
            }

            menor = menor is null ? contexto.Costos[i, columna] : Math.Min(menor.Value, contexto.Costos[i, columna]);
        }

        return menor;
    }

    private static double[] CalcularPenalizacionesFilas(ContextoResolucion contexto)
    {
        var penalizaciones = new double[contexto.Filas];
        for (var i = 0; i < contexto.Filas; i++)
        {
            if (contexto.OfertasRestantes[i] <= Tolerance)
            {
                continue;
            }

            var costos = new List<decimal>();
            for (var j = 0; j < contexto.Columnas; j++)
            {
                if (contexto.DemandasRestantes[j] > Tolerance)
                {
                    costos.Add(contexto.Costos[i, j]);
                }
            }

            penalizaciones[i] = CalcularPenalizacion(costos);
        }

        return penalizaciones;
    }

    private static double[] CalcularPenalizacionesColumnas(ContextoResolucion contexto)
    {
        var penalizaciones = new double[contexto.Columnas];
        for (var j = 0; j < contexto.Columnas; j++)
        {
            if (contexto.DemandasRestantes[j] <= Tolerance)
            {
                continue;
            }

            var costos = new List<decimal>();
            for (var i = 0; i < contexto.Filas; i++)
            {
                if (contexto.OfertasRestantes[i] > Tolerance)
                {
                    costos.Add(contexto.Costos[i, j]);
                }
            }

            penalizaciones[j] = CalcularPenalizacion(costos);
        }

        return penalizaciones;
    }

    private static double CalcularPenalizacion(List<decimal> costos)
    {
        if (costos.Count < 2)
        {
            return 0;
        }

        costos.Sort();
        return (double)(costos[1] - costos[0]);
    }

    private static void Asignar(ContextoResolucion contexto, int fila, int columna, double cantidad)
    {
        contexto.Asignaciones[fila, columna] += cantidad;
        contexto.OfertasRestantes[fila] -= cantidad;
        contexto.DemandasRestantes[columna] -= cantidad;

        if (Math.Abs(contexto.OfertasRestantes[fila]) <= Tolerance)
        {
            contexto.OfertasRestantes[fila] = 0;
        }

        if (Math.Abs(contexto.DemandasRestantes[columna]) <= Tolerance)
        {
            contexto.DemandasRestantes[columna] = 0;
        }
    }

    private static void AgregarPasoAsignacion(
        ContextoResolucion contexto,
        string tipo,
        string titulo,
        int fila,
        int columna,
        double cantidad,
        string descripcion,
        bool includePenalties = false)
    {
        var ofertaAgotada = contexto.OfertasRestantes[fila] <= Tolerance;
        var demandaAgotada = contexto.DemandasRestantes[columna] <= Tolerance;
        var agotamientos = new List<string>();

        if (ofertaAgotada)
        {
            agotamientos.Add($"se agota la oferta de la fila {fila + 1}");
        }

        if (demandaAgotada)
        {
            agotamientos.Add($"se satisface la demanda de la columna {columna + 1}");
        }

        if (agotamientos.Count > 0)
        {
            descripcion += " Despues de asignar, " + string.Join(" y ", agotamientos) + ".";
        }

        AgregarPaso(contexto, tipo, titulo, descripcion, fila, columna, ofertaAgotada, demandaAgotada, includePenalties);
    }

    private static void AgregarPaso(
        ContextoResolucion contexto,
        string tipo,
        string titulo,
        string descripcion,
        int? filaSeleccionada = null,
        int? columnaSeleccionada = null,
        bool ofertaAgotada = false,
        bool demandaAgotada = false,
        bool includePenalties = false,
        bool esFinal = false)
    {
        contexto.Pasos.Add(new PasoResolucion
        {
            Numero = contexto.Pasos.Count + 1,
            Tipo = tipo,
            Titulo = titulo,
            Descripcion = descripcion,
            Matriz = CrearMatrizCeldas(contexto),
            OfertasRestantes = (double[])contexto.OfertasRestantes.Clone(),
            DemandasRestantes = (double[])contexto.DemandasRestantes.Clone(),
            PenalizacionesFilas = includePenalties && contexto.PenalizacionesFilas is not null
                ? (double[])contexto.PenalizacionesFilas.Clone()
                : null,
            PenalizacionesColumnas = includePenalties && contexto.PenalizacionesColumnas is not null
                ? (double[])contexto.PenalizacionesColumnas.Clone()
                : null,
            FilaSeleccionada = filaSeleccionada,
            ColumnaSeleccionada = columnaSeleccionada,
            OfertaAgotada = ofertaAgotada,
            DemandaAgotada = demandaAgotada,
            EsFinal = esFinal
        });
    }

    private static IReadOnlyList<IReadOnlyList<Celda>> CrearMatrizCeldas(ContextoResolucion contexto)
    {
        var matriz = new List<IReadOnlyList<Celda>>(contexto.Filas);
        for (var i = 0; i < contexto.Filas; i++)
        {
            var fila = new List<Celda>(contexto.Columnas);
            for (var j = 0; j < contexto.Columnas; j++)
            {
                fila.Add(new Celda(i, j, contexto.Costos[i, j], contexto.Asignaciones[i, j], contexto.EsFicticia[i, j]));
            }

            matriz.Add(fila);
        }

        return matriz;
    }

    private static bool HayPendientes(ContextoResolucion contexto)
    {
        return contexto.OfertasRestantes.Any(value => value > Tolerance) &&
            contexto.DemandasRestantes.Any(value => value > Tolerance);
    }

    private static decimal CalcularCostoTotal(ContextoResolucion contexto)
    {
        decimal total = 0;
        for (var i = 0; i < contexto.Filas; i++)
        {
            for (var j = 0; j < contexto.Columnas; j++)
            {
                total += contexto.Costos[i, j] * (decimal)contexto.Asignaciones[i, j];
            }
        }

        return total;
    }

    private sealed class LineaCandidata
    {
        public LineaCandidata(int indice, double penalizacion, decimal menorCosto)
        {
            Indice = indice;
            Penalizacion = penalizacion;
            MenorCosto = menorCosto;
        }

        public int Indice { get; }
        public double Penalizacion { get; }
        public decimal MenorCosto { get; }
    }

    private sealed class ContextoResolucion
    {
        public ContextoResolucion(
            MetodoTransporte metodo,
            decimal[,] costos,
            double[,] asignaciones,
            double[] ofertasRestantes,
            double[] demandasRestantes,
            double[] ofertasOriginales,
            double[] demandasOriginales,
            bool[,] esFicticia,
            bool estaBalanceado,
            string? balanceAgregado,
            string balanceMessage)
        {
            Metodo = metodo;
            Costos = costos;
            Asignaciones = asignaciones;
            OfertasRestantes = ofertasRestantes;
            DemandasRestantes = demandasRestantes;
            OfertasOriginales = ofertasOriginales;
            DemandasOriginales = demandasOriginales;
            EsFicticia = esFicticia;
            EstaBalanceado = estaBalanceado;
            BalanceAgregado = balanceAgregado;
            BalanceMessage = balanceMessage;
        }

        public MetodoTransporte Metodo { get; }
        public decimal[,] Costos { get; }
        public double[,] Asignaciones { get; }
        public double[] OfertasRestantes { get; }
        public double[] DemandasRestantes { get; }
        public double[] OfertasOriginales { get; }
        public double[] DemandasOriginales { get; }
        public bool[,] EsFicticia { get; }
        public bool EstaBalanceado { get; }
        public string? BalanceAgregado { get; }
        public string BalanceMessage { get; }
        public double[]? PenalizacionesFilas { get; set; }
        public double[]? PenalizacionesColumnas { get; set; }
        public List<PasoResolucion> Pasos { get; } = [];
        public int Filas => Costos.GetLength(0);
        public int Columnas => Costos.GetLength(1);
    }
}


