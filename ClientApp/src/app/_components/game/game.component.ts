import { Card } from './../../_models/card';
import { Game } from './../../_models/game';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../../_models/user';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {

   currentUser:User;

  game: Game;

  constructor(private _hubService: HubService, private _router: Router) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
      if (this.game.cardsDrew.length == 2) {
        setTimeout(() => {
          this.game.cardsDrew = [];
          this.game.cardsPlayed=[];
        }, 30000000);
      }
      if (this.game.gameEnded) {
        setTimeout(() => {
          alert(
            `You ${this.getPlayer().calculatedPoints > this.getOpponent().calculatedPoints ? 'Won' : 'Lost'}! Your points: ${
              this.getPlayer().calculatedPoints
            }, Opponent: ${this.getOpponent().calculatedPoints}`
          );
        }, 500);
      }
    });
    this._hubService.CurrentUser.subscribe(user=>{
      this.currentUser=user;
    })
  }

  getPlayer() {
    if (this.currentUser.connectionId == this.game.player1.user.connectionId) {
      return this.game.player1;
    } else {
      return this.game.player2;
    }
  }
  getOpponent() {
    if (this.currentUser.connectionId != this.game.player1.user.connectionId) {
      return this.game.player1;
    } else {
      return this.game.player2;
    }
  }

  makeMove(card) {
    this._hubService.MakeMove(card);
  }

  exitGame() {
    var cfrm = confirm('Really exit game?');
    if (cfrm) {
      this._hubService.ExitGame();
    }
  }
}
