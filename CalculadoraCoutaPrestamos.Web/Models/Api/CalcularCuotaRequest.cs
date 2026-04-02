using System.ComponentModel.DataAnnotations;

namespace CalculadoraCoutaPrestamos.Web.Models.Api;

public sealed class CalcularCuotaRequest
{
    [Required]
    public DateOnly FechaNacimiento { get; set; }

    [Range(0.01, 1e15)]
    public decimal Monto { get; set; }

    [Required]
    public int Meses { get; set; }
}
