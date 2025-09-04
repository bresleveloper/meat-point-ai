-- Beef Cuts Seed Data
INSERT INTO BeefCuts (Name, Description, CowBodyLocation, Tenderness, MarblingLevel, BestCookingMethods, ComplexityLevel, CookingTips, TemperatureGuidelines) VALUES

-- Tender Cuts (Low Complexity)
('Filet Mignon', 'The most tender cut from the tenderloin, virtually no connective tissue. Perfect for beginners.', 'Short Loin', 'Very Tender', 'Low', 'Pan-frying,Grilling', 1, 'Season simply with salt and pepper. Cook quickly over high heat. Don''t overcook - medium rare is perfect.', 'Rare: 120-125°F, Medium Rare: 130-135°F, Medium: 135-145°F'),

('Ribeye Steak', 'Well-marbled steak with excellent flavor. Forgiving cut that''s hard to mess up.', 'Rib', 'Very Tender', 'High', 'Grilling,Pan-frying,Broiling', 2, 'Let it come to room temperature before cooking. The marbling will keep it juicy even if slightly overcooked.', 'Rare: 120-125°F, Medium Rare: 130-135°F, Medium: 135-145°F'),

('New York Strip', 'Tender with good beefy flavor. Less marbled than ribeye but still forgiving.', 'Short Loin', 'Tender', 'Medium', 'Grilling,Pan-frying,Broiling', 2, 'Great for grilling. Create a good sear first, then finish cooking. Rest for 5 minutes after cooking.', 'Rare: 120-125°F, Medium Rare: 130-135°F, Medium: 135-145°F'),

-- Medium Complexity Cuts
('Sirloin Steak', 'Flavorful and moderately tender. Good value cut that requires some technique.', 'Sirloin', 'Moderate', 'Low', 'Grilling,Pan-frying,Broiling', 3, 'Don''t overcook - becomes tough quickly. Marinate for extra tenderness. Cut against the grain.', 'Medium Rare: 130-135°F, Medium: 135-145°F (don''t go beyond medium)'),

('Flank Steak', 'Lean cut with strong beefy flavor. Must be cooked and sliced properly.', 'Flank', 'Moderate', 'Low', 'Grilling,Broiling,Stir-frying', 3, 'Marinate for 2+ hours. Cook hot and fast. MUST slice very thin against the grain or it will be tough.', 'Medium Rare: 130-135°F (do not cook beyond medium)'),

('Skirt Steak', 'Very flavorful but requires proper technique. Great for fajitas when done right.', 'Plate', 'Moderate', 'Medium', 'Grilling,Pan-frying', 3, 'Cook very hot and very fast (2-3 minutes per side). Let rest, then slice thin against the grain.', 'Medium Rare: 130-135°F (cooks very quickly)'),

-- Tougher Cuts Requiring Skill
('Chuck Roast', 'Tough cut that becomes incredibly tender when cooked low and slow. Requires patience.', 'Chuck', 'Tough', 'Medium', 'Braising,Slow cooking,Roasting', 4, 'Brown all sides first for flavor. Cook low and slow with moisture. Internal temp of 200°F+ for fall-apart tender.', 'Braising: Cook until 195-205°F for tenderness'),

('Brisket', 'The ultimate BBQ challenge. Requires 12+ hours of low, slow cooking to break down connective tissue.', 'Brisket', 'Tough', 'Medium', 'Smoking,Braising', 5, 'Season 12+ hours ahead. Cook at 225-250°F. Wrap in foil at 160°F internal. Don''t rush - it''s done when it''s done.', 'Smoking: 195-205°F internal (probe tender)'),

('Short Ribs', 'Incredibly flavorful but tough. Becomes melt-in-your-mouth when braised properly.', 'Chuck', 'Tough', 'High', 'Braising,Slow cooking', 4, 'Brown first for color and flavor. Braise in liquid for 2-3 hours at 325°F until fork tender.', 'Braising: 190-200°F internal (fall-off-bone tender)'),

-- Specialty Cuts
('Tri-tip', 'California favorite. Triangle-shaped cut that''s tender when sliced correctly.', 'Sirloin', 'Tender', 'Medium', 'Grilling,Roasting', 3, 'Season with salt, pepper, garlic. Grill over medium heat. Very important to slice against the grain.', 'Medium Rare: 130-135°F, Medium: 135-145°F'),

('Hanger Steak', 'Called "butcher''s steak" because butchers kept it for themselves. Very flavorful.', 'Plate', 'Tender', 'Medium', 'Grilling,Pan-frying', 3, 'Marinate for flavor. Cook quickly over high heat. Must slice against the grain. One per cow so it''s special!', 'Medium Rare: 130-135°F (don''t overcook)'),

('Round Roast', 'Very lean cut from the rear leg. Requires careful cooking to avoid dryness.', 'Round', 'Tough', 'Low', 'Slow cooking,Braising', 4, 'Very lean so it can dry out easily. Use moist heat cooking methods. Great for pot roast with vegetables.', 'Braising: 190-200°F internal for tenderness');