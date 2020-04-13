import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GameMode, TypeOfDeck } from './../../../_models/enums';
import { HubService } from './../../../_services/hub.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { takeWhile } from 'rxjs/operators';
import { Game } from 'app/_models/game';

@Component({
  selector: 'app-game-setup',
  templateUrl: './game-setup.component.html',
  styleUrls: ['./game-setup.component.css']
})
export class GameSetupComponent implements OnInit, OnDestroy {
  private _isAlive = true;

  activeGame: Game;

  playUntilPoints: number = 21;
  expectedNumberOfPlayers: number = 2;
  gameMode: GameMode = GameMode.plain;
  deckType: TypeOfDeck = TypeOfDeck.triestino
  password: string = "";

  constructor(private _hubService: HubService, private _activeModal: NgbActiveModal) { }

  ngOnDestroy(): void {
    this._isAlive = false;
  }

  ngOnInit() {

    this._hubService.ActiveGame.pipe(takeWhile(() => this._isAlive)).subscribe(game => {
      this.activeGame = game;
    });

    let storedPlayUntilPoints = localStorage.getItem("playUntilPoints");
    if (storedPlayUntilPoints)
      this.playUntilPoints = JSON.parse(storedPlayUntilPoints);
    let storedExpectedNumberOfPlayers = localStorage.getItem("expectedNumberOfPlayers");
    if (storedExpectedNumberOfPlayers)
      this.expectedNumberOfPlayers = JSON.parse(storedExpectedNumberOfPlayers);
    let storedGameMode = localStorage.getItem("gameMode");
    if (storedGameMode)
      this.gameMode = JSON.parse(storedGameMode);
    let storedDeckType = localStorage.getItem("deckType");
    if (storedDeckType)
      this.deckType = JSON.parse(storedDeckType);

  }

  createGame() {
    localStorage.setItem('playUntilPoints', JSON.stringify(this.playUntilPoints));
    localStorage.setItem('expectedNumberOfPlayers', JSON.stringify(this.expectedNumberOfPlayers));
    localStorage.setItem('gameMode', JSON.stringify(this.gameMode));
    localStorage.setItem('deckType', JSON.stringify(this.deckType));


    if(this.activeGame==null){
      this._hubService.CreateGame(this.playUntilPoints, this.expectedNumberOfPlayers, this.gameMode, this.deckType, this.password);
    }else{
      this._hubService.UpdateGame(this.activeGame.gameSetup.id, this.playUntilPoints, this.expectedNumberOfPlayers, this.gameMode, this.deckType, this.password);
    }

    this.closeModal();
  }

  gameModeChanged() {
    if (this.gameMode == GameMode.plain) {
      if (this.playUntilPoints == 51)
        this.playUntilPoints = 21;
      if (this.expectedNumberOfPlayers == 3 || this.expectedNumberOfPlayers == 5)
        this.expectedNumberOfPlayers = 2;
    } else {
      if (this.playUntilPoints == 21 || this.playUntilPoints == 41)
        this.playUntilPoints = 51;
    }
  }

  closeModal() {
    this._activeModal.dismiss();
  }



}
