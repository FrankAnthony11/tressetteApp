import { Component, OnInit, OnDestroy } from '@angular/core';
import { Game } from 'app/_models/game';
import { HubService } from 'app/_services/hub.service';
import { takeWhile } from 'rxjs/operators';

@Component({
  selector: 'app-game-spectators',
  templateUrl: './game-spectators.component.html',
  styleUrls: ['./game-spectators.component.css']
})
export class GameSpectatorsComponent implements OnInit, OnDestroy {
  private _isAlive = true;

  game: Game;

  constructor(private _hubService: HubService) { }

  ngOnDestroy(): void {
    this._isAlive = false;
  }

  ngOnInit() {
    this._hubService.ActiveGame.pipe(takeWhile(() => this._isAlive)).subscribe(game => {
      this.game = game;
    });
  }
}
