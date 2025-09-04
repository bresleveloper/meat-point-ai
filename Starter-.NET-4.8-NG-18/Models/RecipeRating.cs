using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Starter_.NET_4._8_NG_18.Models
{
    public class RecipeRatings
    {
        public int RecipeRatingID { get; set; }
        public int RecipeID { get; set; }
        public int UserID { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public string Review { get; set; }
        public DateTime RatingDate { get; set; }
        public bool WouldMakeAgain { get; set; }
        public string CookingNotes { get; set; } // User's cooking experience notes
        public int ActualCookingTimeMinutes { get; set; }
        public string DifficultyFeedback { get; set; } // "Too Easy", "Just Right", "Too Hard"
    }
}