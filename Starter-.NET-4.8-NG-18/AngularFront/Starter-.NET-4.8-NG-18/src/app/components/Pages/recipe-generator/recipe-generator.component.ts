import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BeefCut } from '../../../models/beef-cut.model';
import { RecipeGenerationRequest } from '../../../models/recipe.model';
import { AIService } from '../../../services/ai.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-recipe-generator',
  templateUrl: './recipe-generator.component.html',
  styleUrls: ['./recipe-generator.component.css']
})
export class RecipeGeneratorComponent implements OnInit {
  private aiService = inject(AIService);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Form data
  recipeRequest: RecipeGenerationRequest = {
    BeefCutID: 0,
    ComplexityLevel: 3,
    NumberOfDiners: 4,
    DinerAges: '',
    CookingMethod: '',
    CookingTimeMinutes: 60,
    DietaryRestrictions: '',
    UserPrompt: ''
  };

  // State
  selectedCut?: BeefCut;
  isGenerating = false;
  errorMessage = '';
  userUsage: any = null;

  // Options
  cookingMethods = [
    'Grilling', 'Pan-frying', 'Roasting', 'Braising', 
    'Slow cooking', 'Broiling', 'Smoking', 'Stir-frying'
  ];

  cookingTimes = [
    { label: '15 minutes', value: 15 },
    { label: '30 minutes', value: 30 },
    { label: '1 hour', value: 60 },
    { label: '2 hours', value: 120 },
    { label: '3+ hours', value: 180 }
  ];

  dietaryOptions = [
    'Low-carb', 'Gluten-free', 'Dairy-free', 'Keto', 
    'Paleo', 'No restrictions'
  ];

  dinerAgeOptions = [
    { label: 'Adults only', value: 'Adult' },
    { label: 'Adults + Kids', value: 'Adult,Child' },
    { label: 'Adults + Teens', value: 'Adult,Teen' },
    { label: 'Mixed ages', value: 'Adult,Teen,Child' }
  ];

  ngOnInit(): void {
    // Check authentication
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadUserUsage();
  }

  private loadUserUsage(): void {
    this.aiService.getUserUsage().subscribe({
      next: (usage) => {
        this.userUsage = usage;
      },
      error: (error) => {
        console.error('Error loading user usage:', error);
      }
    });
  }

  onComplexityChange(level: number): void {
    this.recipeRequest.ComplexityLevel = level;
  }

  onCutSelected(cut: BeefCut): void {
    this.selectedCut = cut;
    this.recipeRequest.BeefCutID = cut.BeefCutID;
    
    // Auto-select compatible cooking methods based on cut
    const bestMethods = cut.BestCookingMethods.split(',').map(m => m.trim());
    if (bestMethods.length > 0 && !this.recipeRequest.CookingMethod) {
      this.recipeRequest.CookingMethod = bestMethods[0];
    }
  }

  canGenerate(): boolean {
    return this.recipeRequest.BeefCutID > 0 && 
           this.recipeRequest.CookingMethod &&
           this.recipeRequest.NumberOfDiners > 0 &&
           this.userUsage?.CanGenerateMore &&
           !this.isGenerating;
  }

  onGenerate(): void {
    if (!this.canGenerate()) {
      return;
    }

    this.isGenerating = true;
    this.errorMessage = '';

    this.aiService.generateRecipe(this.recipeRequest).subscribe({
      next: (response) => {
        this.isGenerating = false;
        if (response.Success) {
          // Navigate to recipe display page
          this.router.navigate(['/recipe', response.Recipe?.RecipeID]);
        } else {
          this.errorMessage = response.Message;
        }
      },
      error: (error) => {
        this.isGenerating = false;
        this.errorMessage = 'An error occurred while generating the recipe. Please try again.';
        console.error('Recipe generation error:', error);
      }
    });
  }

  goToUpgrade(): void {
    this.router.navigate(['/upgrade']);
  }
}