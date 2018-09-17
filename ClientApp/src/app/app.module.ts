import { GameGuard } from './_guards/game.guard';
import { GameComponent } from './_components/game/game.component';
import { HubService } from './_services/hub.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import {NgbModule} from '@ng-bootstrap/ng-bootstrap';

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
    NgbModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'waitingRoom', component: WaitingRoomComponent, canActivate:[WaitingRoomGuard] },
      { path: 'game', component: GameComponent, canActivate:[GameGuard] }
    ])
  ],
  providers: [HubService, WaitingRoomGuard,GameGuard],
  bootstrap: [AppComponent]
})
export class AppModule {}
