import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';

//import { MaterialModule } from './material.module';
import { AppComponent } from '../components/General/AppComponent/app.component';


import { SimpleTableComponent } from '../components/General/simple-table/simple-table.component';
import { ExampleItemPageComponent } from '../components/Pages/example-item-page/example-item-page.component';
import { ExampleItemComponent } from '../components/Pages/example-item-page/example-item/example-item.component';
import { AppRoutingModule } from './app-routing.module';


const allComps=[
  AppComponent,
  SimpleTableComponent,
  ExampleItemPageComponent,
  ExampleItemComponent,

]

@NgModule({
  declarations: allComps,
  exports: allComps,
  imports: [
    AppRoutingModule,
    ReactiveFormsModule,
    BrowserModule,
    //BrowserAnimationsModule,
    //MaterialModule,
    //AngularToastifyModule, //npm i angular-toastify

  ],
})
export class ComponentsModule { }
