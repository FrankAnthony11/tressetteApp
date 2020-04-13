import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Player } from './../../_models/player';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { User } from 'app/_models/user';
import { HubService } from 'app/_services/hub.service';
import { Game } from 'app/_models/game';
import { takeWhile } from 'rxjs/operators';
import { GameMode } from 'app/_models/enums';
import { GameSetupComponent } from '../_modals/game-setup/game-setup.component';

@Component({
  selector: 'app-waiting-room',
  templateUrl: './waiting-room.component.html',
  styleUrls: ['./waiting-room.component.css']
})
export class WaitingRoomComponent implements OnInit, OnDestroy {
  private _isAlive = true;

  activeGame: Game;
  currentUser: User;

  constructor(private _hubService: HubService, private _router: Router, private _modalService: NgbModal) { }

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
  updateGame() {
    var userSettingsModal = this._modalService.open(GameSetupComponent, { backdrop: 'static', keyboard: false });
    return userSettingsModal;
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

  kickPlayerFromGame(player: Player) {
    let cfrm = confirm('Really kick this player? ' + player.user.name);
    if (cfrm) this._hubService.KickUSerFromGame(player.user);
  }

  getDeckTypeName() {
    return this.activeGame.gameSetup.typeOfDeck == 1 ? "Napoletane" : "Triestine";
  }


  getGameModeName() {
    return this.activeGame.gameSetup.gameMode == GameMode.plain ? "Standard" : "Evasion";
  }

}
