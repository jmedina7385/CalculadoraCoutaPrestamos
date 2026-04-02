USE CalculadoraPrestamos;
GO

IF OBJECT_ID(N'dbo.LogConsultas', N'U') IS NOT NULL DROP TABLE dbo.LogConsultas;
IF OBJECT_ID(N'dbo.PlazosMeses', N'U') IS NOT NULL DROP TABLE dbo.PlazosMeses;
IF OBJECT_ID(N'dbo.TasasPorEdad', N'U') IS NOT NULL DROP TABLE dbo.TasasPorEdad;
GO

CREATE TABLE dbo.TasasPorEdad (
    Edad INT NOT NULL PRIMARY KEY,
    Tasa DECIMAL(5, 2) NOT NULL
);

CREATE TABLE dbo.PlazosMeses (
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Descripcion NVARCHAR(50) NOT NULL,
    Valor INT NOT NULL UNIQUE
);

CREATE TABLE dbo.LogConsultas (
    IdConsulta INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    FechaConsulta DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Edad INT NOT NULL,
    Monto DECIMAL(18, 2) NOT NULL,
    Meses INT NOT NULL,
    ValorCuota DECIMAL(18, 2) NOT NULL,
    IP_Consulta NVARCHAR(45) NULL
);
GO

INSERT INTO dbo.TasasPorEdad (Edad, Tasa) VALUES
(18, 1.20), (19, 1.18), (20, 1.16), (21, 1.14), (22, 1.12), (23, 1.10), (24, 1.08), (25, 1.05);

INSERT INTO dbo.PlazosMeses (Descripcion, Valor) VALUES
(N'3 Meses', 3), (N'6 Meses', 6), (N'9 Meses', 9), (N'12 Meses', 12);
GO

