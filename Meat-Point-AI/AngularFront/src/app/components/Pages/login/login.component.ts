import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../models/user.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  
  private returnUrl: string = '/recipe-generator';

  loginData: LoginRequest = {
    email: '',
    password: ''
  };

  isLoading = false;
  errorMessage = '';
  isCheckingAuth = true;

  ngOnInit(): void {
    // Get return URL from query params
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/recipe-generator';
    console.log('🔑 Login: Return URL set to:', this.returnUrl);
    
    // Check if user is already authenticated
    this.authService.waitForAuthInitialization().subscribe(() => {
      this.isCheckingAuth = false;
      
      if (this.authService.isAuthenticated()) {
        console.log('🔑 Login: User already authenticated, redirecting to:', this.returnUrl);
        this.router.navigate([this.returnUrl]);
      }
    });
  }

  onLogin(): void {
    if (!this.loginData.email || !this.loginData.password) {
      this.errorMessage = 'Please fill in all fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.Success) {
          console.log('🔑 Login: Login successful, redirecting to:', this.returnUrl);
          this.router.navigate([this.returnUrl]);
        } else {
          this.errorMessage = response.Message;
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'An error occurred during login';
        console.error('Login error:', error);
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}