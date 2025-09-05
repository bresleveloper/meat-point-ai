import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, filter, Observable } from 'rxjs';
import { ExampleItem } from '../models/example-item.model';
import { environment } from '../../environments/environment';

class SimpleService<T> {
  protected http: HttpClient = inject(HttpClient)

  private itemsCache:BehaviorSubject<T[]> = new BehaviorSubject<T[]>([])
  public items:Observable<T[]> = this.itemsCache.pipe(filter(arr=>arr && arr.length > 0))

  constructor(protected baseUrl:string) {
    this.baseUrl = `${environment.serverApiUrl}/${baseUrl}`
    //this.itemsCache.subscribe(c=>console.log("itemsCache", c))
    this.get().subscribe(items=>this.itemsCache.next(items as T[]))
  }

  protected jsonHeadersOptions = {
    headers:{
      "Content-Type":"application/json",
      "Accept":"application/json",
    }
  }

  get(id?:number):Observable<T | T[]>{
    return this.http.get<T | T[]>(this.baseUrl + (id? `/${id}` : ""));
  }

  add(item:T){
    return this.http.post(this.baseUrl, item);
  } 

  update(id:number, item:T){
    //console.log("inner update");
    return this.http.put(`${this.baseUrl}/${id}`, item);
  } 

  delete(id:number){
    return this.http.delete(`${this.baseUrl}/${id}`);
  } 
}


@Injectable({ providedIn: 'root' })
export class ExampleItemService extends SimpleService<ExampleItem> {
  constructor() { super("ExampleItem") }
}

// Beef Meal Planning Services

@Injectable({ providedIn: 'root' })
export class RecipeService extends SimpleService<any> {
  constructor() { super("Recipe") }
}

@Injectable({ providedIn: 'root' })
export class MealPlanService extends SimpleService<any> {
  constructor() { super("MealPlan") }
}




