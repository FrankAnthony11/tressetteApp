import { GameTabsComponent } from './_components/_game/game-tabs/game-tabs.component';
import { GameSpectatorsComponent } from './_components/_game/game-spectators/game-spectators.component';
import { ActivePlayersComponent } from './_components/_lobbyInfo/active-players/active-players.component';
import { GameInfoTemplateComponent } from './_components/_game/game-info-template/game-info-template.component';
import { CardWithPlayerNameComponent } from './_components/_cards/card-with-player-name/card-with-player-name.component';
import { AllChatComponent } from './_components/all-chat/all-chat.component';
import { GameDeactivateGuard } from './_guards/game-deactivate.guard';
import { GameChatComponent } from './_components/game-chat/game-chat.component';
import { GameGuard } from './_guards/game.guard';
import { GameComponent } from './_components/game/game.component';
import { HubService } from './_services/hub.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { ToastrModule } from 'ngx-toastr';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SidebarModule } from 'ng-sidebar';

import { AppComponent } from './app.component';
import { HomeComponent } from './_components/home/home.component';
import { WaitingRoomComponent } from './_components/waiting-room/waiting-room.component';
import { WaitingRoomGuard } from './_guards/waiting-room.guard';
import { RunningGamesComponent } from './_components/_lobbyInfo/running-games/running-games.component';
import { GameSetupComponent } from './_components/_modals/game-setup/game-setup.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    CardWithPlayerNameComponent,
    GameInfoTemplateComponent,
    GameComponent,
    WaitingRoomComponent,
    ActivePlayersComponent,
    RunningGamesComponent,
    GameChatComponent,
    GameSpectatorsComponent,
    AllChatComponent,
    GameTabsComponent,
    GameSetupComponent,

  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    BrowserAnimationsModule,
    NgbModule,
    SidebarModule.forRoot(),
    ToastrModule.forRoot(),
    RouterModule.forRoot([
      { path: '', component: HomeComponent },
      { path: 'waitingRoom', component: WaitingRoomComponent, canActivate: [WaitingRoomGuard] },
      { path: 'game', component: GameComponent, canActivate: [GameGuard], canDeactivate: [GameDeactivateGuard] },
      { path: '**', redirectTo: '/' }
    ])
  ],
  providers: [HubService, WaitingRoomGuard, GameGuard, GameDeactivateGuard],
  bootstrap: [AppComponent],
  entryComponents:[
    GameSetupComponent
  ]
})
export class AppModule {}
