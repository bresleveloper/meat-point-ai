-- Fix Daily Usage Count Issue
-- Reset all users' daily usage count to 0 and set reset date to today
-- This will give all existing users their full daily recipe allowance

UPDATE Users 
SET DailyUsageCount = 0, 
    LastUsageReset = CAST(GETDATE() AS DATE)
WHERE DailyUsageCount > 0;

-- Verify the fix
SELECT 
    UserID, 
    Email, 
    PlanStatus,
    DailyUsageCount, 
    LastUsageReset,
    CASE 
        WHEN PlanStatus = 'Premium' THEN 30 - DailyUsageCount 
        ELSE 3 - DailyUsageCount 
    END as RemainingRecipes
FROM Users 
WHERE IsActive = 1;