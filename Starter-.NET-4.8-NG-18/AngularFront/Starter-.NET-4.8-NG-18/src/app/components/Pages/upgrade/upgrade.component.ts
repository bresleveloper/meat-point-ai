import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { PaymentService, PaymentMethodRequest, StripeConfig, SubscriptionStatus } from '../../../services/payment.service';

declare var Stripe: any;

@Component({
  selector: 'app-upgrade',
  templateUrl: './upgrade.component.html',
  styleUrls: ['./upgrade.component.css']
})
export class UpgradeComponent implements OnInit {
  private authService = inject(AuthService);
  private paymentService = inject(PaymentService);
  private router = inject(Router);

  stripeConfig?: StripeConfig;
  subscriptionStatus?: SubscriptionStatus;
  
  stripe: any;
  cardElement: any;
  isLoading = true;
  isProcessing = false;
  errorMessage = '';
  successMessage = '';

  // Demo payment method ID (in real app, this would come from Stripe Elements)
  demoPaymentMethodId = 'pm_card_visa_debit';

  ngOnInit(): void {
    // Check authentication
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadSubscriptionData();
  }

  private loadSubscriptionData(): void {
    // Load Stripe config and subscription status
    this.paymentService.getStripeConfig().subscribe({
      next: (config) => {
        this.stripeConfig = config;
        this.loadSubscriptionStatus();
      },
      error: (error) => {
        console.error('Error loading Stripe config:', error);
        this.errorMessage = 'Failed to load payment configuration';
        this.isLoading = false;
      }
    });
  }

  private loadSubscriptionStatus(): void {
    this.paymentService.getSubscriptionStatus().subscribe({
      next: (status) => {
        this.subscriptionStatus = status;
        this.isLoading = false;
        
        // If already premium, show success message
        if (status.IsActive) {
          this.successMessage = 'You already have an active Premium subscription!';
        }
      },
      error: (error) => {
        console.error('Error loading subscription status:', error);
        this.isLoading = false;
      }
    });
  }

  onUpgradeToPremium(): void {
    if (!this.stripeConfig || this.isProcessing) {
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';
    this.successMessage = '';

    // For demo purposes, we'll use a simulated payment method
    // In a real app, you would collect this from Stripe Elements
    const request: PaymentMethodRequest = {
      paymentMethodId: this.demoPaymentMethodId
    };

    this.paymentService.subscribe(request).subscribe({
      next: (response) => {
        this.isProcessing = false;
        if (response.Success) {
          this.successMessage = 'Welcome to Premium! You now have 30 recipes per day.';
          this.loadSubscriptionStatus(); // Refresh status
          
          // Redirect to recipe generator after a short delay
          setTimeout(() => {
            this.router.navigate(['/recipe-generator']);
          }, 2000);
        } else {
          this.errorMessage = response.Message;
        }
      },
      error: (error) => {
        this.isProcessing = false;
        this.errorMessage = 'An error occurred while processing your payment. Please try again.';
        console.error('Subscription error:', error);
      }
    });
  }

  onCancelSubscription(): void {
    if (!confirm('Are you sure you want to cancel your Premium subscription?')) {
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';

    this.paymentService.cancelSubscription().subscribe({
      next: (response) => {
        this.isProcessing = false;
        if (response.Success) {
          this.successMessage = 'Your Premium subscription has been cancelled.';
          this.loadSubscriptionStatus(); // Refresh status
        } else {
          this.errorMessage = response.Message;
        }
      },
      error: (error) => {
        this.isProcessing = false;
        this.errorMessage = 'An error occurred while cancelling your subscription.';
        console.error('Cancellation error:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/recipe-generator']);
  }

  getRenewalDateDisplay(): string {
    if (!this.subscriptionStatus?.RenewalDate) return '';
    
    const renewalDate = new Date(this.subscriptionStatus.RenewalDate);
    return renewalDate.toLocaleDateString();
  }
}