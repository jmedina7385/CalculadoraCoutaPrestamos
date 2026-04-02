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
        mock.Setup(r => r.CalcularCuotaCompletoAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalculoCuotaCompletoDto(
                false,
                CalculadoraCuotasServicio.MensajeEdadMenor,
                CalculadoraCuotasServicio.TituloEdadMenor,
                null,
                null,
                Array.Empty<AmortizacionFilaDto>()));

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
        mock.Setup(r => r.CalcularCuotaCompletoAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<decimal>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalculoCuotaCompletoDto(
                false,
                CalculadoraCuotasServicio.MensajeEdadMayor,
                CalculadoraCuotasServicio.TituloEdadMayor,
                null,
                null,
                Array.Empty<AmortizacionFilaDto>()));

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
        mock.Setup(r => r.CalcularCuotaCompletoAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                0m,
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalculoCuotaCompletoDto(
                false,
                "El monto del préstamo debe ser mayor que cero.",
                null,
                null,
                null,
                Array.Empty<AmortizacionFilaDto>()));

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
        mock.Setup(r => r.CalcularCuotaCompletoAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<decimal>(),
                5,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalculoCuotaCompletoDto(
                false,
                "La cantidad de meses no es válida.",
                null,
                null,
                null,
                Array.Empty<AmortizacionFilaDto>()));

        var sut = new CalculadoraCuotasServicio(mock.Object);
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddYears(-22));

        var resultado = await sut.CalcularAsync(fecha, 1000, 5, null);

        Assert.False(resultado.Exito);
        Assert.Contains("meses", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalcularAsync_exito_mapea_cuota_tasa_y_tabla()
    {
        var tabla = Enumerable.Range(1, 12)
            .Select(i => new AmortizacionFilaDto(i, 0, 112m, 0, 0, 0))
            .ToList();

        var mock = new Mock<IPrestamoRepositorio>();
        mock.Setup(r => r.CalcularCuotaCompletoAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                1200m,
                12,
                "127.0.0.1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalculoCuotaCompletoDto(true, null, null, 112m, 1.12m, tabla))
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
