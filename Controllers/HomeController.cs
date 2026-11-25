using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP_Final_Lucio.Models;

namespace TP_Final_Lucio.Controllers;

public class HomeController : Controller
{
    private readonly BD _bd;

    public HomeController(BD bd)
    {
        _bd = bd;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetInt32("IdUsuario") != null)
            return RedirectToAction("Catalogo");
        return View();
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string nombreUsuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewBag.Error = "Completá el usuario y la contraseña.";
            return View();
        }

        var usuario = _bd.ValidarUsuario(nombreUsuario.Trim(), contrasena);
        if (usuario == null)
        {
            ViewBag.Error = "El usuario o la contraseña son incorrectos.";
            return View();
        }

        HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Apellido", usuario.Apellido);
        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        return RedirectToAction("Catalogo");
    }

    [HttpGet]
    public IActionResult CrearCuenta() => View();

    [HttpPost]
    public IActionResult CrearCuenta(string nombre, string apellido, string nombreUsuario, string contrasena)
    {
        nombre = (nombre ?? "").Trim();
        apellido = (apellido ?? "").Trim();
        nombreUsuario = (nombreUsuario ?? "").Trim();

        if (nombre.Length == 0 || apellido.Length == 0 || nombreUsuario.Length == 0 || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewBag.Error = "Completá todos los campos.";
            return View();
        }
        if (nombre.Length > 50 || apellido.Length > 50 || nombreUsuario.Length > 50 || contrasena.Length > 50)
        {
            ViewBag.Error = "Los campos no pueden superar los 50 caracteres.";
            return View();
        }
        if (contrasena.Length < 6)
        {
            ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
            return View();
        }
        if (_bd.ExisteNombreUsuario(nombreUsuario))
        {
            ViewBag.Error = "Ese nombre de usuario ya está registrado.";
            return View();
        }

        _bd.CrearUsuario(nombreUsuario, contrasena, nombre, apellido);
        var usuario = _bd.ValidarUsuario(nombreUsuario, contrasena);
        if (usuario != null)
        {
            HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
            HttpContext.Session.SetString("Apellido", usuario.Apellido);
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
        }
        return RedirectToAction("Catalogo");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Catalogo()
    {
        if (!RequiereSesion()) return RedirectToAction("Login");
        ViewBag.Libros = _bd.ObtenerLibros(0, 8);
        ViewBag.HayMas = _bd.ObtenerLibros(8, 1).Count > 0;
        return View();
    }

    [HttpGet]
    public IActionResult CargarMas(int desde)
    {
        if (!RequiereSesion()) return Unauthorized();
        var libros = _bd.ObtenerLibros(desde, 8);
        return Json(libros);
    }

    [HttpGet]
    public IActionResult Detalle(int id)
    {
        if (!RequiereSesion()) return RedirectToAction("Login");
        var libro = _bd.ObtenerLibro(id);
        if (libro == null) return NotFound();
        return View(libro);
    }

    [HttpPost]
    public IActionResult SolicitarPrestamo(int idLibro)
    {
        if (!RequiereSesion()) return RedirectToAction("Login");
        int idUsuario = HttpContext.Session.GetInt32("IdUsuario")!.Value;

        var libro = _bd.ObtenerLibro(idLibro);
        if (libro == null)
        {
            TempData["Mensaje"] = "El libro no existe.";
            return RedirectToAction("Catalogo");
        }
        if (!Request.Form.TryGetValue("fechaRetiro", out var valorFecha)
            || !DateTime.TryParse(valorFecha, out DateTime fechaRetiro))
        {
            TempData["Mensaje"] = "Elegí una fecha de retiro válida.";
            return RedirectToAction("Detalle", new { id = idLibro });
        }

        DateTime hoy = DateTime.Today;
        if (fechaRetiro.Date < hoy)
        {
            TempData["Mensaje"] = "La fecha de retiro no puede ser anterior a hoy.";
            return RedirectToAction("Detalle", new { id = idLibro });
        }

        DateTime fechaDevolucion = fechaRetiro.Date.AddDays(7);
        if (_bd.ContarPrestamosEnPeriodo(idUsuario, fechaRetiro.Date, fechaDevolucion) >= 3)
        {
            TempData["Mensaje"] = "Ya tenés 3 préstamos activos. Cancelá o esperá a devolver uno para pedir otro.";
            return RedirectToAction("Detalle", new { id = idLibro });
        }
        DateTime? hasta = _bd.ObtenerHastaCuandoEstaPrestado(idLibro, fechaRetiro.Date, fechaDevolucion);
        if (hasta != null)
        {
            TempData["Mensaje"] = $"Ese libro está prestado hasta el {hasta.Value:dd/MM/yyyy}.";
            return RedirectToAction("Detalle", new { id = idLibro });
        }

        string codigo = _bd.CrearPrestamo(idLibro, idUsuario, fechaRetiro.Date, fechaDevolucion);
        TempData["Mensaje"] = "Préstamo solicitado. Tu código de 6 caracteres es: " + codigo;
        return RedirectToAction("MisPrestamos");
    }

    [HttpGet]
    public IActionResult Disponibilidad(int id, DateTime? fecha)
    {
        if (!RequiereSesion()) return Unauthorized();
        DateTime retiro = fecha?.Date ?? DateTime.Today;
        if (retiro < DateTime.Today)
            return Json(new { disponible = false, hasta = "", mensaje = "La fecha de retiro no puede ser anterior a hoy." });

        DateTime devolucion = retiro.AddDays(7);
        DateTime? hasta = _bd.ObtenerHastaCuandoEstaPrestado(id, retiro, devolucion);
        if (hasta == null)
            return Json(new { disponible = true, hasta = "", mensaje = "Disponible para ese período." });

        return Json(new
        {
            disponible = false,
            hasta = hasta.Value.ToString("dd/MM/yyyy"),
            mensaje = $"Ese libro está prestado hasta el {hasta.Value:dd/MM/yyyy}."
        });
    }

    [HttpGet]
    public IActionResult MisPrestamos()
    {
        if (!RequiereSesion()) return RedirectToAction("Login");
        int idUsuario = HttpContext.Session.GetInt32("IdUsuario")!.Value;
        ViewBag.Activos = _bd.ObtenerPrestamos(idUsuario, false);
        ViewBag.Pasados = _bd.ObtenerPrestamos(idUsuario, true);
        return View("Prestamos");
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult CancelarPrestamo(int id)
    {
        if (!RequiereSesion()) return Unauthorized();
        int idUsuario = HttpContext.Session.GetInt32("IdUsuario")!.Value;
        bool cancelado = _bd.CancelarPrestamo(id, idUsuario);
        return Json(new { ok = cancelado, mensaje = cancelado ? "Préstamo cancelado." : "No se pudo cancelar el préstamo." });
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool RequiereSesion() => HttpContext.Session.GetInt32("IdUsuario") != null;
}
