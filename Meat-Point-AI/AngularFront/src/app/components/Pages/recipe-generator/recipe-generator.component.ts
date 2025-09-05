import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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