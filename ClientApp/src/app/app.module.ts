import { GameComponent } from './_components/game/game.component';
import { HubService } from './_services/hub.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './_components/home/home.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    GameComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ])
  ],
  providers: [HubService],
  bootstrap: [AppComponent]
})
export class AppModule { }
