import { NgModule } from '@angular/core';
import { AppComponent } from './components/General/AppComponent/app.component';
import { ComponentsModule } from './modules/components.module';
import { provideHttpClient, withInterceptorsFromDi, HttpClient } from '@angular/common/http';
import { HomePageComponent } from './components/Pages/home-page/home-page.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoginComponent } from './components/Pages/login/login.component';
import { RegisterComponent } from './components/Pages/register/register.component';
import { RecipeGeneratorComponent } from './components/Pages/recipe-generator/recipe-generator.component';
import { RecipeDisplayComponent } from './components/Pages/recipe-display/recipe-display.component';
import { UpgradeComponent } from './components/Pages/upgrade/upgrade.component';
import { ComplexitySliderComponent } from './components/General/complexity-slider/complexity-slider.component';

// Translation imports
import { TranslateModule, TranslateService, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

// Translation loader function
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/ngx-translate/', '.json');
}

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
  ],
  imports: [
    ComponentsModule,
    CommonModule,
    FormsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      },
      defaultLanguage: 'en'
    })
  ],
  providers: [provideHttpClient(withInterceptorsFromDi()), provideAnimationsAsync()],
  bootstrap: [AppComponent]
})
export class AppModule { }
