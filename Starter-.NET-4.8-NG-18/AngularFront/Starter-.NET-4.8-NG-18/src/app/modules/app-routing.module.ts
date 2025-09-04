import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomePageComponent } from '../components/Pages/home-page/home-page.component';
import { ExampleItemPageComponent } from '../components/Pages/example-item-page/example-item-page.component';


import { LoginComponent } from '../components/Pages/login/login.component';
import { RegisterComponent } from '../components/Pages/register/register.component';
import { RecipeGeneratorComponent } from '../components/Pages/recipe-generator/recipe-generator.component';
import { RecipeDisplayComponent } from '../components/Pages/recipe-display/recipe-display.component';
import { UpgradeComponent } from '../components/Pages/upgrade/upgrade.component';


  const routes: Routes = [
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'recipe-generator', component: RecipeGeneratorComponent },
    { path: 'recipe/:id', component: RecipeDisplayComponent },
    { path: 'upgrade', component: UpgradeComponent },
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
