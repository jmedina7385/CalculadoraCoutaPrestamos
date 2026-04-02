using CalculadoraCoutaPrestamos.Negocio;

namespace CalculadoraCoutaPrestamos.Tests.Integration;

internal sealed class StubCalculadoraCuotasServicio : ICalculadoraCuotasServicio
{
    public Task<ResultadoCalculoCuota> CalcularAsync(
        DateOnly fechaNacimiento,
        decimal monto,
        int meses,
        string? ipConsulta,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(ResultadoCalculoCuota.Ok(99.50m));

    public Task<IReadOnlyList<PlazoListaItem>> ObtenerPlazosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PlazoListaItem>>(
            new List<PlazoListaItem>
            {
                new("3 Meses", 3),
                new("12 Meses", 12),
            });
}
