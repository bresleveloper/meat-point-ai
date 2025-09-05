
UPDATE BeefCuts2
SET Tenderness = CASE
    WHEN Tenderness = 'Medium' THEN 'Moderate'
    WHEN Tenderness = 'Medium-Firm' THEN 'Tough'
    WHEN Tenderness = 'Tough (needs long cooking)' THEN 'Tough'
    WHEN Tenderness = 'Tough-Medium' THEN 'Tough'
    WHEN Tenderness = 'Tender-Medium' THEN 'Tender'
    WHEN Tenderness = 'Medium-Tender' THEN 'Tender'
    -- save wanted values, otherwise it sets to null
    WHEN Tenderness = 'Tender' THEN 'Tender'
    WHEN Tenderness = 'Tough' THEN 'Tough'
    WHEN Tenderness = 'Very Tender' THEN 'Very Tender'
END;







     USE bresleveloper_BeefMaster;
     -- Step 1: Drop the foreign key constraint
     IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Recipes__BeefCut__440B1D61')
     BEGIN
         ALTER TABLE Recipes DROP CONSTRAINT FK__Recipes__BeefCut__440B1D61;
         PRINT 'Foreign key constraint FK__Recipes__BeefCut__440B1D61 dropped successfully'
        end;