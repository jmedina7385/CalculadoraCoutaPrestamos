using System.Net.Http.Json;
using System.Text.Json;
using CalculadoraCoutaPrestamos.Negocio;

namespace CalculadoraCoutaPrestamos.Tests.Integration;

public sealed class CuotasApiIntegrationTests : IClassFixture<CuotasApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;

    public CuotasApiIntegrationTests(CuotasApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_plazos_devuelve_lista_en_json()
    {
        var response = await _client.GetAsync(new Uri("/api/cuotas/plazos", UriKind.Relative));
        response.EnsureSuccessStatusCode();
        var lista = await response.Content.ReadFromJsonAsync<IReadOnlyList<PlazoApiDto>>(JsonReadOptions);
        Assert.NotNull(lista);
        Assert.Equal(2, lista.Count);
        Assert.Equal(12, lista[^1].Valor);
        Assert.Contains("12", lista[^1].Descripcion, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_calcular_acepta_cuerpo_json_y_devuelve_cuota()
    {
        var body = new CalcularCuerpoDto
        {
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Monto = 1000m,
            Meses = 12,
        };

        var response = await _client.PostAsJsonAsync("/api/cuotas/calcular", body);
        response.EnsureSuccessStatusCode();
        var resultado = await response.Content.ReadFromJsonAsync<CalcularRespuestaDto>(JsonReadOptions);
        Assert.NotNull(resultado);
        Assert.True(resultado.Exito);
        Assert.Equal(99.50m, resultado.Cuota);
        Assert.Equal(
            CalculadoraCuotasServicio.MensajeVisitarSucursal,
            resultado.InformacionSucursal);
        Assert.Null(resultado.Titulo);
    }

    private sealed class PlazoApiDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public int Valor { get; set; }
    }

    private sealed class CalcularCuerpoDto
    {
        public DateOnly FechaNacimiento { get; set; }
        public decimal Monto { get; set; }
        public int Meses { get; set; }
    }

    private sealed class CalcularRespuestaDto
    {
        public bool Exito { get; set; }
        public decimal? Cuota { get; set; }
        public string? Mensaje { get; set; }
        public string? Titulo { get; set; }
        public string? InformacionSucursal { get; set; }
    }
}
