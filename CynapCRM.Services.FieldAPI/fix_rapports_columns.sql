-- This script marks already-applied migrations in __EFMigrationsHistory
-- so that EF Core does not try to re-create existing tables.
-- Then it adds the 3 missing columns directly.

-- Step 1: Mark existing migrations as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260327102129_InitialField', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260327102129_InitialField');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260419134612_FieldAPI', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260419134612_FieldAPI');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260421192023_AddValeurRealiseeToObjectif', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260421192023_AddValeurRealiseeToObjectif');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260422140059_ChangeTypeObjectifToEnum', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260422140059_ChangeTypeObjectifToEnum');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260518000739_NVField', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260518000739_NVField');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260518021251_PeriodaAndTypeObjectif', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260518021251_PeriodaAndTypeObjectif');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260518022822_AddLatLongToRapports', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260518022822_AddLatLongToRapports');

-- Step 2: Add the 3 missing columns to the Rapports table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'Latitude'
)
BEGIN
    ALTER TABLE [Rapports] ADD [Latitude] float NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'Longitude'
)
BEGIN
    ALTER TABLE [Rapports] ADD [Longitude] float NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'IdSuperviseurValidateur'
)
BEGIN
    ALTER TABLE [Rapports] ADD [IdSuperviseurValidateur] int NULL;
END;

-- Step 3: Mark the fix migration as applied too
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT '20260522150900_FixAddLatLongToRapports', '9.0.1'
WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260522150900_FixAddLatLongToRapports');

-- Step 4: Verify
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId];

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Rapports'
ORDER BY ORDINAL_POSITION;
