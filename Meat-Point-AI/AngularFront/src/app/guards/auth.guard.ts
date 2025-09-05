import { inject, Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    console.log('🛡️ AuthGuard: Checking access to:', state.url);
    
    return this.authService.waitForAuthInitialization().pipe(
      map(() => {
        const isAuthenticated = this.authService.isAuthenticated();
        console.log('🛡️ AuthGuard: User authenticated?', isAuthenticated);
        
        if (!isAuthenticated) {
          console.log('🛡️ AuthGuard: Redirecting to login');
          this.router.navigate(['/login'], { 
            queryParams: { returnUrl: state.url } 
          });
          return false;
        }
        
        console.log('🛡️ AuthGuard: Access granted to:', state.url);
        return true;
      })
    );
  }
}