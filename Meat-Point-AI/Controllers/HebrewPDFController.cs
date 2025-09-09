using Meat_Point_AI.App_Data;
using Meat_Point_AI.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace Meat_Point_AI.Controllers
{
    public class HebrewPDFController : ApiController
    {
        [HttpGet]
        [Route("api/hebrewpdf/generate/{recipeId}")]
        [JwtAuthorize]
        public HttpResponseMessage GenerateRecipePDF(int recipeId)
        {
            try
            {
                // Get user ID from JWT token
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User not authenticated");
                }

                // Get the recipe
                var recipe = DAL.select<Recipes>($"SELECT * FROM Recipes WHERE RecipeID = {recipeId}").FirstOrDefault();
                if (recipe == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Recipe not found");
                }

                // Verify the recipe belongs to the authenticated user
                if (recipe.UserID != (int)userId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Access denied to this recipe");
                }

                // Generate Hebrew PDF
                byte[] pdfBytes = HebrewPDFService.GenerateRecipePDF(recipe);

                // Create response
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(pdfBytes);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                string encodedFilename = HttpUtility.UrlEncode($"BeefMaster-Hebrew-{recipe.Title}.pdf", Encoding.UTF8);

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = encodedFilename//, FileNameStar = $"BeefMaster-Hebrew-{recipe.Title}.pdf"
                };

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Hebrew PDF generation error: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to generate Hebrew PDF");
            }
        }

        [HttpGet]
        [Route("api/hebrewpdf/info/{recipeId}")]
        [JwtAuthorize]
        public object GetRecipePDFInfo(int recipeId)
        {
            try
            {
                // Get user ID from JWT token
                var userId = JwtHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return new { Success = false, Message = "User not authenticated" };
                }

                // Get the recipe
                var recipe = DAL.select<Recipes>($"SELECT * FROM Recipes WHERE RecipeID = {recipeId}").FirstOrDefault();
                if (recipe == null)
                {
                    return new { Success = false, Message = "Recipe not found" };
                }

                // Verify the recipe belongs to the authenticated user
                if (recipe.UserID != (int)userId)
                {
                    return new { Success = false, Message = "Access denied to this recipe" };
                }

                return new
                {
                    Success = true,
                    RecipeTitle = recipe.Title,
                    FileName = $"BeefMaster-Hebrew-{SanitizeFileName(recipe.Title)}.pdf",
                    Pages = 2,
                    Description = "2-page Hebrew RTL PDF with shopping list and cooking instructions"
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Hebrew PDF info error: " + ex.Message);
                return new { Success = false, Message = "Failed to get Hebrew PDF information" };
            }
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid file name characters
            string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            
            // Limit length and remove extra spaces
            fileName = fileName.Trim().Replace(" ", "-");
            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 50);
            }
            
            return fileName;
        }
    }
}