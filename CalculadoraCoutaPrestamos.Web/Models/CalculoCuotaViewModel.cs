using System.ComponentModel.DataAnnotations;

namespace CalculadoraCoutaPrestamos.Web.Models;

public sealed class CalculoCuotaViewModel : IValidatableObject
{
    /// <summary>Límite inferior de fecha de nacimiento (coherente con la vista y el input type="date").</summary>
    public const int AniosMinimoAntiguedadNacimiento = 100;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly FechaNacimiento { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, 1e15, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto del préstamo")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "Seleccione un plazo en meses.")]
    [Display(Name = "Plazo (meses)")]
    public int Meses { get; set; } = 12;

    [Display(Name = "Cuota calculada")]
    public decimal? CuotaCalculada { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var fechaMinPermitida = hoy.AddYears(-AniosMinimoAntiguedadNacimiento);

        if (FechaNacimiento > hoy)
        {
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser posterior a hoy.",
                new[] { nameof(FechaNacimiento) });
        }

        if (FechaNacimiento < fechaMinPermitida)
        {
            yield return new ValidationResult(
                $"Indique una fecha de nacimiento realista (no anterior al {fechaMinPermitida:dd/MM/yyyy}).",
                new[] { nameof(FechaNacimiento) });
        }
    }
}
