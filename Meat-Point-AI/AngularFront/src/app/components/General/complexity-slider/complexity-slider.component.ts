import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-complexity-slider',
  templateUrl: './complexity-slider.component.html',
  styleUrls: ['./complexity-slider.component.css']
})
export class ComplexitySliderComponent {
  @Input() value: number = 3;
  @Output() valueChange = new EventEmitter<number>();

  complexityLevels = [
    { level: 1, label: '🤷‍♂️ Stupid Dad', description: 'Just don\'t burn it' },
    { level: 2, label: '👨‍🍳 Kitchen Newbie', description: 'Basic cooking skills' },
    { level: 3, label: '🏠 Home Cook', description: 'Comfortable in kitchen' },
    { level: 4, label: '👩‍🍳 Skilled Chef', description: 'Advanced techniques' },
    { level: 5, label: '⭐ Super Chef Mom', description: 'Master level cooking' }
  ];

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