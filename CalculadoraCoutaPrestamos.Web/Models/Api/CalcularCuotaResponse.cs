namespace CalculadoraCoutaPrestamos.Web.Models.Api;

public sealed class CalcularCuotaResponse
{
    public bool Exito { get; set; }
    public decimal? Cuota { get; set; }
    public string? Mensaje { get; set; }
    public string? Titulo { get; set; }
    public decimal? TasaAplicada { get; set; }
    public IReadOnlyList<AmortizacionCuotaResponse>? TablaAmortizacion { get; set; }
    public string? InformacionSucursal { get; set; }
}
