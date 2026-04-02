namespace CalculadoraCoutaPrestamos.Web.Models.Api;

public sealed class AmortizacionCuotaResponse
{
    public int NumeroCuota { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal Cuota { get; set; }
    public decimal Interes { get; set; }
    public decimal Capital { get; set; }
    public decimal SaldoFinal { get; set; }
}
