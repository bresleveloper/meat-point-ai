using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meat_Point_AI.Models
{
    public class Users
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PlanStatus { get; set; } // "Free", "Premium"
        public DateTime? PremiumStartDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string PaymentMethodId { get; set; }
        public int DailyUsageCount { get; set; }
        public DateTime LastUsageReset { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
    }
}