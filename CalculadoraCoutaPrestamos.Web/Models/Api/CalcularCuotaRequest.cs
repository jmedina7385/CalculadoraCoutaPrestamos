using System.ComponentModel.DataAnnotations;

namespace CalculadoraCoutaPrestamos.Web.Models.Api;

public sealed class CalcularCuotaRequest
{
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly FechaNacimiento { get; set; }

    [Range(0.01, 1e15, ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La cantidad de meses es obligatoria.")]
    public int Meses { get; set; }
}
