import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap, filter, take } from 'rxjs';
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
  
  private authInitializedSubject = new BehaviorSubject<boolean>(false);
  public authInitialized$ = this.authInitializedSubject.asObservable();

  private readonly TOKEN_KEY = 'auth_token';

  constructor() {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    console.log('🔐 AuthService: Starting initialization...');
    const token = this.getToken();
    console.log('🔐 AuthService: Token exists?', !!token);
    
    this.checkAuthStatus().subscribe({
      next: (response) => {
        console.log('🔐 AuthService: Auth check response:', response);
        console.log('🔐 AuthService: User authenticated?', response.Success);
        this.authInitializedSubject.next(true);
      },
      error: (error) => {
        console.error('🔐 AuthService: Auth initialization error:', error);
        this.clearCurrentUser();
        this.removeToken();
        this.authInitializedSubject.next(true);
      }
    });
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

  private checkAuthStatus(): Observable<AuthResponse> {
    const token = this.getToken();
    console.log('🔐 AuthService: checkAuthStatus - Token:', token?.substring(0, 20) + '...');
    
    if (!token) {
      console.log('🔐 AuthService: No token found, clearing user');
      this.clearCurrentUser();
      return new Observable(observer => {
        observer.next({ Success: false, Message: 'No token found' });
        observer.complete();
      });
    }

    console.log('🔐 AuthService: Making CheckAuth API call...');
    return this.http.get<AuthResponse>(`${this.baseUrl}/CheckAuth`, { headers: this.getAuthHeaders() })
      .pipe(
        tap(response => {
          console.log('🔐 AuthService: CheckAuth API response:', response);
          if (response.Success && response.User) {
            console.log('🔐 AuthService: Setting user as authenticated:', response.User.Email);
            this.setCurrentUser(response.User);
          } else {
            console.log('🔐 AuthService: Auth failed, clearing user and token');
            this.clearCurrentUser();
            this.removeToken();
          }
        })
      );
  }

  private setCurrentUser(user: User): void {
    console.log('🔐 AuthService: Setting current user:', user.Email);
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private clearCurrentUser(): void {
    console.log('🔐 AuthService: Clearing current user');
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

  isAuthInitialized(): boolean {
    return this.authInitializedSubject.value;
  }

  waitForAuthInitialization(): Observable<boolean> {
    return this.authInitialized$.pipe(
      filter(initialized => initialized),
      take(1)
    );
  }

  isPremiumUser(): boolean {
    const user = this.getCurrentUser();
    return user?.PlanStatus === 'Premium' && 
           user.RenewalDate ? new Date(user.RenewalDate) > new Date() : false;
  }
}