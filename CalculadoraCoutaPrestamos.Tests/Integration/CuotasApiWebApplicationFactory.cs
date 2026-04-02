using CalculadoraCoutaPrestamos.Negocio;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CalculadoraCoutaPrestamos.Tests.Integration;

public sealed class CuotasApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var existentes = services.Where(d => d.ServiceType == typeof(ICalculadoraCuotasServicio)).ToList();
            foreach (var d in existentes)
            {
                services.Remove(d);
            }

            services.AddScoped<ICalculadoraCuotasServicio, StubCalculadoraCuotasServicio>();
        });
    }
}
