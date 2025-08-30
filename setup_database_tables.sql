-- Script tổng hợp để tạo các bảng cần thiết cho RunEF WebServer
-- Chạy trên SQL Server 172.19.111.246, Database DataJonah

USE DataJonah;
GO

PRINT '=== BẮT ĐẦU TẠO CÁC BẢNG CHO RUNEF WEBSERVER ===';

-- 1. Tạo bảng datarunefApplicationLogs
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
    PRINT '✓ Bảng datarunefApplicationLogs đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '✓ Bảng datarunefApplicationLogs đã tồn tại!';
END

-- 2. Tạo bảng datarunefRunEFClients
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='datarunefRunEFClients' AND xtype='U')
BEGIN
    CREATE TABLE datarunefRunEFClients (
        Id uniqueidentifier NOT NULL DEFAULT NEWID(),
        ComputerCode nvarchar(100) NOT NULL,
        IpAddress nvarchar(45) NOT NULL,
        ComputerName nvarchar(max) NULL,
        Username nvarchar(max) NULL,
        IsActive bit NOT NULL DEFAULT 1,
        IsOnline bit NOT NULL DEFAULT 0,
        LastSeen datetime2 NULL,
        LastHeartbeat datetime2 NULL,
        Version nvarchar(max) NULL,
        Status nvarchar(max) NULL,
        Notes nvarchar(max) NULL,
        IsBlocked bit NOT NULL DEFAULT 0,
        BlockedAt datetime2 NULL,
        BlockedReason nvarchar(max) NULL,
        BlockedBy nvarchar(max) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2 NULL,
        CONSTRAINT PK_datarunefRunEFClients PRIMARY KEY (Id)
    );
    
    -- Tạo index cho ComputerCode
    CREATE UNIQUE INDEX IX_datarunefRunEFClients_ComputerCode ON datarunefRunEFClients (ComputerCode);
    PRINT '✓ Bảng datarunefRunEFClients đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '✓ Bảng datarunefRunEFClients đã tồn tại!';
END

-- 3. Kiểm tra bảng datarunefaccountweb
IF EXISTS (SELECT * FROM sysobjects WHERE name='datarunefaccountweb' AND xtype='U')
BEGIN
    PRINT '✓ Bảng datarunefaccountweb đã tồn tại!';
END
ELSE
BEGIN
    PRINT '⚠ Bảng datarunefaccountweb chưa tồn tại! Cần tạo bảng này.';
END

PRINT '=== HOÀN THÀNH TẠO CÁC BẢNG ===';
PRINT 'Các bảng đã sẵn sàng cho RunEF WebServer API!';
