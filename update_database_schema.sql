-- Script to add missing columns to datarunefaccountweb table
USE DataJonah;
GO

-- Add IsDeleted column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'datarunefaccountweb' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[datarunefaccountweb] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to datarunefaccountweb table.';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in datarunefaccountweb table.';
END
GO

-- Add DeletedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'datarunefaccountweb' AND COLUMN_NAME = 'DeletedAt')
BEGIN
    ALTER TABLE [dbo].[datarunefaccountweb] ADD [DeletedAt] datetime2(7) NULL;
    PRINT 'Added DeletedAt column to datarunefaccountweb table.';
END
ELSE
BEGIN
    PRINT 'DeletedAt column already exists in datarunefaccountweb table.';
END
GO

-- Update UpdatedAt column to allow NULL if it's currently NOT NULL
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'datarunefaccountweb' AND COLUMN_NAME = 'UpdatedAt' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE [dbo].[datarunefaccountweb] ALTER COLUMN [UpdatedAt] datetime2(7) NULL;
    PRINT 'Updated UpdatedAt column to allow NULL values.';
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already allows NULL values.';
END
GO

PRINT 'Database schema update completed.';