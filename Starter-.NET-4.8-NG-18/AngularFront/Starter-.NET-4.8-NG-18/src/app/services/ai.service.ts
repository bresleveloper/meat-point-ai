import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RecipeGenerationRequest, RecipeGenerationResponse } from '../models/recipe.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AIService {
  private http: HttpClient = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = `${environment.serverApiUrl}/AI`;

  generateRecipe(request: RecipeGenerationRequest): Observable<RecipeGenerationResponse> {
    return this.http.post<RecipeGenerationResponse>(`${this.baseUrl}/GenerateRecipe`, request, {
      headers: this.authService.getAuthorizationHeader()
    });
  }

  getUserUsage(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/GetUserUsage`, {
      headers: this.authService.getAuthorizationHeader()
    });
  }
}