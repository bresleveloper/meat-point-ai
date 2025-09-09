using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Meat_Point_AI.Models;

namespace Meat_Point_AI.App_Data
{
    public class HebrewPDFService
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
                    writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    document.Open();

                    // Define Hebrew-compatible fonts
                    Font titleFont = GetHebrewFont(18, Font.BOLD, BaseColor.DARK_GRAY);
                    Font headerFont = GetHebrewFont(14, Font.BOLD, BaseColor.DARK_GRAY);
                    Font subHeaderFont = GetHebrewFont(12, Font.BOLD, BaseColor.DARK_GRAY);
                    Font normalFont = GetHebrewFont(10, Font.NORMAL, BaseColor.BLACK);
                    Font smallFont = GetHebrewFont(8, Font.NORMAL, BaseColor.GRAY);

                    // Parse recipe data
                    List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
                    List<ShoppingListItem> shoppingList = new List<ShoppingListItem>();
                    List<string> instructions = new List<string>();

                    try
                    {
                        if (!string.IsNullOrEmpty(recipe.Ingredients))
                            ingredients = JsonConvert.DeserializeObject<List<RecipeIngredient>>(JsonConvert.DeserializeObject(recipe.Ingredients).ToString());
                        if (!string.IsNullOrEmpty(recipe.ShoppingList))
                            shoppingList = JsonConvert.DeserializeObject<List<ShoppingListItem>>(JsonConvert.DeserializeObject(recipe.ShoppingList).ToString());
                        if (!string.IsNullOrEmpty(recipe.Instructions))
                            instructions = JsonConvert.DeserializeObject<List<string>>(JsonConvert.DeserializeObject(recipe.Instructions).ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error parsing recipe data for Hebrew PDF: " + ex.Message);
                    }

                    // PAGE 1 - SHOPPING LIST (RTL)
                    GenerateHebrewShoppingListPage(document, recipe, shoppingList, ingredients, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

                    // Start new page
                    document.NewPage();

                    // PAGE 2 - COOKING RECIPE (RTL)
                    GenerateHebrewCookingRecipePage(document, recipe, ingredients, instructions, titleFont, headerFont, subHeaderFont, normalFont, smallFont);

                    document.Close();
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Hebrew PDF Generation error: " + ex.Message);
                throw;
            }
        }

        private static Font GetHebrewFont(float size, int style, BaseColor color)
        {
            try
            {
                // Try to load Hebrew-compatible font
                // First try Arial Unicode MS (common on Windows)
                string fontPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialuni.ttf");
                if (System.IO.File.Exists(fontPath))
                {
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    return new Font(baseFont, size, style, color);
                }

                // Try Arial (has some Hebrew support)
                fontPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                if (System.IO.File.Exists(fontPath))
                {
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    return new Font(baseFont, size, style, color);
                }

                // Fallback to built-in font (may not display Hebrew correctly)
                return FontFactory.GetFont(FontFactory.HELVETICA, size, style, color);
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading Hebrew font: " + ex.Message);
                // Fallback to default font
                return FontFactory.GetFont(FontFactory.HELVETICA, size, style, color);
            }
        }

        private static string ProcessHebrewTextWithNumbers(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Check if text contains Hebrew characters (Hebrew Unicode range: 0x0590-0x05FF)
            bool containsHebrew = text.Any(c => c >= 0x0590 && c <= 0x05FF);
            if (!containsHebrew) return text; // Return as-is for non-Hebrew text

            // Use regex to find all numbers (integers and floats)
            var numberPattern = @"\b\d+(?:\.\d+)?\b";
            var matches = Regex.Matches(text, numberPattern);
            
            if (matches.Count == 0)
            {
                // No numbers found, use original reversal logic
                char[] charsShminner = text.ToCharArray();
                Array.Reverse(charsShminner);
                return new string(charsShminner);
            }

            // Extract numbers and replace with placeholders
            var numbers = new List<string>();
            string textWithPlaceholders = text;
            
            // Replace numbers with placeholders (reverse order to maintain indices)
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                numbers.Insert(0, match.Value); // Insert at beginning to maintain original order
                string placeholder = $"~N{i}~"; // Use a simple, symmetric placeholder
                textWithPlaceholders = textWithPlaceholders.Substring(0, match.Index) + 
                                     placeholder + 
                                     textWithPlaceholders.Substring(match.Index + match.Length);
            }

