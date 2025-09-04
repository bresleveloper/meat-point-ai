import { NgModule } from '@angular/core';
import { AppComponent } from './components/General/AppComponent/app.component';
import { ComponentsModule } from './modules/components.module';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HomePageComponent } from './components/Pages/home-page/home-page.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoginComponent } from './components/Pages/login/login.component';
import { RegisterComponent } from './components/Pages/register/register.component';
import { RecipeGeneratorComponent } from './components/Pages/recipe-generator/recipe-generator.component';
import { RecipeDisplayComponent } from './components/Pages/recipe-display/recipe-display.component';
import { UpgradeComponent } from './components/Pages/upgrade/upgrade.component';
import { BeefCutSelectorComponent } from './components/General/beef-cut-selector/beef-cut-selector.component';
import { ComplexitySliderComponent } from './components/General/complexity-slider/complexity-slider.component';

@NgModule({
  declarations: [
    HomePageComponent,
    // Existing components...
    LoginComponent,
    RegisterComponent,
    RecipeGeneratorComponent,
    RecipeDisplayComponent,
    UpgradeComponent,
    ComplexitySliderComponent,
    BeefCutSelectorComponent
  ],
  imports: [
    ComponentsModule,
    CommonModule,
    FormsModule
  ],
  providers: [provideHttpClient(withInterceptorsFromDi()), provideAnimationsAsync()],
  bootstrap: [AppComponent]
})
export class AppModule { }
