using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using AlgoritmosDeTransporteAPI.Models;

namespace AlgoritmosDeTransporteAPI.Services;

public sealed class PdfService : IPdfService
{
    private PdfFont BoldFont = null!;
    private PdfFont RegularFont = null!;

    private static readonly DeviceRgb ColorEncabezado = new DeviceRgb(130, 226, 179);
    private static readonly DeviceRgb ColorCeldaAsignada = new DeviceRgb(214, 245, 227);
    private static readonly DeviceRgb ColorCeldaVacia = new DeviceRgb(240, 253, 246);
    private static readonly DeviceRgb ColorCeldaFicticia = new DeviceRgb(253, 236, 220);
    private static readonly DeviceRgb ColorCeldaMovimiento = new DeviceRgb(255, 244, 179);
    private static readonly DeviceRgb ColorTextoGris = new DeviceRgb(110, 110, 110);

    public byte[] GenerarReporte(Problema problema)
    {
        BoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        RegularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        document.Add(new Paragraph("Reporte de algoritmo de transporte")
            .SetFontSize(18)
            .SetFont(BoldFont)
            .SetMarginBottom(12));

        document.Add(new Paragraph($"Metodo: {NombreMetodo(problema.Metodo)}"));
        document.Add(new Paragraph($"Costo total: {problema.CostoTotal:0.##}"));
        document.Add(new Paragraph(problema.EstaBalanceado
            ? "Balance: el problema ya estaba balanceado."
            : $"Balance: {problema.BalanceAgregado}."));

        if (problema.Pasos.Count > 0)
        {
            document.Add(new Paragraph("Procedimiento paso a paso")
                .SetFontSize(14)
                .SetFont(BoldFont)
                .SetMarginTop(16)
                .SetMarginBottom(4));

            foreach (var paso in problema.Pasos)
            {
                document.Add(new Paragraph($"Paso {paso.Numero}: {paso.Titulo}")
                    .SetFont(BoldFont)
                    .SetFontSize(12)
                    .SetMarginTop(14)
                    .SetMarginBottom(2));

                document.Add(new Paragraph(paso.Descripcion)
                    .SetFontSize(10)
                    .SetMarginTop(0)
                    .SetMarginBottom(6));

                document.Add(CrearTablaPaso(paso));

                if (paso.PenalizacionesFilas is not null || paso.PenalizacionesColumnas is not null)
                {
                    document.Add(new Paragraph(CrearResumenPenalizaciones(paso))
                        .SetFontSize(9)
                        .SetFontColor(ColorTextoGris)
                        .SetMarginTop(4));
                }
            }
        }

        document.Add(new Paragraph("Matriz final")
            .SetFontSize(14)
            .SetFont(BoldFont)
            .SetMarginTop(18));
        document.Add(CrearTablaFinal(problema.Matriz, problema.Ofertas, problema.Demandas));

        document.Close();
        return stream.ToArray();
    }

    // ---------- Tablas con encabezados (Origen/Destino, Oferta, Demanda) ----------

    private Table CrearTablaConEncabezados(
        IReadOnlyList<IReadOnlyList<Celda>> matriz,
        IReadOnlyList<double> ofertas,
        IReadOnlyList<double> demandas,
        int? filaSeleccionada = null,
        int? columnaSeleccionada = null)
    {
        var filas = matriz.Count;
        var columnas = filas == 0 ? 0 : matriz[0].Count;
        var totalColumnas = columnas + 2;

        var table = new Table(UnitValue.CreatePercentArray(totalColumnas))
            .UseAllAvailableWidth()
            .SetMarginBottom(4);

        // Fila de encabezado (Origen/Destino, 1, 2, 3..., Oferta)
        table.AddHeaderCell(CrearCeldaEncabezado("Origen /\nDestino"));
        for (var j = 0; j < columnas; j++)
        {
            table.AddHeaderCell(CrearCeldaEncabezado((j + 1).ToString()));
        }
        table.AddHeaderCell(CrearCeldaEncabezado("Oferta"));

        // Filas de datos (número de origen, costos/asignaciones, oferta)
        for (var i = 0; i < filas; i++)
        {
            table.AddCell(CrearCeldaEncabezado((i + 1).ToString()));
            for (var j = 0; j < columnas; j++)
            {
                var celda = matriz[i][j];
                var esMovimiento = filaSeleccionada == i && columnaSeleccionada == j;
                table.AddCell(CrearCeldaContenido(celda, esMovimiento));
            }
            var valorOferta = i < ofertas.Count ? ofertas[i] : 0;
            table.AddCell(CrearCeldaValor(valorOferta));
        }

        // Fila de demanda
        table.AddCell(CrearCeldaEncabezado("Demanda"));
        for (var j = 0; j < columnas; j++)
        {
            var valorDemanda = j < demandas.Count ? demandas[j] : 0;
            table.AddCell(CrearCeldaValor(valorDemanda));
        }
        table.AddCell(new Cell().SetBorder(Border.NO_BORDER));

        return table;
    }

