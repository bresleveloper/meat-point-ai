using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.App_Data
{
    public class PaymentService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string STRIPE_SECRET_KEY = System.Configuration.ConfigurationManager.AppSettings["Stripe_Secret_Key"];
        private static readonly string STRIPE_PUBLISHABLE_KEY = System.Configuration.ConfigurationManager.AppSettings["Stripe_Publishable_Key"];
        private static readonly string STRIPE_API_URL = "https://api.stripe.com/v1";

        static PaymentService()
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {STRIPE_SECRET_KEY}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BeefMealPlanner/1.0");
        }

        public class PaymentMethodRequest
        {
            public string PaymentMethodId { get; set; }
        }

        public class SubscriptionResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string SubscriptionId { get; set; }
            public DateTime? NextBillingDate { get; set; }
        }

        public class CreatePaymentIntentRequest
        {
            public int Amount { get; set; } // Amount in cents
            public string Currency { get; set; } = "usd";
            public string PaymentMethodId { get; set; }
        }

        public static async Task<SubscriptionResponse> CreateSubscriptionAsync(int userId, string paymentMethodId)
        {
            try
            {
                // Get user information
                var user = DAL.select<Users>($"SELECT * FROM Users WHERE UserID = {userId}").FirstOrDefault();
                if (user == null)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Check if user already has premium
                if (user.PlanStatus == "Premium" && user.RenewalDate.HasValue && user.RenewalDate > DateTime.Now)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User already has an active premium subscription"
                    };
                }

                // Create or retrieve Stripe customer
                string customerId = await GetOrCreateStripeCustomerAsync(user);
                if (string.IsNullOrEmpty(customerId))
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Failed to create customer in payment system"
                    };
                }

                // Attach payment method to customer
                bool paymentMethodAttached = await AttachPaymentMethodAsync(paymentMethodId, customerId);
                if (!paymentMethodAttached)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Failed to attach payment method"
                    };
                }

                // Create subscription (for now, we'll simulate this since we don't have actual Stripe products set up)
                // In a real implementation, you'd create a Stripe subscription with a price ID
                
                // For demo purposes, we'll just process a one-time payment and update user status
                bool paymentProcessed = await ProcessOneTimePaymentAsync(1000, paymentMethodId, customerId); // $10.00
                
                if (paymentProcessed)
                {
                    // Update user subscription status
                    DateTime renewalDate = DateTime.Now.AddMonths(1);
                    
                    int updateResult = DAL.update("UPDATE Users SET PlanStatus = 'Premium', PremiumStartDate = @StartDate, RenewalDate = @RenewalDate, PaymentMethodId = @PaymentMethodId WHERE UserID = @UserID",
                        new System.Data.SqlClient.SqlParameter[]
                        {
                            new System.Data.SqlClient.SqlParameter("@StartDate", DateTime.Now),
                            new System.Data.SqlClient.SqlParameter("@RenewalDate", renewalDate),
                            new System.Data.SqlClient.SqlParameter("@PaymentMethodId", paymentMethodId),
                            new System.Data.SqlClient.SqlParameter("@UserID", userId)
                        });

                    if (updateResult > 0)
                    {
                        return new SubscriptionResponse
                        {
                            Success = true,
                            Message = "Subscription created successfully!",
                            SubscriptionId = $"sub_sim_{userId}_{DateTime.Now.Ticks}",
                            NextBillingDate = renewalDate
                        };
                    }
                    else
                    {
                        return new SubscriptionResponse
                        {
                            Success = false,
                            Message = "Payment processed but failed to update user subscription status"
                        };
                    }
                }
                else
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Payment failed. Please check your payment method."
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Create subscription error: " + ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the subscription"
                };
            }
        }

        public static async Task<string> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
        {
            try
            {
                var requestBody = new Dictionary<string, string>
                {
                    {"amount", request.Amount.ToString()},
                    {"currency", request.Currency},
                    {"payment_method", request.PaymentMethodId},
                    {"confirmation_method", "manual"},
                    {"confirm", "true"}
                };

                var encodedContent = new FormUrlEncodedContent(requestBody);
                var response = await httpClient.PostAsync($"{STRIPE_API_URL}/payment_intents", encodedContent);
                
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
                    return responseObj.id;
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Stripe PaymentIntent error: {response.StatusCode} - {errorBody}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Create PaymentIntent error: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> GetOrCreateStripeCustomerAsync(Users user)
        {
            try
            {
                // For demo purposes, we'll create a simple customer ID
                // In a real implementation, you'd call Stripe's customer API
                return $"cus_sim_{user.UserID}_{user.Email.GetHashCode()}";
            }
            catch (Exception ex)
            {
                Logger.Error($"Get or create customer error: {ex.Message}");
                return null;
            }
        }

        private static async Task<bool> AttachPaymentMethodAsync(string paymentMethodId, string customerId)
        {
            try
            {
                // For demo purposes, we'll assume this succeeds
                // In a real implementation, you'd call Stripe's attach payment method API
                await Task.Delay(100); // Simulate API call
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Attach payment method error: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> ProcessOneTimePaymentAsync(int amountCents, string paymentMethodId, string customerId)
        {
            try
            {
                // For demo purposes, we'll simulate payment processing
                // In a real implementation, you'd create and confirm a PaymentIntent
                await Task.Delay(200); // Simulate payment processing
                
                // Simulate payment success (90% success rate for demo)
                Random random = new Random();
                return random.NextDouble() > 0.1;
            }
            catch (Exception ex)
            {
                Logger.Error($"Process payment error: {ex.Message}");
                return false;
            }
        }

        public static SubscriptionResponse CancelSubscription(int userId)
        {
            try
            {
                var user = DAL.select<Users>($"SELECT * FROM Users WHERE UserID = {userId}").FirstOrDefault();
                if (user == null)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.PlanStatus != "Premium")
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "User does not have an active premium subscription"
                    };
                }

                // Update user to free plan
                int updateResult = DAL.update("UPDATE Users SET PlanStatus = 'Free', RenewalDate = NULL, PaymentMethodId = NULL WHERE UserID = @UserID",
                    new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@UserID", userId)
                    });

                if (updateResult > 0)
                {
                    return new SubscriptionResponse
                    {
                        Success = true,
                        Message = "Subscription cancelled successfully"
                    };
                }
                else
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Failed to cancel subscription"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Cancel subscription error: " + ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = "An error occurred while cancelling the subscription"
                };
            }
        }

        public static string GetStripePublishableKey()
        {
            return STRIPE_PUBLISHABLE_KEY;
        }
    }
}