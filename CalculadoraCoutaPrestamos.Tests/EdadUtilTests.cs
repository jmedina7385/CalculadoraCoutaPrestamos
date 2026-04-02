using CalculadoraCoutaPrestamos.Negocio;

namespace CalculadoraCoutaPrestamos.Tests;

public class EdadUtilTests
{
    [Theory]
    [InlineData(1994, 4, 2, 2026, 4, 2, 32)]
    [InlineData(2001, 4, 2, 2026, 4, 2, 25)]
    [InlineData(2001, 4, 3, 2026, 4, 2, 24)]
    [InlineData(2000, 4, 2, 2026, 4, 2, 26)]
    [InlineData(2006, 4, 2, 2026, 4, 2, 20)]
    public void CalcularEdad_fechas_fijas(int anioNac, int mesNac, int diaNac, int anioRef, int mesRef, int diaRef, int esperado)
    {
        var nac = new DateOnly(anioNac, mesNac, diaNac);
        var referencia = new DateOnly(anioRef, mesRef, diaRef);
        Assert.Equal(esperado, EdadUtil.CalcularEdad(nac, referencia));
    }
}
