using CalculadoraCoutaPrestamos.Negocio;
using CalculadoraCoutaPrestamos.Web.Infra;
using CalculadoraCoutaPrestamos.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraCoutaPrestamos.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CuotasController : ControllerBase
{
    private readonly ICalculadoraCuotasServicio _servicio;

    public CuotasController(ICalculadoraCuotasServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpPost("calcular")]
    [ProducesResponseType(typeof(CalcularCuotaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CalcularCuotaResponse>> Calcular(
        [FromBody] CalcularCuotaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var error = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Ok(new CalcularCuotaResponse
            {
                Exito = false,
                Mensaje = error,
                Titulo = "Datos no válidos",
            });
        }

        var ip = HttpContextIp.ObtenerIpConsulta(HttpContext);
        var resultado = await _servicio.CalcularAsync(
                request.FechaNacimiento,
                request.Monto,
                request.Meses,
                ip,
                cancellationToken)
            .ConfigureAwait(false);

        var respuesta = new CalcularCuotaResponse
        {
            Exito = resultado.Exito,
            Cuota = resultado.Cuota,
            Mensaje = resultado.Mensaje,
            Titulo = resultado.Titulo,
        };

        if (resultado.Exito)
        {
            respuesta.InformacionSucursal = CalculadoraCuotasServicio.MensajeVisitarSucursal;
            respuesta.TasaAplicada = resultado.TasaAplicada;
            if (resultado.TablaAmortizacion is { Count: > 0 } t)
            {
                respuesta.TablaAmortizacion = t
                    .Select(f => new AmortizacionCuotaResponse
                    {
                        NumeroCuota = f.NumeroCuota,
                        SaldoInicial = f.SaldoInicial,
                        Cuota = f.Cuota,
                        Interes = f.Interes,
                        Capital = f.Capital,
                        SaldoFinal = f.SaldoFinal,
                    })
                    .ToList();
            }
        }

        return Ok(respuesta);
    }

    [HttpGet("plazos")]
    [ProducesResponseType(typeof(IReadOnlyList<PlazoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlazoResponse>>> Plazos(CancellationToken cancellationToken)
    {
        var plazos = await _servicio.ObtenerPlazosAsync(cancellationToken).ConfigureAwait(false);
        var respuesta = plazos
            .Select(p => new PlazoResponse { Descripcion = p.Descripcion, Valor = p.Valor })
            .ToList();
        return Ok(respuesta);
    }
}
