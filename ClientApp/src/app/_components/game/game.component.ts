import { Game } from './../../_models/game';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {
  game: Game;

  constructor(private _hubService: HubService) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
    });
  }

  getPlayer() {
    if (this._hubService.connectionId == this.game.player1.connectionId) {
      return this.game.player1;
    } else {
      return this.game.player2;
    }
  }

  makeMove(card) {
    this._hubService.MakeMove(card);
  }
}
