/*
  Script base para el sink SQL de Serilog.
  Ejecutar una sola vez en la base de datos destino antes de habilitar escritura en SQL.
*/

IF OBJECT_ID('dbo.ApplicationLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationLogs
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationLogs PRIMARY KEY,
        Message NVARCHAR(MAX) NULL,
        MessageTemplate NVARCHAR(MAX) NULL,
        Level NVARCHAR(128) NULL,
        TimeStamp DATETIMEOFFSET NOT NULL,
        Exception NVARCHAR(MAX) NULL,
        Properties NVARCHAR(MAX) NULL,
        LogEvent NVARCHAR(MAX) NULL
    );

    CREATE INDEX IX_ApplicationLogs_TimeStamp ON dbo.ApplicationLogs(TimeStamp DESC);
    CREATE INDEX IX_ApplicationLogs_Level ON dbo.ApplicationLogs(Level);
END;
