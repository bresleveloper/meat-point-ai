using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.SessionState;
using Starter_.NET_4._8_NG_18.App_Data;
using static Starter_.NET_4._8_NG_18.App_Data.AIChefService;

namespace Starter_.NET_4._8_NG_18.Controllers
{
    public class AIController : ApiController
    {
        [System.Web.Http.HttpPost]
        [JwtAuthorize]
        public async Task<RecipeGenerationResponse> GenerateRecipe([FromBody] RecipeGenerationRequest request)
        {
            try
            {
                // Get user ID from JWT token
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Authentication required"
                    };
                }

                // Validate request
                if (request == null)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    };
                }

                if (request.BeefCutID <= 0 || request.ComplexityLevel < 1 || request.ComplexityLevel > 5 || 
                    request.NumberOfDiners <= 0 || request.CookingTimeMinutes <= 0)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Please fill in all required fields with valid values"
                    };
                }

                // Generate the recipe
                var response = await AIChefService.GenerateRecipeAsync(request, userId.Value);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("AI Controller error: " + ex.Message);
                return new RecipeGenerationResponse
                {
                    Success = false,
                    Message = "An error occurred while generating the recipe"
                };
            }
        }

        [System.Web.Http.HttpGet]
        [JwtAuthorize]
        public object GetUserUsage()
        {
            try
            {
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new { Success = false, Message = "Not authenticated" };
                }

                var user = DAL.select<Models.Users>($"SELECT * FROM Users WHERE UserID = {userId.Value}").FirstOrDefault();
                if (user == null)
                {
                    return new { Success = false, Message = "User not found" };
                }

                // Reset usage count if needed
                if (user.LastUsageReset.Date < DateTime.Today)
                {
                    DAL.update("UPDATE Users SET DailyUsageCount = 0, LastUsageReset = @Today WHERE UserID = @UserID",
                        new System.Data.SqlClient.SqlParameter[]
                        {
                            new System.Data.SqlClient.SqlParameter("@Today", DateTime.Today),
                            new System.Data.SqlClient.SqlParameter("@UserID", userId.Value)
                        });
                    user.DailyUsageCount = 0;
                }

                int maxRecipes = user.PlanStatus == "Premium" ? 30 : 3;
                int remainingRecipes = Math.Max(0, maxRecipes - user.DailyUsageCount);

                return new
                {
                    Success = true,
                    PlanStatus = user.PlanStatus,
                    DailyUsageCount = user.DailyUsageCount,
                    MaxDailyRecipes = maxRecipes,
                    RemainingRecipes = remainingRecipes,
                    CanGenerateMore = remainingRecipes > 0
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Get user usage error: " + ex.Message);
                return new { Success = false, Message = "Error retrieving usage information" };
            }
        }
    }
}