using CalculadoraCoutaPrestamos.Datos;

namespace CalculadoraCoutaPrestamos.Negocio;

public sealed class CalculadoraCuotasServicio : ICalculadoraCuotasServicio
{
    public const string MensajeEdadMenor =
        "Lo sentimos, aún no cuenta con la edad para solicitar este producto.";

    public const string MensajeVisitarSucursal =
        "Favor pasar por una de nuestras sucursales para evaluar su caso.";

    public const string MensajeEdadMayor = MensajeVisitarSucursal;

    public const string TituloEdadMenor = "Edad mínima no alcanzada";
    public const string TituloEdadMayor = "Su caso debe evaluarse en sucursal";

    private readonly IPrestamoRepositorio _repositorio;

    public CalculadoraCuotasServicio(IPrestamoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoCalculoCuota> CalcularAsync(
        DateOnly fechaNacimiento,
        decimal monto,
        int meses,
        string? ipConsulta,
        CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var dto = await _repositorio
            .CalcularCuotaCompletoAsync(fechaNacimiento, hoy, monto, meses, ipConsulta, cancellationToken)
            .ConfigureAwait(false);

        if (!dto.Exito)
        {
            return ResultadoCalculoCuota.Error(dto.Mensaje ?? string.Empty, dto.Titulo);
        }

        var tabla = dto.TablaAmortizacion
            .Select(f => new AmortizacionCuota(f.NumeroCuota, f.SaldoInicial, f.Cuota, f.Interes, f.Capital, f.SaldoFinal))
            .ToList();

        return ResultadoCalculoCuota.Ok(dto.Cuota!.Value, dto.TasaAplicada!.Value, tabla);
    }

    public async Task<IReadOnlyList<PlazoListaItem>> ObtenerPlazosAsync(CancellationToken cancellationToken = default)
    {
        var plazos = await _repositorio.ListarPlazosAsync(cancellationToken).ConfigureAwait(false);
        return plazos.Select(p => new PlazoListaItem(p.Descripcion, p.Valor)).ToList();
    }
}
