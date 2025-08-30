-- Tạo bảng datarunefRunEFClients
USE DataJonah;

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
    
    PRINT 'Bảng datarunefRunEFClients đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng datarunefRunEFClients đã tồn tại!';
END
