-- Script to insert test data into RunEFClients table
USE DataJonah;
GO

-- Insert test clients
INSERT INTO [RunEFClients] (
    [Id],
    [ComputerCode],
    [IpAddress],
    [ComputerName],
    [Username],
    [IsActive],
    [IsOnline],
    [LastSeen],
    [LastHeartbeat],
    [Version],
    [Status],
    [Notes],
    [IsBlocked],
    [BlockedAt],
    [BlockedReason],
    [BlockedBy],
    [CreatedAt],
    [UpdatedAt],
    [IsDeleted],
    [DeletedAt]
) VALUES 
(
    NEWID(),
    'PC001',
    '192.168.1.100',
    'DESKTOP-001',
    'user1',
    1,
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    '1.0.0',
    'Active',
    'Test client 1',
    0,
    NULL,
    NULL,
    NULL,
    GETUTCDATE(),
    NULL,
    0,
    NULL
),
(
    NEWID(),
    'PC002',
    '192.168.1.101',
    'DESKTOP-002',
    'user2',
    1,
    0,
    DATEADD(MINUTE, -30, GETUTCDATE()),
    DATEADD(MINUTE, -30, GETUTCDATE()),
    '1.0.0',
    'Offline',
    'Test client 2',
    0,
    NULL,
    NULL,
    NULL,
    GETUTCDATE(),
    NULL,
    0,
    NULL
),
(
    NEWID(),
    'PC003',
    '192.168.1.102',
    'DESKTOP-003',
    'user3',
    1,
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    '1.0.0',
    'Active',
    'Test client 3',
    1,
    GETUTCDATE(),
    'Security violation',
    'admin',
    GETUTCDATE(),
    NULL,
    0,
    NULL
);

PRINT 'Test clients inserted successfully.';
GO

-- Check inserted data
SELECT 
    ComputerCode,
    IpAddress,
    ComputerName,
    IsActive,
    IsOnline,
    IsBlocked,
    LastSeen,
    CreatedAt
FROM [RunEFClients]
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
GO