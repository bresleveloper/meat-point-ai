-- Beef Meal Planning Database Schema

-- Users table
CREATE TABLE Users (
    UserID int IDENTITY(1,1) PRIMARY KEY,
    Email nvarchar(255) NOT NULL UNIQUE,
    PasswordHash nvarchar(500) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    PlanStatus nvarchar(50) NOT NULL DEFAULT 'Free',
    PremiumStartDate datetime2 NULL,
    RenewalDate datetime2 NULL,
    PaymentMethodId nvarchar(255) NULL,
    DailyUsageCount int NOT NULL DEFAULT 0,
    LastUsageReset datetime2 NOT NULL DEFAULT GETDATE(),
    FirstName nvarchar(100) NULL,
    LastName nvarchar(100) NULL,
    IsActive bit NOT NULL DEFAULT 1
);

-- BeefCuts table
CREATE TABLE BeefCuts (
    BeefCutID int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(1000) NULL,
    CowBodyLocation nvarchar(100) NULL,
    Tenderness nvarchar(50) NULL,
    MarblingLevel nvarchar(50) NULL,
    BestCookingMethods nvarchar(500) NULL,
    ComplexityLevel int NOT NULL,
    ImageUrl nvarchar(500) NULL,
    CookingTips nvarchar(2000) NULL,
    TemperatureGuidelines nvarchar(1000) NULL,
    IsActive bit NOT NULL DEFAULT 1
);

-- BeefCuts table
CREATE TABLE BeefCutsHebrew (
    BeefCutID int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(1000) NULL,
    CowBodyLocation nvarchar(100) NULL,
    Tenderness nvarchar(50) NULL,
    MarblingLevel nvarchar(50) NULL,
    BestCookingMethods nvarchar(500) NULL,
    ComplexityLevel int NOT NULL,
    ImageUrl nvarchar(500) NULL,
    CookingTips nvarchar(2000) NULL,
    TemperatureGuidelines nvarchar(1000) NULL,
    IsActive bit NOT NULL DEFAULT 1
);

-- Recipes table
CREATE TABLE Recipes (
    RecipeID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    BeefCutID int NOT NULL,
    Title nvarchar(200) NOT NULL,
    Description nvarchar(1000) NULL,
    ComplexityLevel int NOT NULL,
    NumberOfDiners int NOT NULL,
    DinerAges nvarchar(500) NULL,
    CookingMethod nvarchar(100) NULL,
    CookingTimeMinutes int NOT NULL,
    DietaryRestrictions nvarchar(500) NULL,
    Ingredients nvarchar(max) NULL,
    Instructions nvarchar(max) NULL,
    TemperatureGuide nvarchar(1000) NULL,
    ShoppingList nvarchar(max) NULL,
    UserPrompt nvarchar(2000) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    IsFavorite bit NOT NULL DEFAULT 0,
    Rating decimal(3,1) NULL,
    Notes nvarchar(2000) NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (BeefCutID) REFERENCES BeefCuts(BeefCutID)
);

-- MealPlans table
CREATE TABLE MealPlans (
    MealPlanID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    PlanName nvarchar(200) NOT NULL,
    PlanDate datetime2 NOT NULL,
    RecipeIDs nvarchar(max) NULL,
    TotalDiners int NOT NULL,
    DinerAges nvarchar(500) NULL,
    EstimatedCost decimal(10,2) NULL,
    CombinedShoppingList nvarchar(max) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    IsCompleted bit NOT NULL DEFAULT 0,
    Notes nvarchar(2000) NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- RecipeRatings table
CREATE TABLE RecipeRatings (
    RecipeRatingID int IDENTITY(1,1) PRIMARY KEY,
    RecipeID int NOT NULL,
    UserID int NOT NULL,
    Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Review nvarchar(2000) NULL,
    RatingDate datetime2 NOT NULL DEFAULT GETDATE(),
    WouldMakeAgain bit NOT NULL DEFAULT 0,
    CookingNotes nvarchar(2000) NULL,
    ActualCookingTimeMinutes int NULL,
    DifficultyFeedback nvarchar(100) NULL,
    FOREIGN KEY (RecipeID) REFERENCES Recipes(RecipeID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Create indexes for better performance
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Recipes_UserID ON Recipes(UserID);
CREATE INDEX IX_Recipes_BeefCutID ON Recipes(BeefCutID);
CREATE INDEX IX_Recipes_ComplexityLevel ON Recipes(ComplexityLevel);
CREATE INDEX IX_MealPlans_UserID ON MealPlans(UserID);
CREATE INDEX IX_RecipeRatings_RecipeID ON RecipeRatings(RecipeID);