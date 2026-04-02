# Calculadora de cuotas

## Tecnologías

| Área | Detalle |
|------|---------|
| **Runtime** | .NET 8 (C#), proyectos con nullable habilitado |
| **Web** | ASP.NET Core: **MVC** (Razor), **API REST** (`/api/...`) y archivos estáticos |
| **Documentación API** | **Swagger** (Swashbuckle) solo con `ASPNETCORE_ENVIRONMENT=Development` |
| **Capas** | `CalculadoraCoutaPrestamos.Web` → `CalculadoraCoutaPrestamos.Negocio` → `CalculadoraCoutaPrestamos.Datos` |
| **Datos** | **SQL Server** (o LocalDB), acceso con **Microsoft.Data.SqlClient**, lógica en **procedimientos almacenados** |
| **Front (MVC)** | **Bootstrap 5**, **IMask** (monto en **RD$ / DOP**, miles con coma) en `wwwroot` |
| **Cultura** | `es-DO` (RequestLocalization) |
| **Pruebas** | **xUnit**, **Moq**, **Microsoft.AspNetCore.Mvc.Testing** |
| **Amortización** | Tras un cálculo exitoso, **tabla mes a mes** (capital / interés / saldos) en modal (MVC) y en la respuesta JSON de la API |

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) o **LocalDB** (incluido con Visual Studio / herramientas de datos)

---

## Cómo ejecutar el proyecto (paso a paso)

### 1. Obtener el código

Abre una terminal en la carpeta raíz del repositorio (donde está el `.sln` o las carpetas de proyecto).

### 2. Base de datos

1. Abre **SQL Server Management Studio** (o `sqlcmd`) con acceso a tu instancia (por ejemplo `(localdb)\MSSQLLocalDB`).
2. Ejecuta los scripts **en este orden**:
   - `Database/00_CreateDatabase.sql`
   - `Database/01_Schema.sql`
3. Abre `CalculadoraCoutaPrestamos.Web/appsettings.json` y ajusta **`ConnectionStrings:SqlServer`** si tu servidor, base de datos o autenticación no coinciden con el ejemplo (por defecto apunta a LocalDB y la base `CalculadoraPrestamos`).

El cálculo de cuota, validaciones por edad, tasa, tabla de amortización e inserción en bitácora se concentran en el procedimiento **`dbo.usp_Cuotas_Calcular`**. Si actualizas el código sin recrear la base, despliega ese procedimiento (definición en `Database/01_Schema.sql`).

### 3. Restaurar dependencias .NET

```powershell
dotnet restore
```

(Es opcional si el siguiente comando ya restaura al compilar.)

### 4. Ejecutar la aplicación web

Desde la **raíz del repo**:

```powershell
dotnet run --project CalculadoraCoutaPrestamos.Web
```

Cuando indique que está escuchando (por ejemplo `https://localhost:7094`), abre el navegador:

| URL | Descripción |
|-----|-------------|
| `/Prestamos` | Calculadora **MVC** |
| `/swagger` | Documentación OpenAPI (solo en **Development**) |

La variable `ASPNETCORE_ENVIRONMENT` suele ser `Development` al ejecutar con `dotnet run` desde el proyecto.

---

## Pruebas automatizadas

En la raíz del repo:

```powershell
dotnet test
```

---

## Estructura de la solución

- **`CalculadoraCoutaPrestamos.Web`** — MVC, API y `wwwroot`
- **`CalculadoraCoutaPrestamos.Negocio`** — reglas de negocio (cuotas, edad, tabla de amortización, mensajes)
- **`CalculadoraCoutaPrestamos.Datos`** — repositorio SQL y llamadas a procedimientos
- **`CalculadoraCoutaPrestamos.Tests`** — pruebas unitarias e integración de la API

---

## API (resumen)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/cuotas/plazos` | Lista de plazos |
| POST | `/api/cuotas/calcular` | Cuerpo JSON: `fechaNacimiento`, `monto`, `meses`. La API delega en **`dbo.usp_Cuotas_Calcular`**. Si `exito` es verdadero: `cuota`, `tasaAplicada`, `tablaAmortizacion` (filas con `numeroCuota`, `saldoInicial`, `cuota`, `interes`, `capital`, `saldoFinal`), `informacionSucursal`. |

### Tabla de amortización (lógica)

La **cuota** sigue siendo **(Monto × Tasa) ÷ meses** (tasa según edad en base de datos). La tabla reparte el **capital** en cuotas casi iguales (el último mes ajusta centavos) y muestra el **interés** de cada mes como la parte de la cuota fija que no es capital, de modo que la suma de capital amortizado coincide con el monto y el saldo final queda en cero. Es una **referencia** coherente con la cuota calculada, no un sistema de tasa compuesta distinto.
