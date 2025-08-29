-- Script to check RunEFClients table data
USE DataJonah;
GO

-- Check if RunEFClients table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RunEFClients')
BEGIN
    PRINT 'RunEFClients table exists';
    
    -- Count total records
    SELECT COUNT(*) as TotalRecords FROM RunEFClients;
    
    -- Count non-deleted records
    SELECT COUNT(*) as NonDeletedRecords FROM RunEFClients WHERE IsDeleted = 0;
    
    -- Show all records
    SELECT 
        Id,
        ComputerCode,
        IpAddress,
        ComputerName,
        IsActive,
        IsOnline,
        IsBlocked,
        IsDeleted,
        LastSeen,
        LastHeartbeat,
        CreatedAt
    FROM RunEFClients
    ORDER BY CreatedAt DESC;
END
ELSE
BEGIN
    PRINT 'RunEFClients table does not exist';
END
GO

-- Also check if the table name might be different
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Client%' OR TABLE_NAME LIKE '%RunEF%';
GO