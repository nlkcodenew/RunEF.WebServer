-- Tạo bảng datarunefApplicationLogs
USE DataJonah;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='datarunefApplicationLogs' AND xtype='U')
BEGIN
    CREATE TABLE datarunefApplicationLogs (
        Id uniqueidentifier NOT NULL DEFAULT NEWID(),
        Username nvarchar(100) NULL,
        Action nvarchar(100) NOT NULL,
        Details nvarchar(max) NULL,
        IpAddress nvarchar(45) NULL,
        ComputerCode nvarchar(100) NULL,
        LogTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
        Result nvarchar(max) NULL,
        LogType nvarchar(max) NULL,
        Message nvarchar(max) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2 NULL,
        CONSTRAINT PK_datarunefApplicationLogs PRIMARY KEY (Id)
    );
    
    PRINT 'Bảng datarunefApplicationLogs đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng datarunefApplicationLogs đã tồn tại!';
END
