import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-complexity-slider',
  templateUrl: './complexity-slider.component.html',
  styleUrls: ['./complexity-slider.component.css']
})
export class ComplexitySliderComponent {
  @Input() value: number = 3;
  @Output() valueChange = new EventEmitter<number>();

  constructor(private translate: TranslateService) {}

  get complexityLevels() {
    return [
      { 
        level: 1, 
        label: '🤷‍♂️ ' + this.translate.instant('COMPLEXITY_SLIDER.LEVEL_1'), 
        description: this.translate.instant('COMPLEXITY_SLIDER.LEVEL_1_DESC') 
      },
      { 
        level: 2, 
        label: '👨‍🍳 ' + this.translate.instant('COMPLEXITY_SLIDER.LEVEL_2'), 
        description: this.translate.instant('COMPLEXITY_SLIDER.LEVEL_2_DESC') 
      },
      { 
        level: 3, 
        label: '🏠 ' + this.translate.instant('COMPLEXITY_SLIDER.LEVEL_3'), 
        description: this.translate.instant('COMPLEXITY_SLIDER.LEVEL_3_DESC') 
      },
      { 
        level: 4, 
        label: '👩‍🍳 ' + this.translate.instant('COMPLEXITY_SLIDER.LEVEL_4'), 
        description: this.translate.instant('COMPLEXITY_SLIDER.LEVEL_4_DESC') 
      },
      { 
        level: 5, 
        label: '⭐ ' + this.translate.instant('COMPLEXITY_SLIDER.LEVEL_5'), 
        description: this.translate.instant('COMPLEXITY_SLIDER.LEVEL_5_DESC') 
      }
    ];
  }

  onSliderChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    const newValue = parseInt(target.value);
    this.value = newValue;
    this.valueChange.emit(newValue);
  }

  getCurrentLevel() {
    return this.complexityLevels.find(level => level.level === this.value) || this.complexityLevels[2];
  }
}