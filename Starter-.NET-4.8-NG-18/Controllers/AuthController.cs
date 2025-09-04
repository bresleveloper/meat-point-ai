using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using Starter_.NET_4._8_NG_18.App_Data;
using Starter_.NET_4._8_NG_18.Models;

namespace Starter_.NET_4._8_NG_18.Controllers
{
    public class AuthController : ApiController
    {
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class AuthResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Users User { get; set; }
            public string Token { get; set; }
        }

        [HttpPost]
        [Route("api/auth/login")]
        public AuthResponse Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = AuthService.AuthenticateUser(request.Email, request.Password);
                if (user != null)
                {
                    // Generate JWT token
                    var token = JwtService.GenerateToken(user);

                    // Don't return password hash
                    user.PasswordHash = null;
                    
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = user,
                        Token = token
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Login error: " + ex.Message);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        [HttpPost]
        [Route("api/auth/register")]
        public AuthResponse Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email and password are required"
                    };
                }

                if (request.Password.Length < 6)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Password must be at least 6 characters long"
                    };
                }

                var user = AuthService.RegisterUser(request.Email, request.Password, 
                                                  request.FirstName, request.LastName);
                
                if (user != null)
                {
                    // Generate JWT token
                    var token = JwtService.GenerateToken(user);
                    
                    // Don't return password hash
                    user.PasswordHash = null;
                    
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Registration successful",
                        User = user,
                        Token = token
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Registration failed. Email may already be in use."
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Registration error: " + ex.Message);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        [HttpPost]
        [Route("api/auth/logout")]
        public AuthResponse Logout()
        {
            // JWT tokens are stateless - logout is handled client-side by discarding the token
            return new AuthResponse
            {
                Success = true,
                Message = "Logout successful - please discard your token"
            };
        }

        [HttpGet]
        [Route("api/auth/check")]
        public AuthResponse CheckAuth()
        {
            try
            {
                var authHeader = HttpContext.Current?.Request.Headers["Authorization"];
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User not authenticated"
                    };
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var userId = JwtService.GetUserIdFromToken(token);
                
                if (userId.HasValue)
                {
                    var user = DAL.select<Users>("SELECT * FROM Users WHERE UserID = @UserID", 
                        new System.Data.SqlClient.SqlParameter[] 
                        {
                            new System.Data.SqlClient.SqlParameter("@UserID", userId.Value)
                        }).FirstOrDefault();

                    if (user != null)
                    {
                        user.PasswordHash = null; // Don't return password hash
                        return new AuthResponse
                        {
                            Success = true,
                            Message = "User is authenticated",
                            User = user,
                            Token = token // Return the same token
                        };
                    }
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "User not authenticated"
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Auth check error: " + ex.Message);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred checking authentication"
                };
            }
        }
    }
}