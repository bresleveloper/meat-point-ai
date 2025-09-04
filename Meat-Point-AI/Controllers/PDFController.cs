using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using Meat_Point_AI.App_Data;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.Controllers
{
    public class PDFController : ApiController
    {
        [HttpGet]
        [Route("api/pdf/generate/{recipeId}")]
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

                // Get the beef cut information
                var beefCut = DAL.select<BeefCuts>($"SELECT * FROM BeefCuts WHERE BeefCutID = {recipe.BeefCutID}").FirstOrDefault();

                // Generate PDF
                byte[] pdfBytes = PDFService.GenerateRecipePDF(recipe, beefCut);

                // Create response
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(pdfBytes);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"BeefMaster-{SanitizeFileName(recipe.Title)}.pdf"
                };

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("PDF generation error: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to generate PDF");
            }
        }

        [HttpGet]
        [Route("api/pdf/info/{recipeId}")]
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
                    FileName = $"BeefMaster-{SanitizeFileName(recipe.Title)}.pdf",
                    Pages = 2,
                    Description = "2-page PDF with shopping list and cooking instructions"
                };
            }
            catch (Exception ex)
            {
                Logger.Error("PDF info error: " + ex.Message);
                return new { Success = false, Message = "Failed to get PDF information" };
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