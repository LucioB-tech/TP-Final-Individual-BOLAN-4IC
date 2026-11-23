namespace TP_Final_Lucio.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int IdLibro { get; set; }
    public string Titulo { get; set; } = "";
    public string Autor { get; set; } = "";
    public DateTime FechaRetiro { get; set; }
    public DateTime FechaDevolucion { get; set; }
    public string Codigo { get; set; } = "";
    public bool Cancelado { get; set; }
}
