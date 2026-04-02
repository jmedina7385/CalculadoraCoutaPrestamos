using CalculadoraCoutaPrestamos.Datos;
using CalculadoraCoutaPrestamos.Negocio;
using Moq;

namespace CalculadoraCoutaPrestamos.Tests;

public class CalculadoraCuotasServicioTests
{
    [Fact]
    public async Task CalcularAsync_edad_menor_18_devuelve_mensaje_requerido()
    {
        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.InsertarLogConsultaAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-16));

        var resultado = await sut.CalcularAsync(fecha, 1000, 12, null);

        Assert.False(resultado.Exito);
        Assert.Equal(CalculadoraCuotasServicio.MensajeEdadMenor, resultado.Mensaje);
        Assert.Equal(CalculadoraCuotasServicio.TituloEdadMenor, resultado.Titulo);
    }

    [Fact]
    public async Task CalcularAsync_edad_mayor_25_devuelve_mensaje_requerido()
    {
        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.InsertarLogConsultaAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));

        var resultado = await sut.CalcularAsync(fecha, 1000, 12, null);

        Assert.False(resultado.Exito);
        Assert.Equal(CalculadoraCuotasServicio.MensajeEdadMayor, resultado.Mensaje);
        Assert.Equal(CalculadoraCuotasServicio.TituloEdadMayor, resultado.Titulo);
    }

    [Fact]
    public async Task CalcularAsync_monto_no_positivo_devuelve_error()
    {
        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.InsertarLogConsultaAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-22));

        var resultado = await sut.CalcularAsync(fecha, 0, 12, null);

        Assert.False(resultado.Exito);
        Assert.Contains("monto", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalcularAsync_plazo_invalido_devuelve_error()
    {
        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.PlazoEsValidoAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mock.Setup(r => r.InsertarLogConsultaAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-22));

        var resultado = await sut.CalcularAsync(fecha, 1000, 5, null);

        Assert.False(resultado.Exito);
        Assert.Contains("meses", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalcularAsync_exito_aplica_formula_y_redondea()
    {
        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.PlazoEsValidoAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mock.Setup(r => r.ObtenerTasaPorEdadAsync(22, It.IsAny<CancellationToken>())).ReturnsAsync(1.12m);
        mock.Setup(r => r.InsertarLogConsultaAsync(
                22,
                1200m,
                12,
                112m,
                "127.0.0.1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1)
            .Verifiable();

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-22));

        var resultado = await sut.CalcularAsync(fecha, 1200, 12, "127.0.0.1");

        Assert.True(resultado.Exito);
        Assert.Equal(112m, resultado.Cuota);
        Assert.Equal(1.12m, resultado.TasaAplicada);
        Assert.NotNull(resultado.TablaAmortizacion);
        Assert.Equal(12, resultado.TablaAmortizacion!.Count);
        mock.Verify();
    }
}
