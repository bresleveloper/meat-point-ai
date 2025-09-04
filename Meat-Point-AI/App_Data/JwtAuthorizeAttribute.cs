using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Meat_Point_AI.App_Data;

namespace Meat_Point_AI.App_Data
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                // Get the Authorization header
                var authHeader = HttpContext.Current?.Request.Headers["Authorization"];
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    HandleUnauthorized(actionContext, "Authorization token is required");
                    return;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                // Validate the token
                var principal = JwtService.ValidateToken(token);
                if (principal == null)
                {
                    HandleUnauthorized(actionContext, "Invalid or expired token");
                    return;
                }

                // Set the current principal
                HttpContext.Current.User = principal;
                
                // Store user ID in HttpContext for easy access
                var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    HttpContext.Current.Items["UserId"] = userId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"JWT Authorization error: {ex.Message}");
                HandleUnauthorized(actionContext, "Authentication failed");
            }
        }

        private void HandleUnauthorized(HttpActionContext actionContext, string message)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                Success = false,
                Message = message
            });
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            HandleUnauthorized(actionContext, "Access denied");
        }
    }

    // Helper class to easily get current user ID in controllers
    public static class JwtHelper
    {
        public static int? GetCurrentUserId()
        {
            return HttpContext.Current?.Items["UserId"] as int?;
        }

        public static int? GetUserIdFromAuthHeader()
        {
            return JwtService.GetUserIdFromRequest();
        }
    }
}