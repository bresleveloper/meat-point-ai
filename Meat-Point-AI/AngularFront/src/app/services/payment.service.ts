import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SubscriptionResponse {
  Success: boolean;
  Message: string;
  SubscriptionId?: string;
  NextBillingDate?: Date;
}

export interface PaymentMethodRequest {
  paymentMethodId: string;
}

export interface StripeConfig {
  success: boolean;
  publishableKey: string;
  priceMonthly: number;
  currency: string;
}

export interface SubscriptionStatus {
  Success: boolean;
  PlanStatus: string;
  PremiumStartDate?: Date;
  RenewalDate?: Date;
  IsActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http: HttpClient = inject(HttpClient);
  private baseUrl = `${environment.serverApiUrl}/Payment`;

  subscribe(request: PaymentMethodRequest): Observable<SubscriptionResponse> {
    return this.http.post<SubscriptionResponse>(`${this.baseUrl}/Subscribe`, request);
  }

  cancelSubscription(): Observable<SubscriptionResponse> {
    return this.http.post<SubscriptionResponse>(`${this.baseUrl}/CancelSubscription`, {});
  }

  getStripeConfig(): Observable<StripeConfig> {
    return this.http.get<StripeConfig>(`${this.baseUrl}/GetStripeConfig`);
  }

  getSubscriptionStatus(): Observable<SubscriptionStatus> {
    return this.http.get<SubscriptionStatus>(`${this.baseUrl}/GetSubscriptionStatus`);
  }
}