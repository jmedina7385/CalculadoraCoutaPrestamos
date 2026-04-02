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
