namespace TP_Final_Lucio.Models;

public class Prestamo
{
    public int Id { get; set; }
    public Libro Libro { get; set; } = new Libro();
    public DateTime FechaRetiro { get; set; }
    public DateTime FechaDevolucion { get; set; }
    public string Codigo { get; set; } = "";
    public bool Cancelado { get; set; }
}
