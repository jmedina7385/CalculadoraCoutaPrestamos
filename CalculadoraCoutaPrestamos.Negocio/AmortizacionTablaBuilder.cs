namespace CalculadoraCoutaPrestamos.Negocio;

public static class AmortizacionTablaBuilder
{
    /// <summary>
    /// Genera la tabla mes a mes: capital casi uniforme (última cuota ajusta centavos) e interés como complemento de la cuota fija.
    /// </summary>
    public static IReadOnlyList<AmortizacionCuota> Generar(decimal monto, int meses, decimal cuota)
    {
        if (meses <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(meses));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monto));
        }

        var filas = new List<AmortizacionCuota>(meses);
        var saldo = monto;
        var capitalBase = Math.Round(monto / meses, 2, MidpointRounding.AwayFromZero);

        for (var i = 1; i <= meses; i++)
        {
            var saldoInicial = saldo;
            var capital = i < meses ? capitalBase : saldoInicial;
            var interes = cuota - capital;
            if (interes < 0)
            {
                interes = 0;
            }

            saldo = saldoInicial - capital;
            if (saldo < 0)
            {
                saldo = 0;
            }

            filas.Add(new AmortizacionCuota(i, saldoInicial, cuota, interes, capital, saldo));
        }

        return filas;
    }
}
