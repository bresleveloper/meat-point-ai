using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Meat_Point_AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Meat_Point_AI.App_Data
{
    public class AIChefService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string OPENAI_API_KEY = System.Configuration.ConfigurationManager.AppSettings["OpenAI_API_Key"];
        private static readonly string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";

        public class RecipeGenerationRequest
        {
            public int BeefCutID { get; set; }
            public int ComplexityLevel { get; set; }
            public int NumberOfDiners { get; set; }
            public string DinerAges { get; set; }
            public string CookingMethod { get; set; }
            public int CookingTimeMinutes { get; set; }
            public string DietaryRestrictions { get; set; }
            public string UserPrompt { get; set; }
        }

        public class RecipeGenerationResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Recipes Recipe { get; set; }
        }

        static AIChefService()
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_KEY}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BeefMealPlanner/1.0");
        }

        public static async Task<RecipeGenerationResponse> GenerateRecipeAsync(RecipeGenerationRequest request, int userId)
        {
            try
            {
                // Check if user can generate recipe
                if (!AuthService.CanGenerateRecipe(userId))
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Daily recipe generation limit reached. Upgrade to premium for more recipes!"
                    };
                }

                // Get beef cut information
                var beefCut = DAL.select<BeefCuts>($"SELECT * FROM BeefCuts WHERE BeefCutID = {request.BeefCutID}").FirstOrDefault();
                if (beefCut == null)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Invalid beef cut selected"
                    };
                }

                // Build the AI prompt
                string systemPrompt = BuildSystemPrompt();
                string userPrompt = BuildUserPrompt(request, beefCut);

                // Call OpenAI API
                var openAIResponse = await CallOpenAIAsync(systemPrompt, userPrompt);
                if (!openAIResponse.Success)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Failed to generate recipe: " + openAIResponse.Message
                    };
                }

                // Parse the response into a Recipe object
                var recipe = ParseAIResponse(openAIResponse.Content, request, userId);
                if (recipe == null)
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Failed to parse recipe from AI response"
                    };
                }

                // Save the recipe to database
                int recipeId = DAL.insert(recipe, true);
                if (recipeId > 0)
                {
                    recipe.RecipeID = recipeId;
                    
                    // Increment user's usage count
                    AuthService.IncrementUsageCount(userId);
                    
                    return new RecipeGenerationResponse
                    {
                        Success = true,
                        Message = "Recipe generated successfully!",
                        Recipe = recipe
                    };
                }
                else
                {
                    return new RecipeGenerationResponse
                    {
                        Success = false,
                        Message = "Failed to save recipe to database"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AI Recipe Generation error: " + ex.Message);
                return new RecipeGenerationResponse
                {
                    Success = false,
                    Message = "An error occurred while generating the recipe"
                };
            }
        }

        private static string BuildSystemPrompt()
        {
            return @"You are an expert beef chef specializing in teaching people how to cook different cuts of beef. 
You create recipes that are educational and matched to the cook's skill level. Your recipes focus ONLY on beef cuts from cows.

IMPORTANT: You must respond with a valid JSON object in this exact format:
{
  ""title"": ""Recipe name"",
  ""description"": ""Brief description"",
  ""ingredients"": [
    {""item"": ""ingredient name"", ""quantity"": ""amount"", ""category"": ""Meat/Vegetables/Seasonings/Pantry""}
  ],
  ""instructions"": [
    ""Step 1 instruction"",
    ""Step 2 instruction""
  ],
  ""temperatureGuide"": ""Internal temperatures and doneness levels"",
  ""shoppingList"": [
    {""item"": ""shopping item"", ""category"": ""category"", ""notes"": ""special instructions""}
  ],
  ""cookingTips"": ""Additional tips for this specific cut""
}

Make recipes educational about the specific beef cut, its characteristics, and why certain cooking methods work best.";
        }

        private static string BuildUserPrompt(RecipeGenerationRequest request, BeefCuts beefCut)
        {
            var complexityDescriptions = new Dictionary<int, string>
            {
                {1, "\"Stupid Dad\" level - Very simple, hard to mess up, minimal technique required"},
                {2, "Beginner level - Basic techniques, forgiving cuts, simple seasonings"},
                {3, "Intermediate level - Some technique required, moderate complexity"},
                {4, "Advanced level - Requires skill, timing, and attention to detail"},
                {5, "\"Super Chef Mom\" level - Expert techniques, complex flavors, professional methods"}
            };

            string ageContext = "";
            if (!string.IsNullOrEmpty(request.DinerAges))
            {
                if (request.DinerAges.Contains("Child") || request.DinerAges.Contains("Kid"))
                {
                    ageContext = "Make the recipe kid-friendly with milder flavors and fun presentation. ";
                }
            }

            return $@"Create a beef recipe with these requirements:

BEEF CUT: {beefCut.Name}
- Description: {beefCut.Description}
- Tenderness: {beefCut.Tenderness}
- Best cooking methods: {beefCut.BestCookingMethods}
- Complexity level: {(complexityDescriptions.ContainsKey(request.ComplexityLevel) ? complexityDescriptions[request.ComplexityLevel] : "Intermediate")}

COOKING REQUIREMENTS:
- Cooking method: {request.CookingMethod}
- Cooking time: approximately {request.CookingTimeMinutes} minutes
- Number of diners: {request.NumberOfDiners}
- Dietary restrictions: {request.DietaryRestrictions}
{ageContext}

{(!string.IsNullOrEmpty(request.UserPrompt) ? $"SPECIAL REQUESTS: {request.UserPrompt}" : "")}

Create a recipe that:
1. Teaches about this specific beef cut and why it's prepared this way
2. Matches the complexity level (simpler for lower levels, more advanced for higher levels)
3. Includes proper temperature guidelines for food safety
4. Provides a complete shopping list organized by category
5. Gives educational tips about working with this cut of beef

Respond ONLY with valid JSON in the specified format.";
        }

        private static async Task<(bool Success, string Content, string Message)> CallOpenAIAsync(string systemPrompt, string userPrompt)
        {
            try
            {
                var requestBody = new
                {
                    //model = "gpt-3.5-turbo",
                    model = "gpt-4.1-nano",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    max_tokens = 2000,
                    temperature = 0.7
                };

                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(OPENAI_API_URL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    //dynamic responseObj = JsonConvert.DeserializeObject(responseBody);
                    JObject jObject = JsonConvert.DeserializeObject<JObject>(responseBody);
                    //var structures = jObject["user"]["bab"]["structures"][0];
                    //string aiResponse = responseObj.choices[0].message.content;
                    string aiResponse = jObject["choices"][0]["message"]["content"].ToString();
                    return (true, aiResponse, "Success");
                }
                else
                {
                    Logger.Error($"OpenAI API error: {response.StatusCode} - {responseBody}");
                    return (false, null, $"OpenAI API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenAI API call error: {ex.Message}");
                return (false, null, ex.Message);
            }
        }

        private static Recipes ParseAIResponse(string aiResponse, RecipeGenerationRequest request, int userId)
        {
            try
            {
                // Clean up the response in case it has markdown formatting
                aiResponse = aiResponse.Trim();
                if (aiResponse.StartsWith("```json"))
                {
                    aiResponse = aiResponse.Substring(7);
                }
                if (aiResponse.EndsWith("```"))
                {
                    aiResponse = aiResponse.Substring(0, aiResponse.Length - 3);
                }

                //dynamic responseObj = JsonConvert.DeserializeObject(aiResponse);
                JObject jObject = JsonConvert.DeserializeObject<JObject>(aiResponse);


                var recipe = new Recipes
                {
                    UserID = userId,
                    BeefCutID = request.BeefCutID,
                    //Title = jObject["title,
                    Title = jObject["title"].ToString(),
                    Description = jObject["description"].ToString(),
                    ComplexityLevel = request.ComplexityLevel,
                    NumberOfDiners = request.NumberOfDiners,
                    DinerAges = request.DinerAges ?? "",
                    CookingMethod = request.CookingMethod ?? "",
                    CookingTimeMinutes = request.CookingTimeMinutes,
                    DietaryRestrictions = request.DietaryRestrictions ?? "",
                    Ingredients = JsonConvert.SerializeObject(jObject["ingredients"].ToString()),
                    Instructions = JsonConvert.SerializeObject(jObject["instructions"].ToString()),
                    TemperatureGuide = jObject["temperatureGuide"].ToString(),
                    ShoppingList = JsonConvert.SerializeObject(jObject["shoppingList"].ToString()),
                    UserPrompt = request.UserPrompt ?? "",
                    CreatedDate = DateTime.Now,
                    IsFavorite = false,
                    Notes = jObject["cookingTips"].ToString()
                };

                return recipe;
            }
            catch (Exception ex)
            {
                Logger.Error($"Recipe parsing error: {ex.Message}");
                return null;
            }
        }
    }
}