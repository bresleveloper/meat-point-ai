import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { BeefCut } from '../../../models/beef-cut.model';
import { BeefCutService } from '../../../services/simple-service.service';

@Component({
  selector: 'app-beef-cut-selector',
  templateUrl: './beef-cut-selector.component.html',
  styleUrls: ['./beef-cut-selector.component.css']
})
export class BeefCutSelectorComponent implements OnInit {
  @Input() selectedCutId?: number;
  @Output() cutSelected = new EventEmitter<BeefCut>();

  private beefCutService = inject(BeefCutService);
  
  beefCuts: BeefCut[] = [];
  filteredCuts: BeefCut[] = [];
  selectedCut?: BeefCut;
  filterBy: 'all' | 'tenderness' | 'complexity' = 'all';
  isLoading = true;

  ngOnInit(): void {
    this.loadBeefCuts();
  }

  private loadBeefCuts(): void {
    this.beefCutService.get().subscribe({
      next: (cuts: BeefCut[]) => {
        this.beefCuts = cuts.filter(cut => cut.IsActive);
        this.filteredCuts = [...this.beefCuts];
        
        if (this.selectedCutId) {
          this.selectedCut = this.beefCuts.find(cut => cut.BeefCutID === this.selectedCutId);
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading beef cuts:', error);
        this.isLoading = false;
      }
    });
  }

  selectCut(cut: BeefCut): void {
    this.selectedCut = cut;
    this.selectedCutId = cut.BeefCutID;
    this.cutSelected.emit(cut);
  }

  filterCuts(filterType: 'all' | 'tenderness' | 'complexity'): void {
    this.filterBy = filterType;
    
    switch (filterType) {
      case 'tenderness':
        this.filteredCuts = [...this.beefCuts].sort((a, b) => {
          const order = { 'Very Tender': 1, 'Tender': 2, 'Moderate': 3, 'Tough': 4 };
          return order[a.Tenderness] - order[b.Tenderness];
        });
        break;
      case 'complexity':
        this.filteredCuts = [...this.beefCuts].sort((a, b) => a.ComplexityLevel - b.ComplexityLevel);
        break;
      default:
        this.filteredCuts = [...this.beefCuts];
    }
  }

  getTendernessColor(tenderness: string): string {
    switch (tenderness) {
      case 'Very Tender': return '#90EE90';
      case 'Tender': return '#FFD700';
      case 'Moderate': return '#FFA500';
      case 'Tough': return '#FF6347';
      default: return '#DEB887';
    }
  }

  getComplexityStars(level: number): string[] {
    return Array(5).fill('').map((_, i) => i < level ? '★' : '☆');
  }

}