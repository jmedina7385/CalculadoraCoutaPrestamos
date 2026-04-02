namespace CalculadoraCoutaPrestamos.Negocio;

public sealed class ResultadoCalculoCuota
{
    public bool Exito { get; init; }
    public decimal? Cuota { get; init; }
    public decimal? TasaAplicada { get; init; }
    public IReadOnlyList<AmortizacionCuota>? TablaAmortizacion { get; init; }
    public string? Mensaje { get; init; }
    public string? Titulo { get; init; }

    public static ResultadoCalculoCuota Ok(decimal cuota) =>
        new() { Exito = true, Cuota = cuota };

    public static ResultadoCalculoCuota Ok(
        decimal cuota,
        decimal tasaAplicada,
        IReadOnlyList<AmortizacionCuota> tablaAmortizacion) =>
        new()
        {
            Exito = true,
            Cuota = cuota,
            TasaAplicada = tasaAplicada,
            TablaAmortizacion = tablaAmortizacion,
        };

    public static ResultadoCalculoCuota Error(string mensaje, string? titulo = null) =>
        new() { Exito = false, Mensaje = mensaje, Titulo = titulo };
}
