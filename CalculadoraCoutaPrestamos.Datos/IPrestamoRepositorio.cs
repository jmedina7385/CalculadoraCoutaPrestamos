namespace CalculadoraCoutaPrestamos.Datos;

public interface IPrestamoRepositorio
{
    /// <summary>
    /// Ejecuta el cálculo en base de datos (edad, validaciones, tasa, cuota, amortización y log).
    /// </summary>
    Task<CalculoCuotaCompletoDto> CalcularCuotaCompletoAsync(
        DateOnly fechaNacimiento,
        DateOnly fechaReferencia,
        decimal monto,
        int meses,
        string? ipConsulta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlazoItemDto>> ListarPlazosAsync(CancellationToken cancellationToken = default);
}

public sealed record PlazoItemDto(string Descripcion, int Valor);

public sealed record AmortizacionFilaDto(
    int NumeroCuota,
    decimal SaldoInicial,
    decimal Cuota,
    decimal Interes,
    decimal Capital,
    decimal SaldoFinal);

public sealed record CalculoCuotaCompletoDto(
    bool Exito,
    string? Mensaje,
    string? Titulo,
    decimal? Cuota,
    decimal? TasaAplicada,
    IReadOnlyList<AmortizacionFilaDto> TablaAmortizacion);
