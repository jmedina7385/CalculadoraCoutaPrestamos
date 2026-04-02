using System.ComponentModel.DataAnnotations;

namespace CalculadoraCoutaPrestamos.Web.Models;

public sealed class CalculoCuotaViewModel
{
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly FechaNacimiento { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, 1e15, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto del préstamo")]
    public decimal Monto { get; set; }

    [Required]
    [Display(Name = "Plazo (meses)")]
    public int Meses { get; set; } = 12;

    [Display(Name = "Cuota calculada")]
    public decimal? CuotaCalculada { get; set; }
}
