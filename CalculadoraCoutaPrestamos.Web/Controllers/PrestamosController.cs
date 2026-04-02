using CalculadoraCoutaPrestamos.Negocio;
using CalculadoraCoutaPrestamos.Web.Infra;
using CalculadoraCoutaPrestamos.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraCoutaPrestamos.Web.Controllers;

public class PrestamosController : Controller
{
    private readonly ICalculadoraCuotasServicio _servicio;

    public PrestamosController(ICalculadoraCuotasServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        await CargarPlazosAsync(cancellationToken).ConfigureAwait(false);
        return View(new CalculoCuotaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CalculoCuotaViewModel modelo, CancellationToken cancellationToken)
    {
        await CargarPlazosAsync(cancellationToken).ConfigureAwait(false);
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var ip = HttpContextIp.ObtenerIpConsulta(HttpContext);
        var resultado = await _servicio.CalcularAsync(
                modelo.FechaNacimiento,
                modelo.Monto,
                modelo.Meses,
                ip,
                cancellationToken)
            .ConfigureAwait(false);

        if (!resultado.Exito)
        {
            ViewBag.ModalTipo = "danger";
            ViewBag.ModalTitulo = resultado.Titulo ?? "No se pudo calcular la cuota";
            ViewBag.ModalMensaje = resultado.Mensaje;
            return View(modelo);
        }

        modelo.CuotaCalculada = resultado.Cuota;
        ViewBag.ModalTipo = "success";
        ViewBag.ModalTitulo = "Cálculo realizado";
        ViewBag.ModalCuota = resultado.Cuota;
        ViewBag.ModalTasa = resultado.TasaAplicada;
        ViewBag.ModalTabla = resultado.TablaAmortizacion;
        ViewBag.ModalAviso = CalculadoraCuotasServicio.MensajeVisitarSucursal;
        return View(modelo);
    }

    private async Task CargarPlazosAsync(CancellationToken cancellationToken)
    {
        var plazos = await _servicio.ObtenerPlazosAsync(cancellationToken).ConfigureAwait(false);
        ViewBag.Plazos = plazos;
    }
}
