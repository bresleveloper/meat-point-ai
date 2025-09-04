using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.App_Data
{
    public class AuthService
    {
        public static class PasswordHelper
        {
            public static string HashPassword(string password)
            {
                // Generate a random salt
                byte[] salt = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Hash the password with the salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    byte[] hash = pbkdf2.GetBytes(32);
                    
                    // Combine salt and hash
                    byte[] hashBytes = new byte[64];
                    Array.Copy(salt, 0, hashBytes, 0, 32);
                    Array.Copy(hash, 0, hashBytes, 32, 32);
                    
                    return Convert.ToBase64String(hashBytes);
                }
            }

            public static bool VerifyPassword(string password, string hash)
            {
                try
                {
                    byte[] hashBytes = Convert.FromBase64String(hash);
                    
                    // Extract the salt
                    byte[] salt = new byte[32];
                    Array.Copy(hashBytes, 0, salt, 0, 32);
                    
                    // Compute the hash of the provided password
                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
                    {
                        byte[] hash2 = pbkdf2.GetBytes(32);
                        
                        // Compare the hashes
                        for (int i = 0; i < 32; i++)
                        {
                            if (hashBytes[i + 32] != hash2[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public static Users AuthenticateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            try
            {
                var users = DAL.select<Users>("SELECT * FROM Users WHERE Email = @Email AND IsActive = 1", 
                    new System.Data.SqlClient.SqlParameter[] 
                    {
                        new System.Data.SqlClient.SqlParameter("@Email", email)
                    });

                var user = users.FirstOrDefault();
                if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
                {
                    return user;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Authentication error: " + ex.Message);
            }

            return null;
        }

        public static Users RegisterUser(string email, string password, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            if (!DAL.Validators.Email(email))
            {
                return null;
            }

            try
            {
                // Check if user already exists
                var existingUsers = DAL.select<Users>("SELECT * FROM Users WHERE Email = @Email", 
                    new System.Data.SqlClient.SqlParameter[] 
                    {
                        new System.Data.SqlClient.SqlParameter("@Email", email)
                    });

                if (existingUsers.Any())
                {
                    return null; // User already exists
                }

                // Create new user
                var newUser = new Users
                {
                    Email = email,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    CreatedDate = DateTime.Now,
                    PlanStatus = "Free",
                    DailyUsageCount = 0,
                    LastUsageReset = DateTime.Today,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true
                };

                int newUserId = DAL.insert(newUser);
                if (newUserId > 0)
                {
                    newUser.UserID = newUserId;
                    return newUser;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Registration error: " + ex.Message);
                throw ex;
            }

            return null;
        }

        public static bool CanGenerateRecipe(int userId)
        {
            try
            {
                var user = DAL.select<Users>("SELECT * FROM Users WHERE UserID = @UserID", 
                    new System.Data.SqlClient.SqlParameter[] 
                    {
                        new System.Data.SqlClient.SqlParameter("@UserID", userId)
                    }).FirstOrDefault();

                if (user == null) return false;

                // Check if we need to reset daily usage
                if (user.LastUsageReset.Date < DateTime.Today)
                {
                    DAL.update("UPDATE Users SET DailyUsageCount = 0, LastUsageReset = @Today WHERE UserID = @UserID",
                    new System.Data.SqlClient.SqlParameter[]
                        {
                            new System.Data.SqlClient.SqlParameter("@Today", DateTime.Today),
                            new System.Data.SqlClient.SqlParameter("@UserID", userId)
                        });
                    user.DailyUsageCount = 0;
                }

                // Check limits based on plan
                int maxRecipes = user.PlanStatus == "Premium" ? 30 : 3;
                return user.DailyUsageCount < maxRecipes;
            }
            catch (Exception ex)
            {
                Logger.Error("Usage check error: " + ex.Message);
                return false;
            }
        }

        public static void IncrementUsageCount(int userId)
        {
            try
            {
                DAL.update("UPDATE Users SET DailyUsageCount = DailyUsageCount + 1 WHERE UserID = @UserID",
                    new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@UserID", userId)
                    });
            }
            catch (Exception ex)
            {
                Logger.Error("Usage increment error: " + ex.Message);
            }
        }
    }
}