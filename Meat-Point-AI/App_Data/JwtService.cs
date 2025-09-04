using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.App_Data
{
    public static class JwtService
    {
        private static readonly string _jwtSecret = ConfigurationManager.AppSettings["JWT_Secret"];
        private static readonly string _jwtIssuer = ConfigurationManager.AppSettings["JWT_Issuer"];
        private static readonly string _jwtAudience = ConfigurationManager.AppSettings["JWT_Audience"];
        private static readonly int _jwtExpirationHours = int.Parse(ConfigurationManager.AppSettings["JWT_ExpirationHours"] ?? "24");

        private static readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        private static readonly SigningCredentials _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        public static string GenerateToken(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("plan_status", user.PlanStatus),
                new Claim("daily_usage", user.DailyUsageCount.ToString()),
                new Claim("is_active", user.IsActive.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtExpirationHours),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = _signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                Logger.Error($"JWT validation error: {ex.Message}");
                return null;
            }
        }

        public static int? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

        public static int? GetUserIdFromRequest(System.Net.Http.HttpRequestMessage request = null)
        {
            try
            {
                // For Web API 2, get from HttpContext
                var authHeader = HttpContext.Current?.Request.Headers["Authorization"];
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return null;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                return GetUserIdFromToken(token);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error extracting user ID from request: {ex.Message}");
                return null;
            }
        }

        public static Users GetUserFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return null;

            try
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = principal.FindFirst(ClaimTypes.Email);
                var planStatusClaim = principal.FindFirst("plan_status");
                var dailyUsageClaim = principal.FindFirst("daily_usage");
                var isActiveClaim = principal.FindFirst("is_active");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return null;

                return new Users
                {
                    UserID = userId,
                    Email = emailClaim?.Value,
                    PlanStatus = planStatusClaim?.Value ?? "Free",
                    DailyUsageCount = int.TryParse(dailyUsageClaim?.Value, out var usage) ? usage : 0,
                    IsActive = bool.TryParse(isActiveClaim?.Value, out var active) && active
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error extracting user from token: {ex.Message}");
                return null;
            }
        }
    }
}