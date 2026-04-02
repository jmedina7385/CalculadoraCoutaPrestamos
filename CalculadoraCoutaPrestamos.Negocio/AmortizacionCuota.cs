namespace CalculadoraCoutaPrestamos.Negocio;

public sealed record AmortizacionCuota(
    int NumeroCuota,
    decimal SaldoInicial,
    decimal Cuota,
    decimal Interes,
    decimal Capital,
    decimal SaldoFinal);