    private Table CrearTablaFinal(
        IReadOnlyList<IReadOnlyList<Celda>> matriz,
        IReadOnlyList<double> ofertas,
        IReadOnlyList<double> demandas)
        => CrearTablaConEncabezados(matriz, ofertas, demandas);

    private Table CrearTablaPaso(PasoResolucion paso)
        => CrearTablaConEncabezados(
            paso.Matriz,
            paso.OfertasRestantes,
            paso.DemandasRestantes,
            paso.FilaSeleccionada,
            paso.ColumnaSeleccionada);

    // ---------- Estilos de celda ----------

    private Cell CrearCeldaEncabezado(string texto)
    {
        return new Cell()
            .Add(new Paragraph(texto)
                .SetFont(BoldFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER))
            .SetBackgroundColor(ColorEncabezado)
            .SetBorder(new SolidBorder(ColorConstants.WHITE, 2f))
            .SetPadding(6)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
    }

    private Cell CrearCeldaValor(double valor)
    {
        return new Cell()
            .Add(new Paragraph(valor.ToString("0.##"))
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER))
            .SetBackgroundColor(ColorCeldaVacia)
            .SetBorder(new SolidBorder(ColorConstants.WHITE, 2f))
            .SetPadding(6)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
    }

    private Cell CrearCeldaContenido(Celda celda, bool esMovimiento)
    {
        var fondo = celda.EsFicticia
            ? ColorCeldaFicticia
            : esMovimiento
                ? ColorCeldaMovimiento
                : celda.Asignacion > 0
                    ? ColorCeldaAsignada
                    : ColorCeldaVacia;

        var contenedor = new Div().SetTextAlignment(TextAlignment.CENTER);

        contenedor.Add(new Paragraph($"Costo: {celda.Costo:0.##}")
            .SetFont(RegularFont)
            .SetFontSize(8)
            .SetFontColor(ColorTextoGris)
            .SetMarginBottom(2)
            .SetTextAlignment(TextAlignment.CENTER));

        contenedor.Add(new Paragraph(celda.Asignacion > 0 ? celda.Asignacion.ToString("0.##") : "—")
            .SetFont(celda.Asignacion > 0 ? BoldFont : RegularFont)
            .SetFontSize(11)
            .SetMarginTop(0)
            .SetTextAlignment(TextAlignment.CENTER));

        if (celda.EsFicticia)
        {
            contenedor.Add(new Paragraph("Ficticia")
                .SetFont(RegularFont)
                .SetFontSize(7)
                .SetFontColor(ColorTextoGris)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(2));
        }

        return new Cell()
            .Add(contenedor)
            .SetBackgroundColor(fondo)
            .SetBorder(new SolidBorder(ColorConstants.WHITE, 2f))
            .SetPadding(6)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
    }

    private static string CrearResumenPenalizaciones(PasoResolucion paso)
    {
        var partes = new List<string>();
        if (paso.PenalizacionesFilas is not null)
        {
            partes.Add("Filas: " + string.Join(", ", paso.PenalizacionesFilas.Select(p => p.ToString("0.##"))));
        }

        if (paso.PenalizacionesColumnas is not null)
        {
            partes.Add("Columnas: " + string.Join(", ", paso.PenalizacionesColumnas.Select(p => p.ToString("0.##"))));
        }

        return "Penalizaciones - " + string.Join(" | ", partes);
    }

    private static string NombreMetodo(MetodoTransporte metodo)
    {
        return metodo switch
        {
            MetodoTransporte.EsquinaNoroeste => "Esquina noroeste",
            MetodoTransporte.CostoMinimo => "Costo minimo",
            MetodoTransporte.Vogel => "Vogel",
            _ => metodo.ToString()
        };
    }
}