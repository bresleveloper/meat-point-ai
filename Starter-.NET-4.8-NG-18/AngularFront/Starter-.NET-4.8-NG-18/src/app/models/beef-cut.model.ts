export interface BeefCut {
  BeefCutID: number;
  Name: string;
  Description: string;
  CowBodyLocation: string;
  Tenderness: 'Very Tender' | 'Tender' | 'Moderate' | 'Tough';
  MarblingLevel: 'High' | 'Medium' | 'Low';
  BestCookingMethods: string;
  ComplexityLevel: number; // 1-5
  ImageUrl?: string;
  CookingTips: string;
  TemperatureGuidelines: string;
  IsActive: boolean;
}