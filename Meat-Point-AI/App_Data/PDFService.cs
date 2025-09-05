using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.App_Data
{
    public class PDFService
    {
        public class RecipeIngredient
        {
            public string item { get; set; }
            public string quantity { get; set; }
            public string category { get; set; }
        }

        public class ShoppingListItem
        {
            public string item { get; set; }
            public string category { get; set; }
            public string notes { get; set; }
        }

        public static byte[] GenerateRecipePDF(Recipes recipe)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Define fonts
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.DARK_GRAY);
                    Font subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                    Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);

                    // Parse recipe data
                    List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
                    List<ShoppingListItem> shoppingList = new List<ShoppingListItem>();
                    List<string> instructions = new List<string>();


                    try
                    {
                        //JsonConvert.DeserializeObject<List<RecipeIngredient>>(JsonConvert.DeserializeObject(recipe.Ingredients).ToString())
                        if (!string.IsNullOrEmpty(recipe.Ingredients))
                            //ingredients = JsonConvert.DeserializeObject<List<RecipeIngredient>>(recipe.Ingredients);
                            ingredients = JsonConvert.DeserializeObject<List<RecipeIngredient>>(JsonConvert.DeserializeObject(recipe.Ingredients).ToString());
                            if (!string.IsNullOrEmpty(recipe.ShoppingList))
                            //shoppingList = JsonConvert.DeserializeObject<List<ShoppingListItem>>(recipe.ShoppingList);
                            shoppingList = JsonConvert.DeserializeObject<List<ShoppingListItem>>(JsonConvert.DeserializeObject(recipe.ShoppingList).ToString());
                        if (!string.IsNullOrEmpty(recipe.Instructions))
                            //instructions = JsonConvert.DeserializeObject<List<string>>(recipe.Instructions);
                            instructions = JsonConvert.DeserializeObject<List<string>>(JsonConvert.DeserializeObject(recipe.Instructions).ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error parsing recipe data for PDF: " + ex.Message);
                    }

                    // PAGE 1 - SHOPPING LIST
                    GenerateShoppingListPage(document, recipe, shoppingList, ingredients, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

                    // Start new page
                    document.NewPage();

                    // PAGE 2 - COOKING RECIPE
                    GenerateCookingRecipePage(document, recipe, ingredients, instructions, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

                    document.Close();
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("PDF Generation error: " + ex.Message);
                throw;
            }
        }

        private static void GenerateShoppingListPage(Document document, Recipes recipe, 
            List<ShoppingListItem> shoppingList, List<RecipeIngredient> ingredients,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {
            // Header with logo
            Paragraph header = new Paragraph("🥩 MEAT POINT AI - SHOPPING LIST", titleFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 20f;
            document.Add(header);

            // Recipe title
            Paragraph recipeTitle = new Paragraph(recipe.Title, headerFont);
            recipeTitle.Alignment = Element.ALIGN_CENTER;
            recipeTitle.SpacingAfter = 15f;
            document.Add(recipeTitle);

            // Recipe info
            PdfPTable infoTable = new PdfPTable(3);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingAfter = 20f;
            
            AddInfoCell(infoTable, "Serves: " + recipe.NumberOfDiners, normalFont);
            AddInfoCell(infoTable, "Cooking Time: " + GetCookingTimeDisplay(recipe.CookingTimeMinutes), normalFont);
            AddInfoCell(infoTable, "Skill Level: " + GetComplexityLabel(recipe.ComplexityLevel) + " " + GetComplexityStars(recipe.ComplexityLevel), normalFont);
            
            document.Add(infoTable);


            // Shopping list by category
            var shoppingByCategory = shoppingList.GroupBy(s => s.category ?? "Other").ToDictionary(g => g.Key, g => g.ToList());
            
            foreach (var category in shoppingByCategory)
            {
                // Category header
                Paragraph categoryHeader = new Paragraph("□ " + category.Key.ToUpper(), subHeaderFont);
                categoryHeader.SpacingAfter = 5f;
                document.Add(categoryHeader);

                // Items in category
                foreach (var item in category.Value)
                {
                    // Try to find quantity from ingredients
                    string quantity = FindIngredientQuantity(item.item, ingredients);
                    
                    string itemText = "   □ ";
                    if (!string.IsNullOrEmpty(quantity))
                    {
                        itemText += quantity + " " + item.item;
                    }
                    else
                    {
                        itemText += item.item;
                    }
                    
                    if (!string.IsNullOrEmpty(item.notes))
                        itemText += " (" + item.notes + ")";

                    Paragraph itemPara = new Paragraph(itemText, normalFont);
                    itemPara.SpacingAfter = 3f;
                    document.Add(itemPara);
                }

                document.Add(new Paragraph(" ", normalFont)); // Spacing between categories
            }

            // Footer
            Paragraph footer = new Paragraph("Generated with MEAT POINT AI 🤖\nGenerated on " + DateTime.Now.ToString("yyyy-MM-dd"), smallFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);
        }

        private static void GenerateCookingRecipePage(Document document, Recipes recipe,
            List<RecipeIngredient> ingredients, List<string> instructions,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {
            // Header
            Paragraph header = new Paragraph("🥩 MEAT POINT AI - COOKING RECIPE", titleFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 20f;
            document.Add(header);

            // Recipe title and info
            Paragraph recipeTitle = new Paragraph(recipe.Title, headerFont);
            recipeTitle.Alignment = Element.ALIGN_CENTER;
            recipeTitle.SpacingAfter = 10f;
            document.Add(recipeTitle);

            Paragraph description = new Paragraph(recipe.Description, normalFont);
            description.Alignment = Element.ALIGN_CENTER;
            description.SpacingAfter = 15f;
            document.Add(description);


            // Equipment needed (derived from cooking method)
            Paragraph equipHeader = new Paragraph("🔧 EQUIPMENT NEEDED", subHeaderFont);
            equipHeader.SpacingAfter = 8f;
            document.Add(equipHeader);

            string equipment = GetEquipmentForCookingMethod(recipe.CookingMethod);
            Paragraph equipPara = new Paragraph(equipment, normalFont);
            equipPara.SpacingAfter = 15f;
            document.Add(equipPara);

            // Ingredients by category
            /*Paragraph ingredHeader = new Paragraph("📝 INGREDIENTS", subHeaderFont);
            ingredHeader.SpacingAfter = 10f;
            document.Add(ingredHeader);

            var ingredientsByCategory = ingredients.GroupBy(i => i.category ?? "Other").ToDictionary(g => g.Key, g => g.ToList());

            foreach (var category in ingredientsByCategory)
            {
                Paragraph catHeader = new Paragraph(category.Key + ":", normalFont);
                catHeader.SpacingAfter = 5f;
                document.Add(catHeader);

                foreach (var ingredient in category.Value)
                {
                    string ingredText = $"• {ingredient.quantity} {ingredient.item}";
                    Paragraph ingredPara = new Paragraph(ingredText, normalFont);
                    ingredPara.SpacingAfter = 3f;
                    document.Add(ingredPara);
                }
                document.Add(new Paragraph(" ", normalFont));
            }*/

            // Cooking instructions
            Paragraph instrHeader = new Paragraph("👩‍🍳 COOKING INSTRUCTIONS", subHeaderFont);
            instrHeader.SpacingAfter = 10f;
            document.Add(instrHeader);

            for (int i = 0; i < instructions.Count; i++)
            {
                Paragraph stepPara = new Paragraph($"{i + 1}. {instructions[i]}", normalFont);
                stepPara.SpacingAfter = 8f;
                stepPara.FirstLineIndent = -20f;
                stepPara.IndentationLeft = 20f;
                document.Add(stepPara);
            }

            // Temperature guide
            if (!string.IsNullOrEmpty(recipe.TemperatureGuide))
            {
                Paragraph tempHeader = new Paragraph("🌡️ TEMPERATURE GUIDE", subHeaderFont);
                tempHeader.SpacingAfter = 8f;
                document.Add(tempHeader);

                Paragraph tempPara = new Paragraph(recipe.TemperatureGuide, normalFont);
                tempPara.SpacingAfter = 15f;
                document.Add(tempPara);
            }


            // Cooking tips (in ai, notes in db)
            Paragraph tipsHeader = new Paragraph("💡 ONE FREE DEEP INSIGHT ON THIS CUT", subHeaderFont);
            tipsHeader.SpacingAfter = 8f;
            document.Add(tipsHeader);

            Paragraph tipsPara = new Paragraph(recipe.Notes, normalFont);
            tipsPara.SpacingAfter = 15f;
            document.Add(tipsPara);


            // Footer
            Paragraph footer = new Paragraph("🤖 Generated with MEAT POINT AI Chef\nCo-Authored-By: Claude\nGenerated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), smallFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);
        }

        private static void AddInfoCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = Rectangle.NO_BORDER;
            cell.Padding = 5f;
            table.AddCell(cell);
        }

        private static string GetCookingTimeDisplay(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} minutes";
            else
            {
                int hours = minutes / 60;
                int remainingMinutes = minutes % 60;
                return remainingMinutes > 0 ? $"{hours}h {remainingMinutes}m" : $"{hours} hour{(hours > 1 ? "s" : "")}";
            }
        }

        private static string GetComplexityStars(int level)
        {
            return new string('★', level) + new string('☆', 5 - level);
        }

        private static string GetComplexityLabel(int level)
        {
            switch (level)
            {
                case 1: return "🤷‍♂️ Stupid Dad";
                case 2: return "👨‍🍳 Kitchen Newbie";
                case 3: return "🏠 Home Cook";
                case 4: return "👩‍🍳 Skilled Chef";
                case 5: return "⭐ Super Chef Mom";
                default: return "Unknown Level";
            }
        }

        private static string FindIngredientQuantity(string shoppingItem, List<RecipeIngredient> ingredients)
        {
            if (ingredients == null || string.IsNullOrEmpty(shoppingItem))
                return string.Empty;

            // Try exact match first
            var exactMatch = ingredients.FirstOrDefault(i => 
                string.Equals(i.item, shoppingItem, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null && !string.IsNullOrEmpty(exactMatch.quantity))
                return exactMatch.quantity;

            // Try partial match (shopping item contains ingredient or vice versa)
            var partialMatch = ingredients.FirstOrDefault(i =>
                (!string.IsNullOrEmpty(i.item) && 
                 (i.item.ToLower().Contains(shoppingItem.ToLower()) || 
                  shoppingItem.ToLower().Contains(i.item.ToLower()))));
            if (partialMatch != null && !string.IsNullOrEmpty(partialMatch.quantity))
                return partialMatch.quantity;

            return string.Empty;
        }

        private static string GetEquipmentForCookingMethod(string cookingMethod)
        {
            switch (cookingMethod?.ToLower())
            {
                case "grilling":
                    return "• Grill (gas or charcoal)\n• Meat thermometer\n• Tongs\n• Grill brush";
                case "pan-frying":
                    return "• Heavy skillet or cast iron pan\n• Meat thermometer\n• Tongs or spatula\n• Paper towels";
                case "roasting":
                    return "• Roasting pan\n• Meat thermometer\n• Aluminum foil\n• Oven";
                case "braising":
                    return "• Dutch oven or heavy pot with lid\n• Meat thermometer\n• Tongs\n• Ladle";
                case "slow cooking":
                    return "• Slow cooker or Dutch oven\n• Meat thermometer\n• Tongs\n• Ladle";
                case "broiling":
                    return "• Broiler pan\n• Meat thermometer\n• Tongs\n• Oven mitts";
                case "smoking":
                    return "• Smoker\n• Wood chips\n• Meat thermometer\n• Spray bottle for moisture";
                default:
                    return "• Meat thermometer\n• Appropriate cookware for method\n• Tongs or spatula\n• Timer";
            }
        }
    }
}