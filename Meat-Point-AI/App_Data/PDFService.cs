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

        public static byte[] GenerateRecipePDF(Recipes recipe, BeefCuts beefCut)
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
                    GenerateShoppingListPage(document, recipe, beefCut, shoppingList, ingredients, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

                    // Start new page
                    document.NewPage();

                    // PAGE 2 - COOKING RECIPE
                    GenerateCookingRecipePage(document, recipe, beefCut, ingredients, instructions, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

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

        private static void GenerateShoppingListPage(Document document, Recipes recipe, BeefCuts beefCut, 
            List<ShoppingListItem> shoppingList, List<RecipeIngredient> ingredients,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {
            // Header with logo
            Paragraph header = new Paragraph("🥩 BEEF MASTER - SHOPPING LIST", titleFont);
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
            AddInfoCell(infoTable, "Skill Level: " + GetComplexityStars(recipe.ComplexityLevel), normalFont);
            
            document.Add(infoTable);

            // Beef cut section
            if (beefCut != null)
            {
                Paragraph beefHeader = new Paragraph("🥩 PRIMARY BEEF CUT", subHeaderFont);
                beefHeader.SpacingAfter = 10f;
                document.Add(beefHeader);

                PdfPTable beefTable = new PdfPTable(1);
                beefTable.WidthPercentage = 100;
                beefTable.SpacingAfter = 20f;

                PdfPCell beefCell = new PdfPCell();
                beefCell.Padding = 10f;
                beefCell.BackgroundColor = new BaseColor(255, 248, 220); // Cornsilk

                string beefInfo = $"• {beefCut.Name} - from {beefCut.CowBodyLocation}\n";
                beefInfo += $"• Look for: {beefCut.MarblingLevel} marbling, {beefCut.Tenderness} texture\n";
                beefInfo += $"• Ask butcher for approximately {recipe.NumberOfDiners * 0.5:F1} lbs total";

                beefCell.AddElement(new Paragraph(beefInfo, normalFont));
                beefTable.AddCell(beefCell);
                document.Add(beefTable);
            }

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
                    string itemText = "   □ " + item.item;
                    if (!string.IsNullOrEmpty(item.notes))
                        itemText += " (" + item.notes + ")";

                    Paragraph itemPara = new Paragraph(itemText, normalFont);
                    itemPara.SpacingAfter = 3f;
                    document.Add(itemPara);
                }

                document.Add(new Paragraph(" ", normalFont)); // Spacing between categories
            }

            // Shopping tips
            Paragraph tipsHeader = new Paragraph("💡 SHOPPING TIPS", subHeaderFont);
            tipsHeader.SpacingAfter = 10f;
            document.Add(tipsHeader);

            string tips = "• Shop for meat last to keep it cold\n";
            tips += "• Don't be afraid to ask the butcher questions about cuts\n";
            tips += "• Check expiration dates and choose the freshest meat\n";
            tips += "• Consider buying extra and freezing for later use";

            Paragraph tipsPara = new Paragraph(tips, normalFont);
            tipsPara.SpacingAfter = 20f;
            document.Add(tipsPara);

            // Footer
            Paragraph footer = new Paragraph("Generated with Beef Master 🤖\nGenerated on " + DateTime.Now.ToString("yyyy-MM-dd"), smallFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);
        }

        private static void GenerateCookingRecipePage(Document document, Recipes recipe, BeefCuts beefCut,
            List<RecipeIngredient> ingredients, List<string> instructions,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {
            // Header
            Paragraph header = new Paragraph("🥩 BEEF MASTER - COOKING RECIPE", titleFont);
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

            // Beef cut identification
            if (beefCut != null)
            {
                PdfPTable cutTable = new PdfPTable(1);
                cutTable.WidthPercentage = 100;
                cutTable.SpacingAfter = 20f;

                PdfPCell cutCell = new PdfPCell();
                cutCell.Padding = 10f;
                cutCell.BackgroundColor = new BaseColor(240, 230, 140); // Khaki

                string cutInfo = $"🥩 BEEF CUT: {beefCut.Name}\n";
                cutInfo += $"Location: {beefCut.CowBodyLocation} | Tenderness: {beefCut.Tenderness}\n";
                cutInfo += $"Best cooking methods: {beefCut.BestCookingMethods}";

                cutCell.AddElement(new Paragraph(cutInfo, normalFont));
                cutTable.AddCell(cutCell);
                document.Add(cutTable);
            }

            // Equipment needed (derived from cooking method)
            Paragraph equipHeader = new Paragraph("🔧 EQUIPMENT NEEDED", subHeaderFont);
            equipHeader.SpacingAfter = 8f;
            document.Add(equipHeader);

            string equipment = GetEquipmentForCookingMethod(recipe.CookingMethod);
            Paragraph equipPara = new Paragraph(equipment, normalFont);
            equipPara.SpacingAfter = 15f;
            document.Add(equipPara);

            // Ingredients by category
            Paragraph ingredHeader = new Paragraph("📝 INGREDIENTS", subHeaderFont);
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
            }

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

            // Cooking tips
            if (beefCut != null && !string.IsNullOrEmpty(beefCut.CookingTips))
            {
                Paragraph tipsHeader = new Paragraph("💡 COOKING TIPS", subHeaderFont);
                tipsHeader.SpacingAfter = 8f;
                document.Add(tipsHeader);

                Paragraph tipsPara = new Paragraph(beefCut.CookingTips, normalFont);
                tipsPara.SpacingAfter = 15f;
                document.Add(tipsPara);
            }

            // Footer
            Paragraph footer = new Paragraph("🤖 Generated with Beef Master AI Chef\nCo-Authored-By: Claude\nGenerated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), smallFont);
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