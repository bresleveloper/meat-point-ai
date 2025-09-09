import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
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
  private translate = inject(TranslateService);

  // Form data
  recipeRequest: RecipeGenerationRequest = {
    ComplexityLevel: 3,
    NumberOfDiners: 4,
    DinerAges: '',
    CookingMethod: '',
    CookingTimeMinutes: this.getCookingTimeForComplexity(3),
    DietaryRestrictions: '',
    UserPrompt: ''
  };

  // State
  isGenerating = false;
  errorMessage = '';
  userUsage: any = null;

  // Options
  get cookingMethods() {
    return [
      { value: 'Grilling', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.GRILLING') },
      { value: 'Pan-frying', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.PAN_FRYING') },
      { value: 'Oven', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.OVEN') },
      { value: 'Roasting', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.ROASTING') },
      { value: 'Braising', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.BRAISING') },
      { value: 'Slow cooking', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.SLOW_COOKING') },
      { value: 'Broiling', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.BROILING') },
      { value: 'Smoking', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.SMOKING') },
      { value: 'Stir-frying', label: this.translate.instant('RECIPE_GENERATOR.COOKING_METHODS.STIR_FRYING') }
    ];
  }

  get cookingTimes() {
    return [
      { label: this.translate.instant('RECIPE_GENERATOR.COOKING_TIMES.15_MINUTES'), value: 15 },
      { label: this.translate.instant('RECIPE_GENERATOR.COOKING_TIMES.30_MINUTES'), value: 30 },
      { label: this.translate.instant('RECIPE_GENERATOR.COOKING_TIMES.1_HOUR'), value: 60 },
      { label: this.translate.instant('RECIPE_GENERATOR.COOKING_TIMES.2_HOURS'), value: 120 },
      { label: this.translate.instant('RECIPE_GENERATOR.COOKING_TIMES.3_PLUS_HOURS'), value: 180 }
    ];
  }

  get dietaryOptions() {
    return [
      { value: 'Low-carb', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.LOW_CARB') },
      { value: 'Gluten-free', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.GLUTEN_FREE') },
      { value: 'Dairy-free', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.DAIRY_FREE') },
      { value: 'Keto', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.KETO') },
      { value: 'Paleo', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.PALEO') },
      { value: 'No restrictions', label: this.translate.instant('RECIPE_GENERATOR.DIETARY_OPTIONS.NO_RESTRICTIONS') }
    ];
  }

  get dinerAgeOptions() {
    return [
      { label: this.translate.instant('RECIPE_GENERATOR.AGE_OPTIONS.ADULTS_ONLY'), value: 'Adult' },
      { label: this.translate.instant('RECIPE_GENERATOR.AGE_OPTIONS.ADULTS_KIDS'), value: 'Adult,Child' },
      { label: this.translate.instant('RECIPE_GENERATOR.AGE_OPTIONS.ADULTS_TEENS'), value: 'Adult,Teen' },
      { label: this.translate.instant('RECIPE_GENERATOR.AGE_OPTIONS.MIXED_AGES'), value: 'Adult,Teen,Child' }
    ];
  }

  ngOnInit(): void {
    console.log('📝 RecipeGenerator: Component initialized');
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
    this.recipeRequest.CookingTimeMinutes = this.getCookingTimeForComplexity(level);
  }

  private getCookingTimeForComplexity(level: number): number {
    const timeMapping = {
      1: 15,   // Stupid Dad - quick & easy
      2: 30,   // Kitchen Newbie - simple recipes
      3: 60,   // Home Cook - standard recipes
      4: 120,  // Skilled Chef - complex techniques
      5: 180   // Super Chef Mom - elaborate dishes
    };
    return timeMapping[level as keyof typeof timeMapping] || 60;
  }


  canGenerate(): boolean {
    return this.recipeRequest.CookingMethod &&
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