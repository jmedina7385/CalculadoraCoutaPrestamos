using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CalculadoraCoutaPrestamos.Datos;
using CalculadoraCoutaPrestamos.Negocio;
using Microsoft.AspNetCore.Localization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Calculadora de cuotas API", Version = "v1" });
});

var cadenaSql = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "Configure la cadena de conexión SqlServer en appsettings.json.");
builder.Services.AddScoped<IPrestamoRepositorio>(_ => new PrestamoRepositorio(cadenaSql));
builder.Services.AddScoped<ICalculadoraCuotasServicio, CalculadoraCuotasServicio>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("es-DO"), new CultureInfo("es") };
    options.DefaultRequestCulture = new RequestCulture("es-DO");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Calculadora cuotas v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Prestamos}/{action=Index}/{id?}");

app.Run();