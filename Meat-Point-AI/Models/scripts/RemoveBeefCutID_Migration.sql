-- Migration script to remove BeefCutID from Recipes table
-- This script removes all references to BeefCutID from the Recipes table

USE BeefMaster;

-- Step 1: Drop the foreign key constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Recipes__BeefCut__440B1D61')
BEGIN
    ALTER TABLE Recipes DROP CONSTRAINT FK__Recipes__BeefCut__440B1D61;
    PRINT 'Foreign key constraint FK__Recipes__BeefCut__440B1D61 dropped successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK__Recipes__BeefCut__440B1D61 does not exist';
END

-- Step 2: Drop the index on BeefCutID if it exists
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Recipes_BeefCutID' AND object_id = OBJECT_ID('Recipes'))
BEGIN
    DROP INDEX IX_Recipes_BeefCutID ON Recipes;
    PRINT 'Index IX_Recipes_BeefCutID dropped successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_Recipes_BeefCutID does not exist';
END

-- Step 3: Drop the BeefCutID column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Recipes') AND name = 'BeefCutID')
BEGIN
    ALTER TABLE Recipes DROP COLUMN BeefCutID;
    PRINT 'BeefCutID column dropped successfully from Recipes table';
END
ELSE
BEGIN
    PRINT 'BeefCutID column does not exist in Recipes table';
END

PRINT 'Migration completed: BeefCutID completely removed from Recipes table';