import { Router } from '@angular/router';
import { Player } from './../../_models/player';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { User } from 'app/_models/user';
import { HubService } from 'app/_services/hub.service';
import { Game } from 'app/_models/game';
import { takeWhile } from 'rxjs/operators';

@Component({
  selector: 'app-waiting-room',
  templateUrl: './waiting-room.component.html',
  styleUrls: ['./waiting-room.component.css']
})
export class WaitingRoomComponent implements OnInit, OnDestroy {
  private _isAlive = true;

  activeGame: Game;
  password: string;
  currentUser: User;

  constructor(private _hubService: HubService, private _router: Router) { }

  ngOnDestroy(): void {
    this._isAlive = false;
  }

  ngOnInit() {
    this._hubService.ActiveGame.pipe(takeWhile(() => this._isAlive)).subscribe(room => {
      this.activeGame = room;
    });

    this._hubService.CurrentUser.pipe(takeWhile(() => this._isAlive)).subscribe(user => {
      this.currentUser = user;
    });
  }

  leaveWaitingRoom() {
    this._hubService.ExitGame();
    this._router.navigate(['/']);
  }

  joinGame() {
    this._hubService.JoinGame(this.activeGame.gameSetup.id, '');
  }

  userIsSpectator() {
    var exists = this.activeGame.spectators.find(spectator => {
      return spectator.name == this.currentUser.name;
    });
    return exists != null;
  }

  startGame() {
    this._hubService.StartGame();
  }

  setRoomPassword() {
    if (!this.password) return;
    this._hubService.SetGamePassword(this.activeGame.gameSetup.id, this.password);
  }

  kickPlayerFromGame(player: Player) {
    let cfrm = confirm('Really kick this player? ' + player.user.name);
    if (cfrm) this._hubService.KickUSerFromGame(player.user);
  }

  getDeckTypeName() {
    return this.activeGame.gameSetup.typeOfDeck == 1 ? "Napoletane" : "Triestine";
  }

  setDeckType(typeOfDeck: number) {
    this._hubService.SetGameTypeOfDeck(this.activeGame.gameSetup.id, typeOfDeck);
  }
}
