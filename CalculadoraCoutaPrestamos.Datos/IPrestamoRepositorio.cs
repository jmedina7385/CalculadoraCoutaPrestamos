namespace CalculadoraCoutaPrestamos.Datos;

public interface IPrestamoRepositorio
{
    Task<decimal?> ObtenerTasaPorEdadAsync(int edad, CancellationToken cancellationToken = default);

    Task<bool> PlazoEsValidoAsync(int valorMeses, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlazoItemDto>> ListarPlazosAsync(CancellationToken cancellationToken = default);

    Task<int> InsertarLogConsultaAsync(
        int edad,
        decimal monto,
        int meses,
        decimal valorCuota,
        string? ipConsulta,
        CancellationToken cancellationToken = default);
}

public sealed record PlazoItemDto(string Descripcion, int Valor);
