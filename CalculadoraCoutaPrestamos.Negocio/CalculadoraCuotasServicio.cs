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

    private const int EdadMinimaTabla = 18;
    private const int EdadMaximaTabla = 25;

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
        var edad = EdadUtil.CalcularEdad(fechaNacimiento, hoy);
        if (edad < EdadMinimaTabla)
        {
            await RegistrarConsultaAsync(edad, monto, meses, 0, ipConsulta, cancellationToken).ConfigureAwait(false);
            return ResultadoCalculoCuota.Error(MensajeEdadMenor, TituloEdadMenor);
        }

        if (edad > EdadMaximaTabla)
        {
            await RegistrarConsultaAsync(edad, monto, meses, 0, ipConsulta, cancellationToken).ConfigureAwait(false);
            return ResultadoCalculoCuota.Error(MensajeEdadMayor, TituloEdadMayor);
        }

        if (monto <= 0)
        {
            await RegistrarConsultaAsync(edad, monto, meses, 0, ipConsulta, cancellationToken).ConfigureAwait(false);
            return ResultadoCalculoCuota.Error("El monto del préstamo debe ser mayor que cero.");
        }

        if (!await _repositorio.PlazoEsValidoAsync(meses, cancellationToken).ConfigureAwait(false))
        {
            await RegistrarConsultaAsync(edad, monto, meses, 0, ipConsulta, cancellationToken).ConfigureAwait(false);
            return ResultadoCalculoCuota.Error("La cantidad de meses no es válida.");
        }

        var tasa = await _repositorio.ObtenerTasaPorEdadAsync(edad, cancellationToken).ConfigureAwait(false);
        if (tasa is null)
        {
            await RegistrarConsultaAsync(edad, monto, meses, 0, ipConsulta, cancellationToken).ConfigureAwait(false);
            return ResultadoCalculoCuota.Error("No se encontró tasa para la edad indicada.");
        }

        var cuota = Math.Round(monto * tasa.Value / meses, 2, MidpointRounding.AwayFromZero);
        var tabla = AmortizacionTablaBuilder.Generar(monto, meses, cuota);
        await RegistrarConsultaAsync(edad, monto, meses, cuota, ipConsulta, cancellationToken).ConfigureAwait(false);
        return ResultadoCalculoCuota.Ok(cuota, tasa.Value, tabla);
    }

    public async Task<IReadOnlyList<PlazoListaItem>> ObtenerPlazosAsync(CancellationToken cancellationToken = default)
    {
        var plazos = await _repositorio.ListarPlazosAsync(cancellationToken).ConfigureAwait(false);
        return plazos.Select(p => new PlazoListaItem(p.Descripcion, p.Valor)).ToList();
    }

    private Task RegistrarConsultaAsync(
        int edad,
        decimal monto,
        int meses,
        decimal valorCuota,
        string? ipConsulta,
        CancellationToken cancellationToken) =>
        _repositorio.InsertarLogConsultaAsync(edad, monto, meses, valorCuota, ipConsulta, cancellationToken);
}