            // Reverse text with placeholders
            char[] chars = textWithPlaceholders.ToCharArray();
            Array.Reverse(chars);
            string reversedText = new string(chars);

            // Replace placeholders back with original numbers
            for (int i = 0; i < numbers.Count; i++)
            {
                reversedText = reversedText.Replace($"~{i}N~", numbers[i]); // Reversed placeholder pattern
            }

            return reversedText;
        }

        private static string ProcessHebrewText(string text)
        {
            return ProcessHebrewTextWithNumbers(text);
        }

        private static string ProcessHebrewLongText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Check if text contains Hebrew characters (Hebrew Unicode range: 0x0590-0x05FF)
            bool containsHebrew = text.Any(c => c >= 0x0590 && c <= 0x05FF);
            if (!containsHebrew) return text; // Return as-is for non-Hebrew text

            // Handle line breaks properly - split by lines, process each line individually, then rejoin
            if (text.Contains("\n"))
            {
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        // Use number-preserving Hebrew text processing for each line
                        lines[i] = ProcessHebrewTextWithNumbers(lines[i]);
                    }
                }
                return string.Join("\n", lines);
            }

            // dup linebreak functionality for dots
            if (text.Contains("."))
            {
                string[] lines = text.Split('.');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        // Use number-preserving Hebrew text processing for each line
                        lines[i] = ProcessHebrewTextWithNumbers(lines[i] + ".");
                    }
                }

                //return string.Join(".", lines);
                return string.Join("\n", lines);
            }

            // Use number-preserving Hebrew text processing for single line
            return ProcessHebrewTextWithNumbers(text);
        }

        private static string ProcessHebrewLongText2(string text)
        {
            List<string> lines = BreakText(text);
            List<string> reversed = new List<string>();
            foreach (var line in lines)
            {
                reversed.Add(ProcessHebrewText(line));
            }
            return string.Join("\n", reversed);
        }

        private static List<string> BreakText(string text, int maxLineLength = 100)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLineLength)
                return new List<string>() { text } ;

            var result = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                // If adding this word would exceed the limit
                if (currentLine.Length + word.Length + 1 > maxLineLength)
                {
                    // Add the current line if it's not empty
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        result.Add(currentLine.Trim());
                        currentLine = word;
                    }
                    else
                    {
                        // Handle case where single word is longer than max length
                        result.Add(word);
                    }
                }
                else
                {
                    // Add word to current line
                    currentLine += (string.IsNullOrEmpty(currentLine) ? "" : " ") + word;
                }
            }

            // Add the last line if it exists
            if (!string.IsNullOrEmpty(currentLine))
                result.Add(currentLine.Trim());

            return result;
        }

        private static Paragraph CreateRTLParagraph(string text, Font font, int alignment = Element.ALIGN_RIGHT)
        {
            // Process Hebrew text before creating paragraph
            string processedText = ProcessHebrewText(text);
            Paragraph paragraph = new Paragraph(processedText, font);
            paragraph.Alignment = alignment;
            return paragraph;
        }

        private static void GenerateHebrewShoppingListPage(Document document, Recipes recipe, 
            List<ShoppingListItem> shoppingList, List<RecipeIngredient> ingredients,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {

            // Header with logo
            Paragraph header = new Paragraph("🥩 MEAT POINT AI - " + ProcessHebrewText("רשימת קניות"), titleFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 20f;
            document.Add(header);


            // Header with logo (RTL)
            //Paragraph header = CreateRTLParagraph("🥩 MEAT POINT AI - " + ProcessHebrewText("רשימת קניות"), titleFont, Element.ALIGN_CENTER);
            /*Paragraph header = CreateRTLParagraph("🥩 MEAT POINT AI - " + "רשימת קניות", titleFont, Element.ALIGN_CENTER);
            header.SpacingAfter = 20f;
            document.Add(header);*/

            // Recipe title (RTL)
            //Paragraph recipeTitle = CreateRTLParagraph(ProcessHebrewText(recipe.Title), headerFont, Element.ALIGN_CENTER);
            Paragraph recipeTitle = CreateRTLParagraph(recipe.Title, headerFont, Element.ALIGN_CENTER);
            recipeTitle.SpacingAfter = 15f;
            document.Add(recipeTitle);

            // Recipe info (RTL aligned)
            PdfPTable infoTable = new PdfPTable(3);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingAfter = 20f;
            infoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

            //in order for the whole text to be correct i need to ProcessHebrewText all the "line"
            //since claude made so much ProcessHebrewText inside GetCookingTimeDisplayHebrew+GetComplexityLabelHebrew
            //i am lazy and will double GetComplexityLabelHebrew
            AddHebrewInfoCell(infoTable, ProcessHebrewText("מנות: " + recipe.NumberOfDiners) , normalFont);
            AddHebrewInfoCell(infoTable, ProcessHebrewText("זמן בישול: " + ProcessHebrewText(GetCookingTimeDisplayHebrew(recipe.CookingTimeMinutes))), normalFont);
            AddHebrewInfoCell(infoTable, ProcessHebrewText("רמת מיומנות: " + ProcessHebrewText(GetComplexityLabelHebrew(recipe.ComplexityLevel) + " " + GetComplexityStars(recipe.ComplexityLevel))), normalFont);
            
            document.Add(infoTable);

            // Shopping list by category (RTL)
            var shoppingByCategory = shoppingList.GroupBy(s => s.category ?? "Other").ToDictionary(g => g.Key, g => g.ToList());
            
            foreach (var category in shoppingByCategory)
            {
                // Category header (RTL)
                Paragraph categoryHeader = CreateRTLParagraph("□ " + TranslateCategoryToHebrew(category.Key), subHeaderFont);
                categoryHeader.SpacingAfter = 5f;
                document.Add(categoryHeader);

                // Items in category (RTL)
                foreach (var item in category.Value)
                {
                    // Try to find quantity from ingredients
                    string quantity = FindIngredientQuantity(item.item, ingredients);
                    
                    string itemText = "   □ ";
                    if (!string.IsNullOrEmpty(quantity))
                    {
                        //itemText += quantity + " " + ProcessHebrewText(item.item);
                        itemText += quantity + " " + item.item.Replace("(", ")").Replace(")", "(");
                    }
                    else
                    {
                        //itemText += ProcessHebrewText(item.item);
                        itemText += item.item.Replace("(", ")").Replace(")", "("); ;
                    }

                    if (!string.IsNullOrEmpty(item.notes))
                    {
                        //itemText += " (" + ProcessHebrewText(item.notes) + ")";
                        //easier to fix heb with changing () dirz
                        itemText += " )" + item.notes + "(";
                    }

                    Paragraph itemPara = CreateRTLParagraph(itemText, normalFont);
                    itemPara.SpacingAfter = 3f;
                    document.Add(itemPara);
                }

                document.Add(CreateRTLParagraph(" ", normalFont)); // Spacing between categories
            }

            // Footer (RTL)
            Paragraph footer = CreateRTLParagraph(ProcessHebrewText("נוצר עם MEAT POINT AI 🤖") + "\n" + ProcessHebrewText("נוצר ב ") + DateTime.Now.ToString("yyyy-MM-dd"), smallFont, Element.ALIGN_CENTER);
            document.Add(footer);
        }

        private static void GenerateHebrewCookingRecipePage(Document document, Recipes recipe,
            List<RecipeIngredient> ingredients, List<string> instructions,
            Font titleFont, Font headerFont, Font subHeaderFont, Font normalFont, Font smallFont)
        {
            // Header (RTL)

            // Header with logo
            Paragraph header = new Paragraph("🥩 MEAT POINT AI - " + ProcessHebrewText("רשימת קניות"), titleFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 20f;
            document.Add(header);

            /*
            Paragraph header = CreateRTLParagraph("🥩 MEAT POINT AI - " + ProcessHebrewText("מתכון בישול"), titleFont, Element.ALIGN_CENTER);
            header.SpacingAfter = 20f;
            document.Add(header);*/

            // Recipe title and info (RTL)
            Paragraph recipeTitle = CreateRTLParagraph(recipe.Title, headerFont, Element.ALIGN_CENTER);
            recipeTitle.SpacingAfter = 10f;
            document.Add(recipeTitle);

            /*Paragraph description = CreateRTLParagraph(recipe.Description, normalFont, Element.ALIGN_CENTER);
            description.SpacingAfter = 15f;
            document.Add(description);*/
            Paragraph description = new Paragraph(ProcessHebrewLongText2(recipe.Description), normalFont);
            description.Alignment = Element.ALIGN_RIGHT;
            description.SpacingAfter = 15f;
            document.Add(description);



            // Equipment needed (RTL)
            Paragraph equipHeader = CreateRTLParagraph("🔧 " + "ציוד נדרש", subHeaderFont);
            equipHeader.SpacingAfter = 8f;
            document.Add(equipHeader);

            string equipment = GetEquipmentForCookingMethodHebrew(recipe.CookingMethod);
            Paragraph equipPara = CreateRTLParagraph(ProcessHebrewText(equipment), normalFont);
            equipPara.SpacingAfter = 15f;
            document.Add(equipPara);

            // Cooking instructions (RTL)
            Paragraph instrHeader = CreateRTLParagraph("👩‍🍳 " + "הוראות בישול", subHeaderFont);
            instrHeader.SpacingAfter = 10f;
            document.Add(instrHeader);

            for (int i = 0; i < instructions.Count; i++)
            {
                //Paragraph stepPara = CreateRTLParagraph($"{i + 1}. {ProcessHebrewText(instructions[i])}", normalFont);
                Paragraph stepPara = CreateRTLParagraph($"{i + 1}. {instructions[i]}", normalFont);
                stepPara.SpacingAfter = 8f;
                stepPara.FirstLineIndent = -20f;
                stepPara.IndentationLeft = 20f;
                document.Add(stepPara);
            }

            // Temperature guide (RTL)
            if (!string.IsNullOrEmpty(recipe.TemperatureGuide))
            {
                Paragraph tempHeader = CreateRTLParagraph("🌡️ " + "מדריך טמפרטורה", subHeaderFont);
                tempHeader.SpacingAfter = 8f;
                document.Add(tempHeader);

                Paragraph tempPara = CreateRTLParagraph(recipe.TemperatureGuide, normalFont);
                tempPara.SpacingAfter = 15f;
                document.Add(tempPara);
            }

            // Cooking tips (RTL)
            Paragraph tipsHeader = CreateRTLParagraph("💡 " + "טיפ מקצועי על החתיכה הזו", subHeaderFont);
            tipsHeader.SpacingAfter = 8f;
            document.Add(tipsHeader);


            Paragraph tipsPara = new Paragraph(ProcessHebrewLongText2(recipe.Notes), normalFont);
            tipsPara.Alignment = Element.ALIGN_RIGHT;
            tipsPara.SpacingAfter = 15f;
            document.Add(tipsPara);





            // Footer (RTL)
            //Paragraph footer = CreateRTLParagraph("🤖 " + ProcessHebrewText("נוצר עם MEAT POINT AI Chef") + "\nCo-Authored-By: Claude\n" + ProcessHebrewText("נוצר ב ") + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), smallFont, Element.ALIGN_CENTER);
            var footerTXT = "🤖 " + ProcessHebrewText("נוצר עם MEAT POINT AI Chef") + "\nCo-Authored-By: Claude\n"  + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ProcessHebrewText("נוצר ב ");
            footerTXT = ProcessHebrewText(footerTXT);
            Paragraph footer = CreateRTLParagraph(footerTXT, smallFont, Element.ALIGN_CENTER);
            document.Add(footer);
        }

        private static void AddHebrewInfoCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(ProcessHebrewText(text), font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = Rectangle.NO_BORDER;
            cell.Padding = 5f;
            cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            table.AddCell(cell);
        }

        private static string GetCookingTimeDisplayHebrew(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} " + ProcessHebrewText("דקות");
            else
            {
                int hours = minutes / 60;
                int remainingMinutes = minutes % 60;
                return remainingMinutes > 0 
                    ? $"{hours} " + ProcessHebrewText("שעות") + $" {remainingMinutes} " + ProcessHebrewText("דקות")
                    : $"{hours} " + ProcessHebrewText("שעה") + (hours > 1 ? ProcessHebrewText("ות") : "");
            }
        }

        private static string GetComplexityStars(int level)
        {
            return new string('★', level) + new string('☆', 5 - level);
        }

        private static string GetComplexityLabelHebrew(int level)
        {
            switch (level)
            {
                case 1: return "🤷‍♂️ " + ProcessHebrewText("אבא טיפש");
                case 2: return "👨‍🍳 " + ProcessHebrewText("טירון במטבח");
                case 3: return "🏠 " + ProcessHebrewText("בשלן הבית");
                case 4: return "👩‍🍳 " + ProcessHebrewText("שף מיומן");
                case 5: return "⭐ " + ProcessHebrewText("אמא שף מדהימה");
                default: return ProcessHebrewText("רמה לא ידועה");
            }
        }

        private static string TranslateCategoryToHebrew(string category)
        {
            switch (category?.ToLower())
            {
                case "meat": return ProcessHebrewText("בשר");
                case "vegetables": return ProcessHebrewText("ירקות");
                case "seasonings": return ProcessHebrewText("תיבול");
                case "pantry": return ProcessHebrewText("מזווה");
                case "dairy": return ProcessHebrewText("מוצרי חלב");
                case "spices": return ProcessHebrewText("תבלינים");
                default: return category ?? ProcessHebrewText("אחר");
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

        private static string GetEquipmentForCookingMethodHebrew(string cookingMethod)
        {
            switch (cookingMethod?.ToLower())
            {
                case "grilling":
                    return  ProcessHebrewText("גריל (גז או פחם)") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("מלקחיים") + " • \n" + ProcessHebrewText("מברשת לגריל") + " •";
                case "pan-frying":
                    return ProcessHebrewText("מחבת כבדה או מחבת ברזל יצוק") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("מלקחיים או מרית") + " • \n" + ProcessHebrewText("נייר סופג") + " •"  ;
                case "oven":
                    return  ProcessHebrewText("תנור") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("כפפות תנור") + " • \n" + ProcessHebrewText("מלקחיים") + " •";
                case "roasting":
                    return  ProcessHebrewText("תבנית צלייה") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("נייר אלומיניום") + " • \n" + ProcessHebrewText("תנור") + " •";
                case "braising":
                    return  ProcessHebrewText("סיר כבד עם מכסה") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("מלקחיים") + " • \n" + ProcessHebrewText("מצקת") + " •";
                case "slow cooking":
                    return  ProcessHebrewText("סיר בישול איטי או סיר כבד") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("מלקחיים") + " • \n" + ProcessHebrewText("מצקת") + " •";
                case "broiling":
                    return  ProcessHebrewText("תבנית גריל") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("מלקחיים") + " • \n" + ProcessHebrewText("כפפות תנור") + " •";
                case "smoking":
                    return  ProcessHebrewText("מעשנה") + " • \n" + ProcessHebrewText("שבבי עץ") + " • \n" + ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("בקבוק ריסוס לחות") + " •";
                default:
                    return  ProcessHebrewText("מדחום לבשר") + " • \n" + ProcessHebrewText("כלי בישול מתאים לשיטה") + " • \n" + ProcessHebrewText("מלקחיים או מרית") + " • \n" + ProcessHebrewText("טיימר") + " •";
            }
        }
    }
}