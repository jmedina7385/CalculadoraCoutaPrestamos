using System.Data;
using Microsoft.Data.SqlClient;

namespace CalculadoraCoutaPrestamos.Datos;

public sealed class PrestamoRepositorio : IPrestamoRepositorio
{
    private readonly string _cadenaConexion;

    public PrestamoRepositorio(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<CalculoCuotaCompletoDto> CalcularCuotaCompletoAsync(
        DateOnly fechaNacimiento,
        DateOnly fechaReferencia,
        decimal monto,
        int meses,
        string? ipConsulta,
        CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var comando = new SqlCommand("dbo.usp_Cuotas_Calcular", conexion)
        {
            CommandType = CommandType.StoredProcedure,
        };
        comando.Parameters.Add("@FechaNacimiento", SqlDbType.Date).Value = fechaNacimiento.ToDateTime(TimeOnly.MinValue);
        comando.Parameters.Add("@FechaReferencia", SqlDbType.Date).Value = fechaReferencia.ToDateTime(TimeOnly.MinValue);
        comando.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
        comando.Parameters["@Monto"].Precision = 18;
        comando.Parameters["@Monto"].Scale = 2;
        comando.Parameters.Add("@Meses", SqlDbType.Int).Value = meses;
        var pIp = comando.Parameters.Add("@IP_Consulta", SqlDbType.NVarChar, 45);
        pIp.Value = string.IsNullOrEmpty(ipConsulta) ? DBNull.Value : ipConsulta;

        await using var lector = await comando.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await lector.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("dbo.usp_Cuotas_Calcular no devolvió el resultado esperado.");
        }

        var oExito = lector.GetOrdinal("Exito");
        var oMensaje = lector.GetOrdinal("Mensaje");
        var oTitulo = lector.GetOrdinal("Titulo");
        var oCuota = lector.GetOrdinal("Cuota");
        var oTasa = lector.GetOrdinal("TasaAplicada");

        var exito = lector.GetBoolean(oExito);
        var mensaje = lector.IsDBNull(oMensaje) ? null : lector.GetString(oMensaje);
        var titulo = lector.IsDBNull(oTitulo) ? null : lector.GetString(oTitulo);
        var cuota = lector.IsDBNull(oCuota) ? (decimal?)null : lector.GetDecimal(oCuota);
        var tasa = lector.IsDBNull(oTasa) ? (decimal?)null : lector.GetDecimal(oTasa);

        IReadOnlyList<AmortizacionFilaDto> tabla = Array.Empty<AmortizacionFilaDto>();
        if (exito && await lector.NextResultAsync(cancellationToken).ConfigureAwait(false))
        {
            var filas = new List<AmortizacionFilaDto>();
            var on = lector.GetOrdinal("NumeroCuota");
            var osi = lector.GetOrdinal("SaldoInicial");
            var oc = lector.GetOrdinal("Cuota");
            var oi = lector.GetOrdinal("Interes");
            var ok = lector.GetOrdinal("Capital");
            var osf = lector.GetOrdinal("SaldoFinal");
            while (await lector.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                filas.Add(new AmortizacionFilaDto(
                    lector.GetInt32(on),
                    lector.GetDecimal(osi),
                    lector.GetDecimal(oc),
                    lector.GetDecimal(oi),
                    lector.GetDecimal(ok),
                    lector.GetDecimal(osf)));
            }

            tabla = filas;
        }

        return new CalculoCuotaCompletoDto(exito, mensaje, titulo, cuota, tasa, tabla);
    }

    public async Task<IReadOnlyList<PlazoItemDto>> ListarPlazosAsync(CancellationToken cancellationToken = default)
    {
        var lista = new List<PlazoItemDto>();
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var comando = new SqlCommand("dbo.usp_Plazos_Listar", conexion)
        {
            CommandType = CommandType.StoredProcedure,
        };
        await using var lector = await comando.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await lector.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            lista.Add(new PlazoItemDto(lector.GetString(0), lector.GetInt32(1)));
        }

        return lista;
    }
}
