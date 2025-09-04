import { Component } from '@angular/core';
import { ExampleItem } from '../../../../models/example-item.model';
import { ExampleItemService } from '../../../../services/simple-service.service';

@Component({
  selector: 'app-example-item',
  templateUrl: './example-item.component.html',
  styleUrl: './example-item.component.css'
})
export class ExampleItemComponent {
  
  items:ExampleItem[]

  constructor(private svc:ExampleItemService) {
    svc.items.subscribe(items=>this.items = items)
    //svc.items.subscribe(items=> console.log("new items", items))
  }

}
