namespace AlgoritmosDeTransporteAPI.Models;

public sealed class Celda
{
    public Celda(int fila, int columna, decimal costo, double asignacion, bool esFicticia = false)
    {
        Fila = fila;
        Columna = columna;
        Costo = costo;
        Asignacion = asignacion;
        EsFicticia = esFicticia;
    }

    public int Fila { get; }
    public int Columna { get; }
    public decimal Costo { get; }
    public double Asignacion { get; }
    public bool EsFicticia { get; }
}
