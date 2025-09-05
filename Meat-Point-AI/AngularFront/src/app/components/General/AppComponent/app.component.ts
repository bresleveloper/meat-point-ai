import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  
  title = 'meat-point-ai';
  isInitializing = true;

  ngOnInit(): void {
    // Handle initial routing based on authentication status
    this.authService.waitForAuthInitialization().subscribe(() => {
      this.isInitializing = false;
      
      // Only handle routing if we're on the root path
      if (this.router.url === '/' || this.router.url === '') {
        if (this.authService.isAuthenticated()) {
          this.router.navigate(['/recipe-generator']);
        } else {
          this.router.navigate(['/login']);
        }
      }
    });
  }
}
