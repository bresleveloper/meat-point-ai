using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meat_Point_AI.Models
{
    public class MealPlans
    {
        public int MealPlanID { get; set; }
        public int UserID { get; set; }
        public string PlanName { get; set; }
        public DateTime PlanDate { get; set; }
        public string RecipeIDs { get; set; } // JSON array of recipe IDs
        public int TotalDiners { get; set; }
        public string DinerAges { get; set; } // JSON array of ages
        public double EstimatedCost { get; set; }
        public string CombinedShoppingList { get; set; } // JSON combined from all recipes
        public DateTime CreatedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; }
    }
}