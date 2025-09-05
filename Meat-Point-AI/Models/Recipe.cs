using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meat_Point_AI.Models
{
    public class Recipes
    {
        public int RecipeID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ComplexityLevel { get; set; } // 1-5
        public int NumberOfDiners { get; set; }
        public string DinerAges { get; set; } // "Adult,Child,Teen" or specific ages
        public string CookingMethod { get; set; } // "Grilling", "Roasting", etc.
        public int CookingTimeMinutes { get; set; }
        public string DietaryRestrictions { get; set; } // "Low-carb,Gluten-free"
        public string Ingredients { get; set; } // JSON string of ingredients
        public string Instructions { get; set; } // Step-by-step cooking instructions
        public string TemperatureGuide { get; set; }
        public string ShoppingList { get; set; } // JSON string of shopping items
        public string UserPrompt { get; set; } // Optional AI prompt from user
        public DateTime CreatedDate { get; set; }
        public bool IsFavorite { get; set; }
        public double? Rating { get; set; }
        public string Notes { get; set; }
    }
}