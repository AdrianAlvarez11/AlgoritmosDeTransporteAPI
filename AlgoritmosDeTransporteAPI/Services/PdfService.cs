using AlgoritmosDeTransporteAPI.Domain;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace AlgoritmosDeTransporteAPI.Services;

public sealed class PdfService : IPdfService
{
    private static readonly PdfFont BoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

    public byte[] GenerarReporte(Problema problema)
    {
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

        document.Add(new Paragraph("Matriz final")
            .SetFontSize(14)
            .SetFont(BoldFont)
            .SetMarginTop(14));
        document.Add(CrearTablaMatriz(problema.Matriz));

        if (problema.Pasos.Count > 0)
        {
            document.Add(new Paragraph("Pasos")
                .SetFontSize(14)
                .SetFont(BoldFont)
                .SetMarginTop(14));

            foreach (var paso in problema.Pasos)
            {
                document.Add(new Paragraph($"{paso.Numero}. {paso.Titulo}")
                    .SetFont(BoldFont)
                    .SetMarginTop(8)
                    .SetMarginBottom(2));
                document.Add(new Paragraph(paso.Descripcion).SetMarginTop(0));

                if (paso.PenalizacionesFilas is not null || paso.PenalizacionesColumnas is not null)
                {
                    document.Add(new Paragraph(CrearResumenPenalizaciones(paso))
                        .SetFontSize(9)
                        .SetFontColor(ColorConstants.DARK_GRAY));
                }
            }
        }

        document.Close();
        return stream.ToArray();
    }

    private static Table CrearTablaMatriz(IReadOnlyList<IReadOnlyList<Celda>> matriz)
    {
        var columnas = matriz.Count == 0 ? 1 : matriz[0].Count;
        var table = new Table(UnitValue.CreatePercentArray(columnas)).UseAllAvailableWidth();

        foreach (var fila in matriz)
        {
            foreach (var celda in fila)
            {
                var contenido = $"Costo: {celda.Costo:0.##}\nAsig.: {celda.Asignacion:0.##}";
                if (celda.EsFicticia)
                {
                    contenido += "\nFicticia";
                }

                table.AddCell(new Cell()
                    .Add(new Paragraph(contenido).SetFontSize(9))
                    .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.75f))
                    .SetBackgroundColor(celda.Asignacion > 0 ? new DeviceRgb(229, 244, 234) : ColorConstants.WHITE));
            }
        }

        return table;
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


