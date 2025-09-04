using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using Starter_.NET_4._8_NG_18.App_Data;
using static Starter_.NET_4._8_NG_18.App_Data.PaymentService;

namespace Starter_.NET_4._8_NG_18.Controllers
{
    public class PaymentController : ApiController
    {
        [HttpPost]
        [JwtAuthorize]
        public async Task<SubscriptionResponse> Subscribe([FromBody] PaymentMethodRequest request)
        {
            try
            {
                // Get user ID from JWT token
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "You must be logged in to subscribe"
                    };
                }

                if (string.IsNullOrWhiteSpace(request?.PaymentMethodId))
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "Payment method is required"
                    };
                }

                var result = await PaymentService.CreateSubscriptionAsync((int)userId, request.PaymentMethodId);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Subscribe error: " + ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your subscription"
                };
            }
        }

        [HttpPost]
        [JwtAuthorize]
        public SubscriptionResponse CancelSubscription()
        {
            try
            {
                // Get user ID from JWT token
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new SubscriptionResponse
                    {
                        Success = false,
                        Message = "You must be logged in to cancel subscription"
                    };
                }

                var result = PaymentService.CancelSubscription((int)userId);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Cancel subscription error: " + ex.Message);
                return new SubscriptionResponse
                {
                    Success = false,
                    Message = "An error occurred while cancelling your subscription"
                };
            }
        }

        [HttpGet]
        public object GetStripeConfig()
        {
            try
            {
                return new
                {
                    Success = true,
                    PublishableKey = PaymentService.GetStripePublishableKey(),
                    PriceMonthly = 10.00,
                    Currency = "USD"
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Get Stripe config error: " + ex.Message);
                return new
                {
                    Success = false,
                    Message = "Failed to load payment configuration"
                };
            }
        }

        [HttpGet]
        [JwtAuthorize]
        public object GetSubscriptionStatus()
        {
            try
            {
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new { Success = false, Message = "Not authenticated" };
                }

                var user = DAL.select<Models.Users>($"SELECT * FROM Users WHERE UserID = {userId}").FirstOrDefault();
                if (user == null)
                {
                    return new { Success = false, Message = "User not found" };
                }

                return new
                {
                    Success = true,
                    PlanStatus = user.PlanStatus,
                    PremiumStartDate = user.PremiumStartDate,
                    RenewalDate = user.RenewalDate,
                    IsActive = user.PlanStatus == "Premium" && user.RenewalDate.HasValue && user.RenewalDate > DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Get subscription status error: " + ex.Message);
                return new { Success = false, Message = "Error retrieving subscription status" };
            }
        }
    }
}