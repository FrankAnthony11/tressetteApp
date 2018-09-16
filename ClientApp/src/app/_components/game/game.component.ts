import { Card } from './../../_models/card';
import { Game } from './../../_models/game';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {
  game: Game;
  cardsDrew: Card[] = new Array();

  constructor(private _hubService: HubService, private _router: Router) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
      if (game.cardsDrew.length == 2) {
        this.cardsDrew = game.cardsDrew;
        setTimeout(() => {
          this.cardsDrew = [];
        }, 1500);
      }
      if (game.gameEnded) {
        setTimeout(() => {
          let gameOutput = '';
          if (this.getPlayer().calculatedPoints > this.getOpponent().calculatedPoints) {
            gameOutput = 'Won';
          } else {
            gameOutput = 'Lost';
          }
          alert(`You ${gameOutput}! Your points: ${this.getPlayer().calculatedPoints}, Opponent: ${this.getOpponent().calculatedPoints}`);
          this._router.navigateByUrl('/');
        }, 500);
      }
    });
  }

  getPlayer() {
    if (this._hubService.connectionId == this.game.player1.connectionId) {
      return this.game.player1;
    } else {
      return this.game.player2;
    }
  }
  getOpponent() {
    if (this._hubService.connectionId != this.game.player1.connectionId) {
      return this.game.player1;
    } else {
      return this.game.player2;
    }
  }

  makeMove(card) {
    this._hubService.MakeMove(card);
  }
}
