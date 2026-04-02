namespace CalculadoraCoutaPrestamos.Negocio;

public interface ICalculadoraCuotasServicio
{
    Task<ResultadoCalculoCuota> CalcularAsync(
        DateOnly fechaNacimiento,
        decimal monto,
        int meses,
        string? ipConsulta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlazoListaItem>> ObtenerPlazosAsync(CancellationToken cancellationToken = default);
}

public sealed record PlazoListaItem(string Descripcion, int Valor);
