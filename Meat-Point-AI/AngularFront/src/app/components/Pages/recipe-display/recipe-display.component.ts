import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Recipe, RecipeIngredient, ShoppingListItem } from '../../../models/recipe.model';
import { AuthService } from '../../../services/auth.service';
import { RecipeService } from '../../../services/simple-service.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-recipe-display',
  templateUrl: './recipe-display.component.html',
  styleUrls: ['./recipe-display.component.css']
})
export class RecipeDisplayComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private recipeService = inject(RecipeService);

  recipe?: Recipe;
  ingredients: RecipeIngredient[] = [];
  instructions: string[] = [];
  shoppingList: ShoppingListItem[] = [];
  
  isLoading = true;
  errorMessage = '';
  currentView: 'recipe' | 'shopping' = 'recipe';

  ngOnInit(): void {
    console.log('🍽️ RecipeDisplay: Component initialized');
    
    const recipeId = this.route.snapshot.paramMap.get('id');
    if (recipeId) {
      this.loadRecipe(parseInt(recipeId));
    } else {
      this.errorMessage = 'Recipe not found';
      this.isLoading = false;
    }
  }

  private loadRecipe(recipeId: number): void {
    this.recipeService.get(recipeId).subscribe({
      next: (recipe: Recipe) => {
        this.recipe = recipe;
        this.parseRecipeData();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading recipe:', error);
        this.errorMessage = 'Failed to load recipe';
        this.isLoading = false;
      }
    });
  }


  private parseRecipeData(): void {
    if (!this.recipe) return;

    console.log('=== Recipe Data Parsing Debug ===');
    console.log('Full recipe object:', this.recipe);

    try {
      // Parse ingredients
      console.log('Raw Ingredients string:', this.recipe.Ingredients);
      if (this.recipe.Ingredients && this.recipe.Ingredients.trim() !== '') {
        this.ingredients = JSON.parse(JSON.parse(this.recipe.Ingredients));
        console.log('Parsed ingredients:', this.ingredients);
        console.log('Ingredients array length:', this.ingredients.length);
      } else {
        console.warn('Ingredients string is empty or null');
        this.ingredients = [];
      }

      // Parse instructions
      console.log('Raw Instructions string:', this.recipe.Instructions);
      if (this.recipe.Instructions && this.recipe.Instructions.trim() !== '') {
        this.instructions = JSON.parse(JSON.parse(this.recipe.Instructions));
        console.log('Parsed instructions:', this.instructions);
        console.log('Instructions array length:', this.instructions.length);
      } else {
        console.warn('Instructions string is empty or null');
        this.instructions = [];
      }

      // Parse shopping list
      console.log('Raw ShoppingList string:', this.recipe.ShoppingList);
      if (this.recipe.ShoppingList && this.recipe.ShoppingList.trim() !== '') {
        this.shoppingList = JSON.parse(JSON.parse(this.recipe.ShoppingList));
        console.log('Parsed shopping list:', this.shoppingList);
        console.log('Shopping list array length:', this.shoppingList.length);
      } else {
        console.warn('ShoppingList string is empty or null');
        this.shoppingList = [];
      }

    } catch (error) {
      console.error('Error parsing recipe data:', error);
      console.error('Error details:', {
        ingredients: this.recipe.Ingredients,
        instructions: this.recipe.Instructions,
        shoppingList: this.recipe.ShoppingList
      });
      // Initialize with empty arrays on error
      this.ingredients = [];
      this.instructions = [];
      this.shoppingList = [];
    }

    console.log('=== Final parsed arrays ===');
    console.log('Final ingredients:', this.ingredients);
    console.log('Final instructions:', this.instructions);
    console.log('Final shopping list:', this.shoppingList);
  }

  getComplexityStars(level: number): string[] {
    return Array(5).fill('').map((_, i) => i < level ? '★' : '☆');
  }

  getComplexityLabel(level: number): string {
    const labels = {
      1: '🤷‍♂️ Stupid Dad',
      2: '👨‍🍳 Kitchen Newbie', 
      3: '🏠 Home Cook',
      4: '👩‍🍳 Skilled Chef',
      5: '⭐ Super Chef Mom'
    };
    return labels[level as keyof typeof labels] || 'Unknown Level';
  }

  getCookingTimeDisplay(): string {
    if (!this.recipe) return '';
    
    const minutes = this.recipe.CookingTimeMinutes;
    if (minutes < 60) {
      return `${minutes} minutes`;
    } else {
      const hours = Math.floor(minutes / 60);
      const remainingMinutes = minutes % 60;
      return remainingMinutes > 0 ? `${hours}h ${remainingMinutes}m` : `${hours} hour${hours > 1 ? 's' : ''}`;
    }
  }

  getDinerAgesDisplay(): string {
    if (!this.recipe?.DinerAges) return '';
    
    return this.recipe.DinerAges.split(',')
      .map(age => age.trim())
      .join(', ')
      .replace('Adult', 'Adults')
      .replace('Child', 'Kids')
      .replace('Teen', 'Teens');
  }

  getIngredientsByCategory(): { [category: string]: RecipeIngredient[] } {
    return this.ingredients.reduce((acc, ingredient) => {
      const category = ingredient.category || 'Other';
      if (!acc[category]) {
        acc[category] = [];
      }
      acc[category].push(ingredient);
      return acc;
    }, {} as { [category: string]: RecipeIngredient[] });
  }

  getShoppingListByCategory(): { [category: string]: ShoppingListItem[] } {
    return this.shoppingList.reduce((acc, item) => {
      const category = item.category || 'Other';
      if (!acc[category]) {
        acc[category] = [];
      }
      acc[category].push(item);
      return acc;
    }, {} as { [category: string]: ShoppingListItem[] });
  }

  switchView(view: 'recipe' | 'shopping'): void {
    this.currentView = view;
  }

  printPDF(): void {
    if (!this.recipe) return;

    const pdfUrl = `${environment.serverApiUrl}/pdf/generate/${this.recipe.RecipeID}`;
    const headers = this.authService.getAuthorizationHeader();

    this.http.get(pdfUrl, { 
      headers: headers, 
      responseType: 'blob' 
    }).subscribe({
      next: (blob) => {
        // Create blob URL and trigger download
        const blobUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = `BeefMaster-${this.recipe!.Title.replace(/[^a-zA-Z0-9]/g, '-')}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Clean up the blob URL
        window.URL.revokeObjectURL(blobUrl);
      },
      error: (error) => {
        console.error('PDF download error:', error);
        if (error.status === 401) {
          this.errorMessage = 'Authentication required. Please log in again.';
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = 'Failed to download PDF. Please try again.';
        }
      }
    });
  }

  toggleFavorite(): void {
    if (!this.recipe) return;
    
    const updatedRecipe = { ...this.recipe, IsFavorite: !this.recipe.IsFavorite };
    this.recipeService.update(this.recipe.RecipeID, updatedRecipe).subscribe({
      next: () => {
        this.recipe!.IsFavorite = !this.recipe!.IsFavorite;
      },
      error: (error) => {
        console.error('Error updating favorite status:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/recipe-generator']);
  }
}