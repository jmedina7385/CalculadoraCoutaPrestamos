namespace CalculadoraCoutaPrestamos.Negocio;

public static class EdadUtil
{
    public static int CalcularEdad(DateOnly fechaNacimiento, DateOnly fechaReferencia)
    {
        var edad = fechaReferencia.Year - fechaNacimiento.Year;
        if (fechaNacimiento > fechaReferencia.AddYears(-edad))
        {
            edad--;
        }

        return edad;
    }
}
