using Dapper;
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
        conexion.Open();
        return conexion.QueryFirstOrDefault<Usuario>(
            "SELECT Id, NombreUsuario, Nombre, Apellido FROM Usuarios WHERE NombreUsuario = @usuario AND Contrasena = @contrasena",
            new { usuario = nombreUsuario, contrasena });
    }

    public bool ExisteNombreUsuario(string nombreUsuario)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @usuario",
            new { usuario = nombreUsuario }) > 0;
    }

    public void CrearUsuario(string nombreUsuario, string contrasena, string nombre, string apellido)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        conexion.Execute(
            "INSERT INTO Usuarios (NombreUsuario, Contrasena, Nombre, Apellido) VALUES (@usuario, @contrasena, @nombre, @apellido)",
            new { usuario = nombreUsuario, contrasena, nombre, apellido });
    }

    public List<Libro> ObtenerLibros(int desde, int cantidad)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.Query<Libro, Categoria, Libro>(@"
            SELECT l.id, l.Titulo, l.Autor, l.AnioEdicion, l.Descripcion, l.Imagen,
                   CASE WHEN EXISTS (
                       SELECT 1 FROM Prestamos p
                       WHERE p.IdLibro = l.id AND p.Cancelado = 0
                         AND p.FechaDevolucion >= CAST(GETDATE() AS date)
                   ) THEN 0 ELSE 1 END AS Disponible,
                   c.id AS Id, c.Nombre
            FROM Libros l
            INNER JOIN [dbo].[Categorías] c ON c.id = l.IdCategoria
            WHERE l.Activo = 1
            ORDER BY l.id
            OFFSET @desde ROWS FETCH NEXT @cantidad ROWS ONLY",
            (libro, categoria) =>
            {
                libro.Categoria = categoria;
                return libro;
            },
            new { desde, cantidad },
            splitOn: "Id").AsList();
    }

    public Libro? ObtenerLibro(int id)
    {
        return ObtenerLibrosPorId(id);
    }

    private Libro? ObtenerLibrosPorId(int id)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.Query<Libro, Categoria, Libro>(@"
            SELECT l.id, l.Titulo, l.Autor, l.AnioEdicion, l.Descripcion, l.Imagen,
                   CASE WHEN EXISTS (
                       SELECT 1 FROM Prestamos p
                       WHERE p.IdLibro = l.id AND p.Cancelado = 0
                         AND p.FechaDevolucion >= CAST(GETDATE() AS date)
                   ) THEN 0 ELSE 1 END AS Disponible,
                   c.id AS Id, c.Nombre
            FROM Libros l INNER JOIN [dbo].[Categorías] c ON c.id = l.IdCategoria
            WHERE l.id = @id AND l.Activo = 1",
            (libro, categoria) =>
            {
                libro.Categoria = categoria;
                return libro;
            },
            new { id },
            splitOn: "Id").FirstOrDefault();
    }

    public int ContarPrestamosActivos(int idUsuario)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.ExecuteScalar<int>(@"
            SELECT COUNT(*) FROM Prestamos
            WHERE IdUsuario = @usuario AND Cancelado = 0
              AND FechaDevolucion >= CAST(GETDATE() AS date)",
            new { usuario = idUsuario });
    }

    public int ContarPrestamosEnPeriodo(int idUsuario, DateTime retiro, DateTime devolucion)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.ExecuteScalar<int>(@"
            SELECT COUNT(*) FROM Prestamos
            WHERE IdUsuario = @usuario AND Cancelado = 0
              AND FechaRetiro <= @devolucion AND FechaDevolucion >= @retiro",
            new { usuario = idUsuario, retiro, devolucion });
    }

    public DateTime? ObtenerHastaCuandoEstaPrestado(int idLibro, DateTime retiro, DateTime devolucion)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.QueryFirstOrDefault<DateTime?>(@"
            SELECT MAX(FechaDevolucion)
            FROM Prestamos
            WHERE IdLibro = @libro AND Cancelado = 0
              AND FechaRetiro <= @devolucion AND FechaDevolucion >= @retiro",
            new { libro = idLibro, retiro, devolucion });
    }

    public string CrearPrestamo(int idLibro, int idUsuario, DateTime retiro, DateTime devolucion)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var aleatorio = new Random();
        string codigo;

        do
        {
            codigo = new string(Enumerable.Range(0, 6)
                .Select(_ => caracteres[aleatorio.Next(caracteres.Length)])
                .ToArray());
        } while (conexion.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Prestamos WHERE Codigo = @codigo",
            new { codigo }) > 0);

        conexion.Execute(@"
            INSERT INTO Prestamos (IdLibro, IdUsuario, FechaRetiro, FechaDevolucion, Codigo, Cancelado, fechaCancelacion)
            VALUES (@libro, @usuario, @retiro, @devolucion, @codigo, 0, NULL)",
            new { libro = idLibro, usuario = idUsuario, retiro, devolucion, codigo });
        return codigo;
    }

    public List<Prestamo> ObtenerPrestamos(int idUsuario, bool pasados)
    {
        using var conexion = CrearConexion();
        string condicion = pasados
            ? "(p.Cancelado = 1 OR p.FechaDevolucion < CAST(GETDATE() AS date))"
            : "(p.Cancelado = 0 AND p.FechaDevolucion >= CAST(GETDATE() AS date))";
        conexion.Open();
        return conexion.Query<Prestamo, Libro, Categoria, Prestamo>($@"
            SELECT p.id, p.FechaRetiro, p.FechaDevolucion, p.Codigo, p.Cancelado,
                   l.id, l.Titulo, l.Autor, l.AnioEdicion, l.Descripcion, l.Imagen,
                   c.id, c.Nombre
            FROM Prestamos p
            INNER JOIN Libros l ON l.id = p.IdLibro
            INNER JOIN [dbo].[Categorías] c ON c.id = l.IdCategoria
            WHERE p.IdUsuario = @usuario AND {condicion}
            ORDER BY p.FechaRetiro DESC",
            (prestamo, libro, categoria) =>
            {
                libro.Categoria = categoria;
                prestamo.Libro = libro;
                return prestamo;
            },
            new { usuario = idUsuario },
            splitOn: "Id,Id").AsList();
    }

    public bool CancelarPrestamo(int idPrestamo, int idUsuario)
    {
        using var conexion = CrearConexion();
        conexion.Open();
        return conexion.Execute(@"
            UPDATE Prestamos SET Cancelado = 1, fechaCancelacion = GETDATE()
            WHERE id = @id AND IdUsuario = @usuario AND Cancelado = 0
              AND FechaDevolucion >= CAST(GETDATE() AS date)",
            new { id = idPrestamo, usuario = idUsuario }) > 0;
    }
}
