import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest } from '../../../models/user.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  registerData: RegisterRequest = {
    email: '',
    password: '',
    firstName: '',
    lastName: ''
  };

  confirmPassword = '';
  isLoading = false;
  errorMessage = '';

  onRegister(): void {
    if (!this.registerData.email || !this.registerData.password || 
        !this.registerData.firstName || !this.registerData.lastName) {
      this.errorMessage = 'Please fill in all fields';
      return;
    }

    if (this.registerData.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    if (this.registerData.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerData).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.Success) {
          this.router.navigate(['/recipe-generator']);
        } else {
          this.errorMessage = response.Message;
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'An error occurred during registration';
        console.error('Registration error:', error);
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}