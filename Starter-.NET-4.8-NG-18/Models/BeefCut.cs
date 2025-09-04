using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Starter_.NET_4._8_NG_18.Models
{
    public class BeefCuts
    {
        public int BeefCutID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CowBodyLocation { get; set; }
        public string Tenderness { get; set; } // "Very Tender", "Tender", "Moderate", "Tough"
        public string MarblingLevel { get; set; } // "High", "Medium", "Low"
        public string BestCookingMethods { get; set; } // "Grilling,Roasting,Pan-frying"
        public int ComplexityLevel { get; set; } // 1-5 (1 = stupid dad, 5 = super chef mom)
        public string ImageUrl { get; set; }
        public string CookingTips { get; set; }
        public string TemperatureGuidelines { get; set; }
        public bool IsActive { get; set; }
    }
}