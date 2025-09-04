export interface User {
  UserID: number;
  Email: string;
  PasswordHash?: string;
  CreatedDate: Date;
  PlanStatus: 'Free' | 'Premium';
  PremiumStartDate?: Date;
  RenewalDate?: Date;
  PaymentMethodId?: string;
  DailyUsageCount: number;
  LastUsageReset: Date;
  FirstName?: string;
  LastName?: string;
  IsActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthResponse {
  Success: boolean;
  Message: string;
  User?: User;
  Token?: string;
}