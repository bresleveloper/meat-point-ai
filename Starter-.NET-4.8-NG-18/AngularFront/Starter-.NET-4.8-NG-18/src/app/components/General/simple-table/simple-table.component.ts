import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';

@Component({
  selector: 'simple-table',
  templateUrl: './simple-table.component.html',
  styleUrl: './simple-table.component.css'
})
export class SimpleTableComponent implements OnChanges {

  
  @Input() items
  cols:string[]
  @Input() title:string

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.cols && changes['items'] && changes['items'].currentValue 
        && changes['items'].currentValue.length
        && changes['items'].currentValue.length > 0) {
      this.cols = Object.keys(this.items[0])
    }
  }
}
