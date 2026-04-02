-- Ejecutar conectado a master (por defecto en SSMS al abrir una nueva consultación).
IF DB_ID(N'CalculadoraPrestamos') IS NULL
BEGIN
    CREATE DATABASE CalculadoraPrestamos;
END
GO
