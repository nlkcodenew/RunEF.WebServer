-- Create RunEFClients table
CREATE TABLE [RunEFClients] (
    [Id] uniqueidentifier NOT NULL,
    [ComputerCode] nvarchar(100) NOT NULL,
    [IpAddress] nvarchar(45) NULL,
    [ComputerName] nvarchar(max) NULL,
    [Username] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsOnline] bit NOT NULL,
    [LastSeen] datetime2 NULL,
    [LastHeartbeat] datetime2 NULL,
    [Version] nvarchar(max) NULL,
    [Status] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [IsBlocked] bit NOT NULL,
    [BlockedAt] datetime2 NULL,
    [BlockedReason] nvarchar(max) NULL,
    [BlockedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_RunEFClients] PRIMARY KEY ([Id])
);

-- Create unique index on ComputerCode
CREATE UNIQUE INDEX [IX_RunEFClients_ComputerCode] ON [RunEFClients] ([ComputerCode]);

GO