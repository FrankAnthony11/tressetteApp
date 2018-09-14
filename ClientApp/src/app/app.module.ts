import { GameComponent } from './_components/game/game.component';
import { HubService } from './_services/hub.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './_components/home/home.component';
import { WaitingRoomComponent } from './_components/waiting-room/waiting-room.component';
import { WaitingRoomGuard } from './_guards/waiting-room.guard';

@NgModule({
  declarations: [AppComponent, HomeComponent, GameComponent, WaitingRoomComponent],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'waitingRoom', component: WaitingRoomComponent, canActivate:[WaitingRoomGuard] }
    ])
  ],
  providers: [HubService, WaitingRoomGuard],
  bootstrap: [AppComponent]
})
export class AppModule {}
