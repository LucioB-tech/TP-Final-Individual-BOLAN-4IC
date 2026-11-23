using Microsoft.Data.SqlClient;

namespace TP_Final_Lucio.Models;

public class BD
{
    private readonly string _conexion;

    public BD(IConfiguration configuracion)
    {
        _conexion = configuracion.GetConnectionString("ConexionSQL")
            ?? throw new InvalidOperationException("No se encontró la conexión ConexionSQL.");
    }

    private SqlConnection CrearConexion()
    {
        return new SqlConnection(_conexion);
    }

    public Usuario? ValidarUsuario(string nombreUsuario, string contrasena)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            "SELECT Id, NombreUsuario, Nombre, Apellido FROM Usuarios WHERE NombreUsuario = @usuario AND Contrasena = @contrasena", conexion);
        comando.Parameters.AddWithValue("@usuario", nombreUsuario);
        comando.Parameters.AddWithValue("@contrasena", contrasena);
        conexion.Open();
        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;

        return new Usuario
        {
            Id = Convert.ToInt32(lector["Id"]),
            NombreUsuario = lector["NombreUsuario"].ToString() ?? "",
            Nombre = lector["Nombre"].ToString() ?? "",
            Apellido = lector["Apellido"].ToString() ?? ""
        };
    }

    public bool ExisteNombreUsuario(string nombreUsuario)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @usuario", conexion);
        comando.Parameters.AddWithValue("@usuario", nombreUsuario);
        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    public void CrearUsuario(string nombreUsuario, string contrasena, string nombre, string apellido)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            "INSERT INTO Usuarios (NombreUsuario, Contrasena, Nombre, Apellido) VALUES (@usuario, @contrasena, @nombre, @apellido)", conexion);
        comando.Parameters.AddWithValue("@usuario", nombreUsuario);
        comando.Parameters.AddWithValue("@contrasena", contrasena);
        comando.Parameters.AddWithValue("@nombre", nombre);
        comando.Parameters.AddWithValue("@apellido", apellido);
        conexion.Open();
        comando.ExecuteNonQuery();
    }

    public List<Libro> ObtenerLibros(int desde, int cantidad)
    {
        var libros = new List<Libro>();
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            SELECT l.id, l.Titulo, l.Autor, l.AnioEdicion, l.Descripcion, l.Imagen,
                   l.IdCategoria, c.Nombre AS Categoria,
                   CASE WHEN EXISTS (
                       SELECT 1 FROM Prestamos p
                       WHERE p.IdLibro = l.id AND p.Cancelado = 0
                         AND p.FechaDevolucion >= CAST(GETDATE() AS date)
                   ) THEN 0 ELSE 1 END AS Disponible
            FROM Libros l
            INNER JOIN Categorias c ON c.id = l.IdCategoria
            WHERE l.Activo = 1
            ORDER BY l.id
            OFFSET @desde ROWS FETCH NEXT @cantidad ROWS ONLY", conexion);
        comando.Parameters.AddWithValue("@desde", desde);
        comando.Parameters.AddWithValue("@cantidad", cantidad);
        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            libros.Add(new Libro
            {
                Id = Convert.ToInt32(lector["id"]),
                Titulo = lector["Titulo"].ToString() ?? "",
                Autor = lector["Autor"].ToString() ?? "",
                AnioEdicion = Convert.ToInt32(lector["AnioEdicion"]),
                Descripcion = lector["Descripcion"].ToString() ?? "",
                Imagen = lector["Imagen"].ToString() ?? "",
                IdCategoria = Convert.ToInt32(lector["IdCategoria"]),
                Categoria = lector["Categoria"].ToString() ?? "",
                Disponible = Convert.ToBoolean(lector["Disponible"])
            });
        }
        return libros;
    }

    public Libro? ObtenerLibro(int id)
    {
        return ObtenerLibrosPorId(id);
    }

    private Libro? ObtenerLibrosPorId(int id)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            SELECT l.id, l.Titulo, l.Autor, l.AnioEdicion, l.Descripcion, l.Imagen,
                   l.IdCategoria, c.Nombre AS Categoria,
                   CASE WHEN EXISTS (
                       SELECT 1 FROM Prestamos p
                       WHERE p.IdLibro = l.id AND p.Cancelado = 0
                         AND p.FechaDevolucion >= CAST(GETDATE() AS date)
                   ) THEN 0 ELSE 1 END AS Disponible
            FROM Libros l INNER JOIN Categorias c ON c.id = l.IdCategoria
            WHERE l.id = @id AND l.Activo = 1", conexion);
        comando.Parameters.AddWithValue("@id", id);
        conexion.Open();
        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;
        return new Libro
        {
            Id = Convert.ToInt32(lector["id"]),
            Titulo = lector["Titulo"].ToString() ?? "",
            Autor = lector["Autor"].ToString() ?? "",
            AnioEdicion = Convert.ToInt32(lector["AnioEdicion"]),
            Descripcion = lector["Descripcion"].ToString() ?? "",
            Imagen = lector["Imagen"].ToString() ?? "",
            IdCategoria = Convert.ToInt32(lector["IdCategoria"]),
            Categoria = lector["Categoria"].ToString() ?? "",
            Disponible = Convert.ToBoolean(lector["Disponible"])
        };
    }

    public int ContarPrestamosActivos(int idUsuario)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            SELECT COUNT(*) FROM Prestamos
            WHERE IdUsuario = @usuario AND Cancelado = 0
              AND FechaDevolucion >= CAST(GETDATE() AS date)", conexion);
        comando.Parameters.AddWithValue("@usuario", idUsuario);
        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public bool LibroTienePrestamoActivo(int idLibro)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            SELECT COUNT(*) FROM Prestamos
            WHERE IdLibro = @libro AND Cancelado = 0
              AND FechaDevolucion >= CAST(GETDATE() AS date)", conexion);
        comando.Parameters.AddWithValue("@libro", idLibro);
        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    public string CrearPrestamo(int idLibro, int idUsuario)
    {
        string codigo = new Random().Next(100000, 1000000).ToString();
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            INSERT INTO Prestamos (IdLibro, IdUsuario, FechaRetiro, FechaDevolucion, Codigo, Cancelado, fechaCancelacion)
            VALUES (@libro, @usuario, CAST(GETDATE() AS date), DATEADD(day, 7, CAST(GETDATE() AS date)), @codigo, 0, NULL)", conexion);
        comando.Parameters.AddWithValue("@libro", idLibro);
        comando.Parameters.AddWithValue("@usuario", idUsuario);
        comando.Parameters.AddWithValue("@codigo", codigo);
        conexion.Open();
        comando.ExecuteNonQuery();
        return codigo;
    }

    public List<Prestamo> ObtenerPrestamos(int idUsuario, bool pasados)
    {
        var prestamos = new List<Prestamo>();
        using var conexion = CrearConexion();
        string condicion = pasados
            ? "(p.Cancelado = 1 OR p.FechaDevolucion < CAST(GETDATE() AS date))"
            : "(p.Cancelado = 0 AND p.FechaDevolucion >= CAST(GETDATE() AS date))";
        using var comando = new SqlCommand($@"
            SELECT p.id, p.IdLibro, l.Titulo, l.Autor, p.FechaRetiro,
                   p.FechaDevolucion, p.Codigo, p.Cancelado
            FROM Prestamos p INNER JOIN Libros l ON l.id = p.IdLibro
            WHERE p.IdUsuario = @usuario AND {condicion}
            ORDER BY p.FechaRetiro DESC", conexion);
        comando.Parameters.AddWithValue("@usuario", idUsuario);
        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            prestamos.Add(new Prestamo
            {
                Id = Convert.ToInt32(lector["id"]),
                IdLibro = Convert.ToInt32(lector["IdLibro"]),
                Titulo = lector["Titulo"].ToString() ?? "",
                Autor = lector["Autor"].ToString() ?? "",
                FechaRetiro = Convert.ToDateTime(lector["FechaRetiro"]),
                FechaDevolucion = Convert.ToDateTime(lector["FechaDevolucion"]),
                Codigo = lector["Codigo"].ToString() ?? "",
                Cancelado = Convert.ToBoolean(lector["Cancelado"])
            });
        }
        return prestamos;
    }

    public bool CancelarPrestamo(int idPrestamo, int idUsuario)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(@"
            UPDATE Prestamos SET Cancelado = 1, fechaCancelacion = GETDATE()
            WHERE id = @id AND IdUsuario = @usuario AND Cancelado = 0
              AND FechaDevolucion >= CAST(GETDATE() AS date)", conexion);
        comando.Parameters.AddWithValue("@id", idPrestamo);
        comando.Parameters.AddWithValue("@usuario", idUsuario);
        conexion.Open();
        return comando.ExecuteNonQuery() > 0;
    }
}
