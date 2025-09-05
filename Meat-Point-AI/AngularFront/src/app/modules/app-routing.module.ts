import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomePageComponent } from '../components/Pages/home-page/home-page.component';
import { ExampleItemPageComponent } from '../components/Pages/example-item-page/example-item-page.component';


import { LoginComponent } from '../components/Pages/login/login.component';
import { RegisterComponent } from '../components/Pages/register/register.component';
import { RecipeGeneratorComponent } from '../components/Pages/recipe-generator/recipe-generator.component';
import { RecipeDisplayComponent } from '../components/Pages/recipe-display/recipe-display.component';
import { UpgradeComponent } from '../components/Pages/upgrade/upgrade.component';
import { AuthGuard } from '../guards/auth.guard';


  const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'recipe-generator', component: RecipeGeneratorComponent, canActivate: [AuthGuard] },
    { path: 'recipe/:id', component: RecipeDisplayComponent, canActivate: [AuthGuard] },
    { path: 'upgrade', component: UpgradeComponent, canActivate: [AuthGuard] },
    { path: '', redirectTo: '/recipe-generator', pathMatch: 'full' },
    { path: '**', redirectTo: '/recipe-generator' }
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
