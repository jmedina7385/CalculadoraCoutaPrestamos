using System.Linq;
using CalculadoraCoutaPrestamos.Negocio;

namespace CalculadoraCoutaPrestamos.Tests;

public class AmortizacionTablaBuilderTests
{
    [Fact]
    public void Generar_suma_capital_igual_a_monto_y_ultimo_saldo_cero()
    {
        const decimal monto = 1200m;
        const int meses = 12;
        const decimal cuota = 112m;

        var tabla = AmortizacionTablaBuilder.Generar(monto, meses, cuota);

        Assert.Equal(meses, tabla.Count);
        Assert.Equal(monto, tabla.Sum(f => f.Capital));
        Assert.Equal(0m, tabla[^1].SaldoFinal);
        Assert.True(tabla.All(f => f.Cuota == cuota));
    }

    [Fact]
    public void Generar_tres_meses_primera_fila_saldo_inicial_es_monto()
    {
        var tabla = AmortizacionTablaBuilder.Generar(1000m, 3, 400m);

        Assert.Equal(3, tabla.Count);
        Assert.Equal(1000m, tabla[0].SaldoInicial);
        Assert.Equal(400m, tabla[0].Cuota);
        Assert.Equal(1000m, tabla.Sum(f => f.Capital));
    }
}
