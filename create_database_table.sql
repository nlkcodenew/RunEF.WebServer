
-- Script to create datarunefaccountweb table in DataJonah database
USE DataJonah;
GO

-- Create the table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='datarunefaccountweb' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[datarunefaccountweb] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(50) NULL,
        [LastName] nvarchar(50) NULL,
        [Role] nvarchar(20) NOT NULL DEFAULT 'User',
        [Changedate] datetime2(7) NULL,
        [OldPassword] nvarchar(max) NULL,
        [GroupControl] nvarchar(max) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [LastLoginAt] datetime2(7) NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiryTime] datetime2(7) NULL,
        [ComputerCode] nvarchar(max) NULL,
        [IpAddress] nvarchar(45) NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2(7) NULL,
        CONSTRAINT [PK_datarunefaccountweb] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [IX_datarunefaccountweb_Username] UNIQUE NONCLUSTERED ([Username] ASC)
    );
    
    PRINT 'Table datarunefaccountweb created successfully.';
END
ELSE
BEGIN
    PRINT 'Table datarunefaccountweb already exists.';
END
GO

-- Insert admin account if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[datarunefaccountweb] WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[datarunefaccountweb] 
    (Username, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('admin', '$2a$11$qH7Kz1XfE8rJgK5Zm.xO4uCvj8/Jm0yNZzMcW.VfH8qL3pO9uN8Ju', 'System', 'Administrator', 'Admin', 1, GETUTCDATE(), GETUTCDATE());
    
    PRINT 'Admin account created with username: admin, password: admin';
END
ELSE
BEGIN
    PRINT 'Admin account already exists.';
END
GO
