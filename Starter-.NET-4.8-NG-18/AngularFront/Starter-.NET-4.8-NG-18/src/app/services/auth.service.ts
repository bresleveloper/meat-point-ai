import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http: HttpClient = inject(HttpClient);
  private baseUrl = `${environment.serverApiUrl}/Auth`;
  
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private readonly TOKEN_KEY = 'auth_token';

  constructor() {
    this.checkAuthStatus();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/Login`, request)
      .pipe(
        tap(response => {
          if (response.Success && response.User && response.Token) {
            this.setCurrentUser(response.User);
            this.setToken(response.Token);
          }
        })
      );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/Register`, request)
      .pipe(
        tap(response => {
          if (response.Success && response.User && response.Token) {
            this.setCurrentUser(response.User);
            this.setToken(response.Token);
          }
        })
      );
  }

  logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/Logout`, {}, { headers: this.getAuthHeaders() })
      .pipe(
        tap(response => {
          if (response.Success) {
            this.clearCurrentUser();
            this.removeToken();
          }
        })
      );
  }

  checkAuthStatus(): Observable<AuthResponse> {
    const token = this.getToken();
    if (!token) {
      this.clearCurrentUser();
      return new Observable(observer => {
        observer.next({ Success: false, Message: 'No token found' });
        observer.complete();
      });
    }

    return this.http.get<AuthResponse>(`${this.baseUrl}/CheckAuth`, { headers: this.getAuthHeaders() })
      .pipe(
        tap(response => {
          if (response.Success && response.User) {
            this.setCurrentUser(response.User);
          } else {
            this.clearCurrentUser();
            this.removeToken();
          }
        })
      );
  }

  private setCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private clearCurrentUser(): void {
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    if (token) {
      return new HttpHeaders().set('Authorization', `Bearer ${token}`);
    }
    return new HttpHeaders();
  }

  getAuthorizationHeader(): HttpHeaders {
    return this.getAuthHeaders();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  isPremiumUser(): boolean {
    const user = this.getCurrentUser();
    return user?.PlanStatus === 'Premium' && 
           user.RenewalDate ? new Date(user.RenewalDate) > new Date() : false;
  }
}