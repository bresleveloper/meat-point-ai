export interface Recipe {
  RecipeID: number;
  UserID: number;
  BeefCutID: number;
  Title: string;
  Description: string;
  ComplexityLevel: number; // 1-5
  NumberOfDiners: number;
  DinerAges: string;
  CookingMethod: string;
  CookingTimeMinutes: number;
  DietaryRestrictions: string;
  Ingredients: string; // JSON string
  Instructions: string; // JSON string
  TemperatureGuide: string;
  ShoppingList: string; // JSON string
  UserPrompt?: string;
  CreatedDate: Date;
  IsFavorite: boolean;
  Rating?: number;
  Notes?: string;
}

export interface RecipeIngredient {
  item: string;
  quantity: string;
  category: 'Meat' | 'Vegetables' | 'Seasonings' | 'Pantry';
}

export interface ShoppingListItem {
  item: string;
  category: string;
  notes?: string;
}

export interface RecipeGenerationRequest {
  BeefCutID: number;
  ComplexityLevel: number;
  NumberOfDiners: number;
  DinerAges?: string;
  CookingMethod: string;
  CookingTimeMinutes: number;
  DietaryRestrictions?: string;
  UserPrompt?: string;
}

export interface RecipeGenerationResponse {
  Success: boolean;
  Message: string;
  Recipe?: Recipe;
}