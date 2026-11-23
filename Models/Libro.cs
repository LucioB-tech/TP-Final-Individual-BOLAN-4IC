namespace TP_Final_Lucio.Models;

public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Autor { get; set; } = "";
    public int AnioEdicion { get; set; }
    public string Descripcion { get; set; } = "";
    public string Imagen { get; set; } = "";
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = "";
    public bool Disponible { get; set; }
}
