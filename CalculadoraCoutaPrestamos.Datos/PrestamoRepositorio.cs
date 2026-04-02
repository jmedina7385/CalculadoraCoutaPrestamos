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

    public async Task<decimal?> ObtenerTasaPorEdadAsync(int edad, CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var comando = new SqlCommand("dbo.usp_Tasas_ObtenerPorEdad", conexion)
        {
            CommandType = CommandType.StoredProcedure,
        };
        comando.Parameters.Add("@Edad", SqlDbType.Int).Value = edad;
        var resultado = await comando.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return resultado is DBNull or null ? null : Convert.ToDecimal(resultado);
    }

    public async Task<bool> PlazoEsValidoAsync(int valorMeses, CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var comando = new SqlCommand("dbo.usp_Plazos_ValidarValor", conexion)
        {
            CommandType = CommandType.StoredProcedure,
        };
        comando.Parameters.Add("@Valor", SqlDbType.Int).Value = valorMeses;
        var resultado = await comando.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (resultado is null or DBNull)
        {
            return false;
        }

        return Convert.ToBoolean(resultado, System.Globalization.CultureInfo.InvariantCulture);
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

    public async Task<int> InsertarLogConsultaAsync(
        int edad,
        decimal monto,
        int meses,
        decimal valorCuota,
        string? ipConsulta,
        CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var comando = new SqlCommand("dbo.usp_LogConsulta_Insertar", conexion)
        {
            CommandType = CommandType.StoredProcedure,
        };
        comando.Parameters.Add("@Edad", SqlDbType.Int).Value = edad;
        comando.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
        comando.Parameters["@Monto"].Precision = 18;
        comando.Parameters["@Monto"].Scale = 2;
        comando.Parameters.Add("@Meses", SqlDbType.Int).Value = meses;
        comando.Parameters.Add("@ValorCuota", SqlDbType.Decimal).Value = valorCuota;
        comando.Parameters["@ValorCuota"].Precision = 18;
        comando.Parameters["@ValorCuota"].Scale = 2;
        var pIp = comando.Parameters.Add("@IP_Consulta", SqlDbType.NVarChar, 45);
        pIp.Value = string.IsNullOrEmpty(ipConsulta) ? DBNull.Value : ipConsulta;

        var id = await comando.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(id);
    }
}
