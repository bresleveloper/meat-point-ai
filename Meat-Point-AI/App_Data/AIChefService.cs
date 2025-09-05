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

                // No specific beef cut - AI will suggest appropriate cut

                // Build the AI prompt
                string systemPrompt = BuildSystemPrompt();
                string userPrompt = BuildUserPrompt(request);

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
You create recipes that are very tasteful and matched to the cook's skill level. Your recipes focus ONLY on beef cuts from cows.

CRITICAL QUANTITY REQUIREMENTS:
- ALL ingredients MUST include specific, precise quantities
- NEVER leave quantities vague or missing
- Calculate meat portions: 6-8 oz per person (e.g., 4 people = 2-2.5 lbs total meat)
- Use standard measurements: lbs, oz, cups, tbsp, tsp, pieces, cloves

MEAT PORTION GUIDELINES:
- Steaks/chops: 6-8 oz per person
- Roasts: 8-10 oz per person (includes bone weight)
- Ground beef: 4-6 oz per person
- Always specify cut thickness for steaks (e.g., ""1-inch thick ribeye steaks"")

MEASUREMENT STANDARDS:
- Meat: ""2 lbs ribeye steaks"", ""1 lb ground beef (80/20)""
- Vegetables: ""1 large yellow onion"", ""3 cloves garlic"", ""2 medium carrots""
- Liquids: ""2 cups beef broth"", ""1/4 cup olive oil"", ""2 tbsp soy sauce""
- Seasonings: ""2 tsp salt"", ""1 tbsp black pepper"", ""1 tsp garlic powder""

IMPORTANT: You must respond with a valid JSON object in this exact format:
{
  ""title"": ""Recipe name"",
  ""description"": ""Brief description"",
  ""ingredients"": [
    {""item"": ""ribeye steaks"", ""quantity"": ""2 lbs (1-inch thick)"", ""category"": ""Meat""},
    {""item"": ""yellow onion"", ""quantity"": ""1 large"", ""category"": ""Vegetables""},
    {""item"": ""garlic"", ""quantity"": ""3 cloves"", ""category"": ""Vegetables""},
    {""item"": ""olive oil"", ""quantity"": ""2 tbsp"", ""category"": ""Pantry""}
  ],
  ""instructions"": [
    ""Step 1 instruction"",
    ""Step 2 instruction""
  ],
  ""temperatureGuide"": ""Internal temperatures and doneness levels"",
  ""shoppingList"": [
    {""item"": ""ribeye steaks"", ""category"": ""Meat"", ""notes"": ""Ask for 1-inch thick cuts""},
    {""item"": ""yellow onion"", ""category"": ""Vegetables"", ""notes"": ""Choose large, firm onion""}
  ],
  ""cookingTips"": ""Add 1 deep insight about the specific cut""
}

VALIDATION RULES:
- Every ingredient MUST have a specific quantity (not ""some"", ""a little"", or ""to taste"")
- Quantities must be measurable and precise
- Include preparation notes in item name when relevant (e.g., ""diced"", ""minced"", ""sliced"")

Make recipes that emphasize the taste of the specific beef cut.";
        }

        private static string BuildUserPrompt(RecipeGenerationRequest request)
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

COOKING PREFERENCES:
- Cooking method: {request.CookingMethod}
- Cooking time: approximately {request.CookingTimeMinutes} minutes
- Number of diners: {request.NumberOfDiners}
- Complexity level: {(complexityDescriptions.ContainsKey(request.ComplexityLevel) ? complexityDescriptions[request.ComplexityLevel] : "Intermediate")}
- Dietary restrictions: {request.DietaryRestrictions}
{ageContext}

{(!string.IsNullOrEmpty(request.UserPrompt) ? $"SPECIAL REQUESTS: {request.UserPrompt}" : "")}

BEEF CUT SELECTION:
Based on the cooking time and method preferences, YOU MUST select the most appropriate beef cut:
- For SHORT cooking times (≤30 minutes): Choose TENDER cuts (Tenderloin, Ribeye, Strip Steak, Sirloin)
- For LONG cooking times (≥120 minutes): Choose TOUGHER cuts perfect for braising/slow cooking (Chuck Roast, Brisket, Short Ribs)
- For MEDIUM cooking times (30-120 minutes): Choose MODERATE tenderness cuts (Top Round, Bottom Round, Tri-tip)

Create a recipe that:
1. SELECTS and EXPLAINS the best beef cut for the cooking method and time specified
2. EXPLAINS why this cut works best for the cooking method (tenderness level, marbling, etc.)
3. Teaches about the selected beef cut and why it's prepared this way
4. Matches the complexity level (simpler for lower levels, more advanced for higher levels)
5. Includes proper temperature guidelines for food safety
6. Provides a complete shopping list organized by category
7. Gives educational tips about working with the selected cut of beef

In your JSON response, make sure to:
- Include the selected beef cut name in the title
- Explain the cut selection reasoning in the description
- Add cut-specific cooking tips in the cookingTips section

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