IF OBJECT_ID(N'dbo.usp_Cuotas_Calcular', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cuotas_Calcular;
IF OBJECT_ID(N'dbo.usp_Tasas_ObtenerPorEdad', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Tasas_ObtenerPorEdad;
IF OBJECT_ID(N'dbo.usp_Plazos_Listar', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Plazos_Listar;
IF OBJECT_ID(N'dbo.usp_Plazos_ValidarValor', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Plazos_ValidarValor;
IF OBJECT_ID(N'dbo.usp_LogConsulta_Insertar', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_LogConsulta_Insertar;
GO

CREATE PROCEDURE dbo.usp_Tasas_ObtenerPorEdad
    @Edad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Tasa
    FROM dbo.TasasPorEdad
    WHERE Edad = @Edad;
END
GO

CREATE PROCEDURE dbo.usp_Plazos_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Descripcion, Valor
    FROM dbo.PlazosMeses
    ORDER BY Valor;
END
GO

CREATE PROCEDURE dbo.usp_Plazos_ValidarValor
    @Valor INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PlazosMeses WHERE Valor = @Valor)
        SELECT CONVERT(BIT, 1);
    ELSE
        SELECT CONVERT(BIT, 0);
END
GO

CREATE PROCEDURE dbo.usp_LogConsulta_Insertar
    @Edad INT,
    @Monto DECIMAL(18, 2),
    @Meses INT,
    @ValorCuota DECIMAL(18, 2),
    @IP_Consulta NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
    VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, @ValorCuota, @IP_Consulta);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdConsulta;
END
GO

/* Cálculo completo en base de datos: edad, validaciones, tasa, cuota, tabla de amortización y bitácora. */
CREATE PROCEDURE dbo.usp_Cuotas_Calcular
    @FechaNacimiento DATE,
    @FechaReferencia DATE,
    @Monto DECIMAL(18, 2),
    @Meses INT,
    @IP_Consulta NVARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Edad INT;
    DECLARE @EdadMinima INT = 18;
    DECLARE @EdadMaxima INT = 25;
    DECLARE @Cuota DECIMAL(18, 2);
    DECLARE @Tasa DECIMAL(5, 2);
    DECLARE @PlazoValido BIT;
    DECLARE @Saldo DECIMAL(18, 2);
    DECLARE @CapitalBase DECIMAL(18, 2);
    DECLARE @i INT;
    DECLARE @SaldoInicial DECIMAL(18, 2);
    DECLARE @Capital DECIMAL(18, 2);
    DECLARE @Interes DECIMAL(18, 2);

    SET @Edad = DATEDIFF(YEAR, @FechaNacimiento, @FechaReferencia);
    IF DATEADD(YEAR, @Edad, @FechaNacimiento) > @FechaReferencia
        SET @Edad = @Edad - 1;

    IF @Edad < @EdadMinima
    BEGIN
        INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
        VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, 0, @IP_Consulta);

        SELECT CONVERT(BIT, 0) AS Exito,
               N'Lo sentimos, aún no cuenta con la edad para solicitar este producto.' AS Mensaje,
               N'Edad mínima no alcanzada' AS Titulo,
               CAST(NULL AS DECIMAL(18, 2)) AS Cuota,
               CAST(NULL AS DECIMAL(5, 2)) AS TasaAplicada;
        RETURN;
    END;

    IF @Edad > @EdadMaxima
    BEGIN
        INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
        VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, 0, @IP_Consulta);

        SELECT CONVERT(BIT, 0) AS Exito,
               N'Favor pasar por una de nuestras sucursales para evaluar su caso.' AS Mensaje,
               N'Su caso debe evaluarse en sucursal' AS Titulo,
               CAST(NULL AS DECIMAL(18, 2)) AS Cuota,
               CAST(NULL AS DECIMAL(5, 2)) AS TasaAplicada;
        RETURN;
    END;

    IF @Monto <= 0
    BEGIN
        INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
        VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, 0, @IP_Consulta);

        SELECT CONVERT(BIT, 0) AS Exito,
               N'El monto del préstamo debe ser mayor que cero.' AS Mensaje,
               CAST(NULL AS NVARCHAR(200)) AS Titulo,
               CAST(NULL AS DECIMAL(18, 2)) AS Cuota,
               CAST(NULL AS DECIMAL(5, 2)) AS TasaAplicada;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.PlazosMeses WHERE Valor = @Meses)
        SET @PlazoValido = 1;
    ELSE
        SET @PlazoValido = 0;

    IF @PlazoValido = 0
    BEGIN
        INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
        VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, 0, @IP_Consulta);

        SELECT CONVERT(BIT, 0) AS Exito,
               N'La cantidad de meses no es válida.' AS Mensaje,
               CAST(NULL AS NVARCHAR(200)) AS Titulo,
               CAST(NULL AS DECIMAL(18, 2)) AS Cuota,
               CAST(NULL AS DECIMAL(5, 2)) AS TasaAplicada;
        RETURN;
    END;

    SELECT @Tasa = Tasa FROM dbo.TasasPorEdad WHERE Edad = @Edad;

    IF @Tasa IS NULL
    BEGIN
        INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
        VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, 0, @IP_Consulta);

        SELECT CONVERT(BIT, 0) AS Exito,
               N'No se encontró tasa para la edad indicada.' AS Mensaje,
               CAST(NULL AS NVARCHAR(200)) AS Titulo,
               CAST(NULL AS DECIMAL(18, 2)) AS Cuota,
               CAST(NULL AS DECIMAL(5, 2)) AS TasaAplicada;
        RETURN;
    END;

    SET @Cuota = ROUND(@Monto * @Tasa / @Meses, 2);

    DECLARE @Amort TABLE (
        NumeroCuota INT NOT NULL PRIMARY KEY,
        SaldoInicial DECIMAL(18, 2) NOT NULL,
        Cuota DECIMAL(18, 2) NOT NULL,
        Interes DECIMAL(18, 2) NOT NULL,
        Capital DECIMAL(18, 2) NOT NULL,
        SaldoFinal DECIMAL(18, 2) NOT NULL
    );

    SET @Saldo = @Monto;
    SET @CapitalBase = ROUND(@Monto / @Meses, 2);
    SET @i = 1;

    WHILE @i <= @Meses
    BEGIN
        SET @SaldoInicial = @Saldo;
        IF @i < @Meses
            SET @Capital = @CapitalBase;
        ELSE
            SET @Capital = @SaldoInicial;

        SET @Interes = @Cuota - @Capital;
        IF @Interes < 0
            SET @Interes = 0;

        SET @Saldo = @SaldoInicial - @Capital;
        IF @Saldo < 0
            SET @Saldo = 0;

        INSERT INTO @Amort (NumeroCuota, SaldoInicial, Cuota, Interes, Capital, SaldoFinal)
        VALUES (@i, @SaldoInicial, @Cuota, @Interes, @Capital, @Saldo);

        SET @i = @i + 1;
    END;

    INSERT INTO dbo.LogConsultas (FechaConsulta, Edad, Monto, Meses, ValorCuota, IP_Consulta)
    VALUES (SYSUTCDATETIME(), @Edad, @Monto, @Meses, @Cuota, @IP_Consulta);

    SELECT CONVERT(BIT, 1) AS Exito,
           CAST(NULL AS NVARCHAR(500)) AS Mensaje,
           CAST(NULL AS NVARCHAR(200)) AS Titulo,
           @Cuota AS Cuota,
           @Tasa AS TasaAplicada;

    SELECT NumeroCuota, SaldoInicial, Cuota, Interes, Capital, SaldoFinal
    FROM @Amort
    ORDER BY NumeroCuota;
END
GO
